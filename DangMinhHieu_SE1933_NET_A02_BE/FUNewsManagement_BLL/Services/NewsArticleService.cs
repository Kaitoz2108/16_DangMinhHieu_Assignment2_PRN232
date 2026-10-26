using System;
using System.Collections.Generic;
using System.Linq;
using FUNewsManagement_BLL.DTOs;
using FUNewsManagement_DAL.Entities;
using FUNewsManagement_DAL.Repositories;

namespace FUNewsManagement_BLL.Services;

public class NewsArticleService : INewsArticleService
{
    private readonly INewsArticleRepository _newsArticleRepository;

    public NewsArticleService(INewsArticleRepository newsArticleRepository)
    {
        _newsArticleRepository = newsArticleRepository;
    }

    public List<NewsArticle> GetAllEntities(bool includeInactive = true)
    {
        return _newsArticleRepository.GetAll(includeInactive);
    }

    public List<NewsArticleDto> GetAll(bool includeInactive = true)
    {
        var articles = _newsArticleRepository.GetAll(includeInactive);
        return articles.Select(MapToDto).ToList();
    }

    public List<NewsArticleDto> GetActiveArticles()
    {
        var articles = _newsArticleRepository.GetActiveArticles();
        return articles.Select(MapToDto).ToList();
    }

    public NewsArticleDto? GetById(string id)
    {
        var article = _newsArticleRepository.GetById(id);
        return article == null ? null : MapToDto(article);
    }

    public NewsArticle? GetEntityById(string id)
    {
        return _newsArticleRepository.GetById(id);
    }

    public List<NewsArticleDto> GetByCreator(short accountId)
    {
        var articles = _newsArticleRepository.GetByCreator(accountId);
        return articles.Select(MapToDto).ToList();
    }

    public List<NewsArticleDto> GetReport(DateTime? startDate, DateTime? endDate)
    {
        var articles = _newsArticleRepository.GetReport(startDate, endDate);
        return articles.Select(MapToDto).ToList();
    }

    public NewsArticleDto Create(NewsArticleDto dto, short? currentUserId)
    {
        var article = new NewsArticle
        {
            NewsArticleId = dto.NewsArticleId,
            NewsTitle = dto.NewsTitle,
            Headline = dto.Headline,
            CreatedDate = dto.CreatedDate ?? DateTime.Now,
            NewsContent = dto.NewsContent,
            NewsSource = dto.NewsSource,
            CategoryId = dto.CategoryId,
            NewsStatus = dto.NewsStatus ?? true,
            CreatedById = dto.CreatedById ?? currentUserId,
            UpdatedById = null,
            ModifiedDate = null
        };

        var created = _newsArticleRepository.Create(article, dto.TagIds);
        return MapToDto(created);
    }

    public NewsArticleDto Update(string id, NewsArticleDto dto, short? currentUserId)
    {
        var article = new NewsArticle
        {
            NewsArticleId = id,
            NewsTitle = dto.NewsTitle,
            Headline = dto.Headline,
            NewsContent = dto.NewsContent,
            NewsSource = dto.NewsSource,
            CategoryId = dto.CategoryId,
            NewsStatus = dto.NewsStatus,
            UpdatedById = currentUserId ?? dto.UpdatedById,
            ModifiedDate = DateTime.Now
        };

        var updated = _newsArticleRepository.Update(article, dto.TagIds);
        return MapToDto(updated);
    }

    public (bool Success, string Message) Delete(string id)
    {
        return _newsArticleRepository.Delete(id);
    }

    private static NewsArticleDto MapToDto(NewsArticle a)
    {
        return new NewsArticleDto
        {
            NewsArticleId = a.NewsArticleId,
            NewsTitle = a.NewsTitle,
            Headline = a.Headline,
            CreatedDate = a.CreatedDate,
            NewsContent = a.NewsContent,
            NewsSource = a.NewsSource,
            CategoryId = a.CategoryId,
            CategoryName = a.Category?.CategoryName,
            NewsStatus = a.NewsStatus,
            CreatedById = a.CreatedById,
            CreatedByName = a.CreatedBy?.AccountName,
            UpdatedById = a.UpdatedById,
            ModifiedDate = a.ModifiedDate,
            TagIds = a.Tags?.Select(t => t.TagId).ToList() ?? new List<int>(),
            Tags = a.Tags?.Select(t => new TagDto
            {
                TagId = t.TagId,
                TagName = t.TagName ?? string.Empty,
                Note = t.Note
            }).ToList() ?? new List<TagDto>()
        };
    }
}
