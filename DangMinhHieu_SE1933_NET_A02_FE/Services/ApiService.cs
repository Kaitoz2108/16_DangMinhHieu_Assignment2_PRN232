using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using FUNewsManagement_BLL.DTOs;
using FUNewsManagement_DAL.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace FUNewsManagement_Client.Services;

public class ApiService : IApiService
{
    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly JsonSerializerOptions _jsonOptions;

    public ApiService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _httpContextAccessor = httpContextAccessor;

        var baseUrl = configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5173";
        if (!baseUrl.EndsWith("/")) baseUrl += "/";
        _httpClient.BaseAddress = new Uri(baseUrl);

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    private void AddAuthorizationHeader()
    {
        var token = _httpContextAccessor.HttpContext?.Session.GetString("JWToken")
            ?? _httpContextAccessor.HttpContext?.User.FindFirst("JWToken")?.Value;

        if (!string.IsNullOrEmpty(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        else
        {
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }
    }

    private void CheckStatusCode(HttpResponseMessage response)
    {
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            throw new UnauthorizedApiException();
        }
        if (response.StatusCode == HttpStatusCode.Forbidden)
        {
            throw new ForbiddenApiException();
        }
    }

    private async Task<T?> ReadJsonSafeAsync<T>(HttpContent content)
    {
        try
        {
            var str = await content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(str))
            {
                return default;
            }
            return JsonSerializer.Deserialize<T>(str, _jsonOptions);
        }
        catch
        {
            return default;
        }
    }

