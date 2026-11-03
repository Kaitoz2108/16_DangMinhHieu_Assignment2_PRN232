using System;
using System.Collections.Generic;
using FUNewsManagement_BLL.DTOs;
using FUNewsManagement_DAL.Entities;

namespace FUNewsManagement_BLL.Services;

public interface INewsArticleService
{
    List<NewsArticle> GetAllEntities(bool includeInactive = true);
    List<NewsArticleDto> GetAll(bool includeInactive = true);
    List<NewsArticleDto> GetActiveArticles();
    NewsArticleDto? GetById(string id);
    NewsArticle? GetEntityById(string id);
    List<NewsArticleDto> GetByCreator(short accountId);
    List<NewsArticleDto> GetReport(DateTime? startDate, DateTime? endDate);
    NewsArticleDto Create(NewsArticleDto dto, short? currentUserId);
    NewsArticleDto Update(string id, NewsArticleDto dto, short? currentUserId);
    (bool Success, string Message) Delete(string id);
}
