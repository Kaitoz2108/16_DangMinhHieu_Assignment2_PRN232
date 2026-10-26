using System;
using System.Security.Claims;
using FUNewsManagement_BLL.DTOs;
using FUNewsManagement_BLL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;

namespace FUNewsManagement_API.Controllers;

public class NewsArticlesController : ODataController
{
    private readonly INewsArticleService _newsArticleService;

    public NewsArticlesController(INewsArticleService newsArticleService)
    {
        _newsArticleService = newsArticleService;
    }

    [EnableQuery]
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Get()
    {
        bool isAuthenticated = User.Identity?.IsAuthenticated == true;
        bool isStaffOrAdmin = isAuthenticated && (User.IsInRole("Staff") || User.IsInRole("Admin"));

        var articles = _newsArticleService.GetAllEntities(includeInactive: isStaffOrAdmin).AsQueryable();
        return Ok(articles);
    }

    [EnableQuery]
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Get([FromRoute] string key)
    {
        var article = _newsArticleService.GetEntityById(key);
        if (article == null)
        {
            return NotFound($"News article with ID {key} not found.");
        }

        bool isAuthenticated = User.Identity?.IsAuthenticated == true;
        bool isStaffOrAdmin = isAuthenticated && (User.IsInRole("Staff") || User.IsInRole("Admin"));

        if (article.NewsStatus != true && !isStaffOrAdmin)
        {
            return NotFound($"News article with ID {key} not found or inactive.");
        }

        return Ok(article);
    }

    [Authorize(Roles = "Staff,Admin")]
    [HttpPost]
    public IActionResult Post([FromBody] NewsArticleDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        short? currentUserId = null;
        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (short.TryParse(idClaim, out var parsedId) && parsedId > 0)
        {
            currentUserId = parsedId;
        }

        try
        {
            var created = _newsArticleService.Create(dto, currentUserId);
            return Created(created);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [Authorize(Roles = "Staff,Admin")]
    [HttpPut]
    public IActionResult Put([FromRoute] string key, [FromBody] NewsArticleDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        short? currentUserId = null;
        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (short.TryParse(idClaim, out var parsedId) && parsedId > 0)
        {
            currentUserId = parsedId;
        }

        try
        {
            var updated = _newsArticleService.Update(key, dto, currentUserId);
            return Ok(updated);
        }
        catch (KeyNotFoundException)
        {
            return NotFound($"News article with ID {key} not found.");
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [Authorize(Roles = "Staff,Admin")]
    [HttpDelete]
    public IActionResult Delete([FromRoute] string key)
    {
        var result = _newsArticleService.Delete(key);
        if (!result.Success)
        {
            return BadRequest(new { message = result.Message });
        }

        return NoContent();
    }

    [Authorize(Roles = "Staff,Admin")]
    [HttpGet("api/NewsArticles/history")]
    public IActionResult GetMyHistory()
    {
        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(idClaim) || !short.TryParse(idClaim, out var accountId))
        {
            return Unauthorized();
        }

        var articles = _newsArticleService.GetByCreator(accountId);
        return Ok(articles);
    }

    [Authorize(Roles = "Staff,Admin")]
    [HttpGet("api/NewsArticles/creator/{accountId}")]
    public IActionResult GetByCreator([FromRoute] short accountId)
    {
        var articles = _newsArticleService.GetByCreator(accountId);
        return Ok(articles);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("api/NewsArticles/report")]
    public IActionResult GetReport([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        var report = _newsArticleService.GetReport(startDate, endDate);
        return Ok(report);
    }
}