    private List<T> ParseODataOrList<T>(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.ValueKind == JsonValueKind.Object && doc.RootElement.TryGetProperty("value", out var valueElem))
            {
                return JsonSerializer.Deserialize<List<T>>(valueElem.GetRawText(), _jsonOptions) ?? new List<T>();
            }
            if (doc.RootElement.ValueKind == JsonValueKind.Array)
            {
                return JsonSerializer.Deserialize<List<T>>(json, _jsonOptions) ?? new List<T>();
            }
        }
        catch
        {
        }
        return new List<T>();
    }

    // --- AUTH ---
    public async Task<(bool Success, LoginResponse? Data, string? ErrorMessage)> LoginAsync(LoginRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/Auth/login", request);
            if (response.IsSuccessStatusCode)
            {
                var data = await ReadJsonSafeAsync<LoginResponse>(response.Content);
                return (true, data, null);
            }

            var errJson = await response.Content.ReadAsStringAsync();
            var errMsg = ExtractErrorMessage(errJson, "Invalid email or password.");
            return (false, null, errMsg);
        }
        catch (Exception ex)
        {
            return (false, null, $"Connection error: {ex.Message}");
        }
    }

    public async Task<(bool Success, SystemAccountDto? Data, string? ErrorMessage)> GetProfileAsync()
    {
        AddAuthorizationHeader();
        try
        {
            var response = await _httpClient.GetAsync("api/Auth/profile");
            CheckStatusCode(response);

            if (response.IsSuccessStatusCode)
            {
                var data = await ReadJsonSafeAsync<SystemAccountDto>(response.Content);
                return (true, data, null);
            }
            return (false, null, "Failed to retrieve profile.");
        }
        catch (UnauthorizedApiException) { throw; }
        catch (ForbiddenApiException) { throw; }
        catch (Exception ex)
        {
            return (false, null, ex.Message);
        }
    }

    public async Task<(bool Success, string? ErrorMessage)> UpdateProfileAsync(ProfileUpdateRequest request)
    {
        AddAuthorizationHeader();
        try
        {
            var response = await _httpClient.PutAsJsonAsync("api/Auth/profile", request);
            CheckStatusCode(response);

            if (response.IsSuccessStatusCode)
            {
                return (true, null);
            }
            var errJson = await response.Content.ReadAsStringAsync();
            return (false, ExtractErrorMessage(errJson, "Failed to update profile."));
        }
        catch (UnauthorizedApiException) { throw; }
        catch (ForbiddenApiException) { throw; }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    // --- CATEGORIES ---
    public async Task<List<Category>> GetCategoriesAsync(string? odataQuery = null)
    {
        AddAuthorizationHeader();
        var url = string.IsNullOrWhiteSpace(odataQuery) ? "odata/Categories" : $"odata/Categories?{odataQuery}";
        try
        {
            var response = await _httpClient.GetAsync(url);
            CheckStatusCode(response);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return ParseODataOrList<Category>(json);
            }
        }
        catch (UnauthorizedApiException) { throw; }
        catch (ForbiddenApiException) { throw; }
        catch
        {
        }
        return new List<Category>();
    }

    public async Task<Category?> GetCategoryByIdAsync(short id)
    {
        AddAuthorizationHeader();
        try
        {
            var response = await _httpClient.GetAsync($"odata/Categories({id})");
            CheckStatusCode(response);

            if (response.IsSuccessStatusCode)
            {
                return await ReadJsonSafeAsync<Category>(response.Content);
            }
        }
        catch (UnauthorizedApiException) { throw; }
        catch (ForbiddenApiException) { throw; }
        catch
        {
        }
        return null;
    }

    public async Task<(bool Success, Category? Data, string? ErrorMessage)> CreateCategoryAsync(Category category)
    {
        AddAuthorizationHeader();
        try
        {
            var response = await _httpClient.PostAsJsonAsync("odata/Categories", category);
            CheckStatusCode(response);

            if (response.IsSuccessStatusCode)
            {
                var created = await ReadJsonSafeAsync<Category>(response.Content);
                return (true, created ?? category, null);
            }
            var err = await response.Content.ReadAsStringAsync();
            return (false, null, ExtractErrorMessage(err, "Failed to create category."));
        }
        catch (UnauthorizedApiException) { throw; }
        catch (ForbiddenApiException) { throw; }
        catch (Exception ex)
        {
            return (false, null, ex.Message);
        }
    }

    public async Task<(bool Success, Category? Data, string? ErrorMessage)> UpdateCategoryAsync(short id, Category category)
    {
        AddAuthorizationHeader();
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"odata/Categories({id})", category);
            CheckStatusCode(response);

            if (response.IsSuccessStatusCode)
            {
                var updated = await ReadJsonSafeAsync<Category>(response.Content);
                return (true, updated ?? category, null);
            }
            var err = await response.Content.ReadAsStringAsync();
            return (false, null, ExtractErrorMessage(err, "Failed to update category."));
        }
        catch (UnauthorizedApiException) { throw; }
        catch (ForbiddenApiException) { throw; }
        catch (Exception ex)
        {
            return (false, null, ex.Message);
        }
    }

    public async Task<(bool Success, string? ErrorMessage)> DeleteCategoryAsync(short id)
    {
        AddAuthorizationHeader();
        try
        {
            var response = await _httpClient.DeleteAsync($"odata/Categories({id})");
            CheckStatusCode(response);

            if (response.IsSuccessStatusCode)
            {
                return (true, null);
            }
            var err = await response.Content.ReadAsStringAsync();
            return (false, ExtractErrorMessage(err, "Failed to delete category."));
        }
        catch (UnauthorizedApiException) { throw; }
        catch (ForbiddenApiException) { throw; }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    // --- ACCOUNTS ---
    public async Task<List<SystemAccount>> GetAccountsAsync(string? odataQuery = null)
    {
        AddAuthorizationHeader();
        var url = string.IsNullOrWhiteSpace(odataQuery) ? "odata/SystemAccounts" : $"odata/SystemAccounts?{odataQuery}";
        try
        {
            var response = await _httpClient.GetAsync(url);
            CheckStatusCode(response);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return ParseODataOrList<SystemAccount>(json);
            }
        }
        catch (UnauthorizedApiException) { throw; }
        catch (ForbiddenApiException) { throw; }
        catch
        {
        }
        return new List<SystemAccount>();
    }

    public async Task<SystemAccount?> GetAccountByIdAsync(short id)
    {
        AddAuthorizationHeader();
        try
        {
            var response = await _httpClient.GetAsync($"odata/SystemAccounts({id})");
            CheckStatusCode(response);

            if (response.IsSuccessStatusCode)
            {
                return await ReadJsonSafeAsync<SystemAccount>(response.Content);
            }
        }
        catch (UnauthorizedApiException) { throw; }
        catch (ForbiddenApiException) { throw; }
        catch
        {
        }
        return null;
    }

    public async Task<(bool Success, SystemAccountDto? Data, string? ErrorMessage)> CreateAccountAsync(SystemAccountDto account)
    {
        AddAuthorizationHeader();
        try
        {
            var response = await _httpClient.PostAsJsonAsync("odata/SystemAccounts", account);
            CheckStatusCode(response);

            if (response.IsSuccessStatusCode)
            {
                var created = await ReadJsonSafeAsync<SystemAccountDto>(response.Content);
                return (true, created ?? account, null);
            }
            var err = await response.Content.ReadAsStringAsync();
            return (false, null, ExtractErrorMessage(err, "Failed to create account."));
        }
        catch (UnauthorizedApiException) { throw; }
        catch (ForbiddenApiException) { throw; }
        catch (Exception ex)
        {
            return (false, null, ex.Message);
        }
    }

    public async Task<(bool Success, SystemAccountDto? Data, string? ErrorMessage)> UpdateAccountAsync(short id, SystemAccountDto account)
    {
        AddAuthorizationHeader();
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"odata/SystemAccounts({id})", account);
            CheckStatusCode(response);

            if (response.IsSuccessStatusCode)
            {
                var updated = await ReadJsonSafeAsync<SystemAccountDto>(response.Content);
                return (true, updated ?? account, null);
            }
            var err = await response.Content.ReadAsStringAsync();
            return (false, null, ExtractErrorMessage(err, "Failed to update account."));
        }
        catch (UnauthorizedApiException) { throw; }
        catch (ForbiddenApiException) { throw; }
        catch (Exception ex)
        {
            return (false, null, ex.Message);
        }
    }

    public async Task<(bool Success, string? ErrorMessage)> DeleteAccountAsync(short id)
    {
        AddAuthorizationHeader();
        try
        {
            var response = await _httpClient.DeleteAsync($"odata/SystemAccounts({id})");
            CheckStatusCode(response);

            if (response.IsSuccessStatusCode)
            {
                return (true, null);
            }
            var err = await response.Content.ReadAsStringAsync();
            return (false, ExtractErrorMessage(err, "Failed to delete account."));
        }
        catch (UnauthorizedApiException) { throw; }
        catch (ForbiddenApiException) { throw; }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    // --- NEWS ARTICLES ---
    public async Task<List<NewsArticle>> GetNewsArticlesAsync(string? odataQuery = null)
    {
        AddAuthorizationHeader();
        var url = string.IsNullOrWhiteSpace(odataQuery) ? "odata/NewsArticles" : $"odata/NewsArticles?{odataQuery}";
        try
        {
            var response = await _httpClient.GetAsync(url);
            CheckStatusCode(response);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return ParseODataOrList<NewsArticle>(json);
            }
        }
        catch (UnauthorizedApiException) { throw; }
        catch (ForbiddenApiException) { throw; }
        catch
        {
        }
        return new List<NewsArticle>();
    }

    public async Task<NewsArticle?> GetNewsArticleByIdAsync(string id)
    {
        AddAuthorizationHeader();
        try
        {
            var response = await _httpClient.GetAsync($"odata/NewsArticles('{id}')");
            CheckStatusCode(response);

            if (response.IsSuccessStatusCode)
            {
                return await ReadJsonSafeAsync<NewsArticle>(response.Content);
            }
        }
        catch (UnauthorizedApiException) { throw; }
        catch (ForbiddenApiException) { throw; }
        catch
        {
        }
        return null;
    }

    public async Task<(bool Success, NewsArticle? Data, string? ErrorMessage)> CreateNewsArticleAsync(NewsArticleDto dto)
    {
        AddAuthorizationHeader();
        try
        {
            var response = await _httpClient.PostAsJsonAsync("odata/NewsArticles", dto);
            CheckStatusCode(response);

            if (response.IsSuccessStatusCode)
            {
                var created = await ReadJsonSafeAsync<NewsArticle>(response.Content);
                return (true, created, null);
            }
            var err = await response.Content.ReadAsStringAsync();
            return (false, null, ExtractErrorMessage(err, "Failed to create news article."));
        }
        catch (UnauthorizedApiException) { throw; }
        catch (ForbiddenApiException) { throw; }
        catch (Exception ex)
        {
            return (false, null, ex.Message);
        }
    }

    public async Task<(bool Success, NewsArticle? Data, string? ErrorMessage)> UpdateNewsArticleAsync(string id, NewsArticleDto dto)
    {
        AddAuthorizationHeader();
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"odata/NewsArticles('{id}')", dto);
            CheckStatusCode(response);

            if (response.IsSuccessStatusCode)
            {
                var updated = await ReadJsonSafeAsync<NewsArticle>(response.Content);
                return (true, updated, null);
            }
            var err = await response.Content.ReadAsStringAsync();
            return (false, null, ExtractErrorMessage(err, "Failed to update news article."));
        }
        catch (UnauthorizedApiException) { throw; }
        catch (ForbiddenApiException) { throw; }
        catch (Exception ex)
        {
            return (false, null, ex.Message);
        }
    }

    public async Task<(bool Success, string? ErrorMessage)> DeleteNewsArticleAsync(string id)
    {
        AddAuthorizationHeader();
        try
        {
            var response = await _httpClient.DeleteAsync($"odata/NewsArticles('{id}')");
            CheckStatusCode(response);

            if (response.IsSuccessStatusCode)
            {
                return (true, null);
            }
            var err = await response.Content.ReadAsStringAsync();
            return (false, ExtractErrorMessage(err, "Failed to delete news article."));
        }
        catch (UnauthorizedApiException) { throw; }
        catch (ForbiddenApiException) { throw; }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public async Task<List<NewsArticle>> GetMyHistoryAsync()
    {
        AddAuthorizationHeader();
        try
        {
            var response = await _httpClient.GetAsync("api/NewsArticles/history");
            CheckStatusCode(response);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return ParseODataOrList<NewsArticle>(json);
            }
        }
        catch (UnauthorizedApiException) { throw; }
        catch (ForbiddenApiException) { throw; }
        catch
        {
        }
        return new List<NewsArticle>();
    }

    public async Task<List<NewsArticle>> GetReportAsync(DateTime? startDate, DateTime? endDate)
    {
        AddAuthorizationHeader();
        var queryParams = new List<string>();
        if (startDate.HasValue) queryParams.Add($"startDate={startDate.Value:yyyy-MM-dd}");
        if (endDate.HasValue) queryParams.Add($"endDate={endDate.Value:yyyy-MM-dd}");
        var qs = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";

        try
        {
            var response = await _httpClient.GetAsync($"api/Reports{qs}");
            CheckStatusCode(response);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return ParseODataOrList<NewsArticle>(json);
            }
        }
        catch (UnauthorizedApiException) { throw; }
        catch (ForbiddenApiException) { throw; }
        catch
        {
        }
        return new List<NewsArticle>();
    }

    // --- TAGS ---
    public async Task<List<Tag>> GetTagsAsync()
    {
        AddAuthorizationHeader();
        try
        {
            var response = await _httpClient.GetAsync("odata/Tags");
            CheckStatusCode(response);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return ParseODataOrList<Tag>(json);
            }
        }
        catch (UnauthorizedApiException) { throw; }
        catch (ForbiddenApiException) { throw; }
        catch
        {
        }
        return new List<Tag>();
    }

    private string ExtractErrorMessage(string json, string defaultMsg)
    {
        if (string.IsNullOrWhiteSpace(json)) return defaultMsg;
        try
        {
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("message", out var msgProp))
            {
                return msgProp.GetString() ?? defaultMsg;
            }
            if (doc.RootElement.TryGetProperty("error", out var errProp))
            {
                if (errProp.ValueKind == JsonValueKind.Object && errProp.TryGetProperty("message", out var subMsg))
                {
                    return subMsg.GetString() ?? defaultMsg;
                }
                return errProp.GetString() ?? defaultMsg;
            }
        }
        catch
        {
        }
        return json.Length > 200 ? json.Substring(0, 200) : json;
    }
}
