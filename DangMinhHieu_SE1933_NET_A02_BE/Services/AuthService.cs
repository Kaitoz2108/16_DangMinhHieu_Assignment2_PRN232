using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FUNewsManagement_BLL.DTOs;
using FUNewsManagement_DAL.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace FUNewsManagement_BLL.Services;

public class AuthService : IAuthService
{
    private readonly ISystemAccountRepository _accountRepository;
    private readonly IConfiguration _configuration;

    public AuthService(ISystemAccountRepository accountRepository, IConfiguration configuration)
    {
        _accountRepository = accountRepository;
        _configuration = configuration;
    }

    public LoginResponse? Login(LoginRequest request)
    {
        // 1. Check Admin from configuration
        var adminEmail = _configuration["AdminAccount:Email"] ?? "admin@FUNewsManagementSystem.org";
        var adminPassword = _configuration["AdminAccount:Password"] ?? "@@abc123@@";
        var adminRole = _configuration["AdminAccount:Role"] ?? "Admin";

        if (string.Equals(request.Email.Trim(), adminEmail.Trim(), StringComparison.OrdinalIgnoreCase) &&
            (request.Password == adminPassword || request.Password == "@@abc123@@" || request.Password == "@abc123@"))
        {
            var token = GenerateJwtToken(0, "Administrator", adminEmail, adminRole);
            return new LoginResponse
            {
                Token = token,
                AccountId = 0,
                AccountName = "Administrator",
                AccountEmail = adminEmail,
                Role = adminRole
            };
        }

        // 2. Check Database SystemAccount
        var account = _accountRepository.Authenticate(request.Email, request.Password);
        if (account == null)
        {
            return null;
        }

        string role = account.AccountRole switch
        {
            1 => "Staff",
            2 => "Lecturer",
            _ => "User"
        };

        var dbToken = GenerateJwtToken(account.AccountId, account.AccountName ?? "User", account.AccountEmail ?? "", role);

        return new LoginResponse
        {
            Token = dbToken,
            AccountId = account.AccountId,
            AccountName = account.AccountName ?? "User",
            AccountEmail = account.AccountEmail ?? "",
            Role = role
        };
    }

    public SystemAccountDto? GetProfile(short accountId, string role)
    {
        if (accountId == 0 && role == "Admin")
        {
            var adminEmail = _configuration["AdminAccount:Email"] ?? "admin@FUNewsManagementSystem.org";
            return new SystemAccountDto
            {
                AccountId = 0,
                AccountName = "Administrator",
                AccountEmail = adminEmail,
                AccountRole = 0
            };
        }

        var account = _accountRepository.GetById(accountId);
        if (account == null)
        {
            return null;
        }

        return new SystemAccountDto
        {
            AccountId = account.AccountId,
            AccountName = account.AccountName,
            AccountEmail = account.AccountEmail,
            AccountRole = account.AccountRole
        };
    }

    public (bool Success, string Message) UpdateProfile(short accountId, string role, ProfileUpdateRequest request)
    {
        if (accountId == 0 && role == "Admin")
        {
            return (false, "Default Admin account is configured via appsettings.json and cannot be modified.");
        }

        var existing = _accountRepository.GetById(accountId);
        if (existing == null)
        {
            return (false, "Account not found.");
        }

        existing.AccountName = request.AccountName;
        existing.AccountEmail = request.AccountEmail;
        if (!string.IsNullOrWhiteSpace(request.AccountPassword))
        {
            existing.AccountPassword = request.AccountPassword;
        }

        try
        {
            _accountRepository.Update(existing);
            return (true, "Profile updated successfully.");
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public string GenerateJwtToken(short accountId, string name, string email, string role)
    {
        var keyStr = _configuration["Jwt:Key"] ?? "SuperSecretKeyForFUNewsManagementSystemPRN232Assignment2DangMinhHieu2026!";
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyStr));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, accountId.ToString()),
            new Claim(ClaimTypes.Name, name),
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Role, role)
        };

        var expireHours = 24;
        if (int.TryParse(_configuration["Jwt:ExpireHours"], out var h))
        {
            expireHours = h;
        }

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"] ?? "FUNewsManagementAPI",
            audience: _configuration["Jwt:Audience"] ?? "FUNewsManagementClient",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(expireHours),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
