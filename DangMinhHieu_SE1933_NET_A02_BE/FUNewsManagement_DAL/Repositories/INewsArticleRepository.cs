using System;
using System.Collections.Generic;
using FUNewsManagement_DAL.Entities;

namespace FUNewsManagement_DAL.Repositories;

public interface INewsArticleRepository
{
    List<NewsArticle> GetAll(bool includeInactive = true);
    List<NewsArticle> GetActiveArticles();
    NewsArticle? GetById(string id);
    List<NewsArticle> GetByCreator(short accountId);
    List<NewsArticle> GetReport(DateTime? startDate, DateTime? endDate);
    NewsArticle Create(NewsArticle article, List<int>? tagIds = null);
    NewsArticle Update(NewsArticle article, List<int>? tagIds = null);
    (bool Success, string Message) Delete(string id);
}
