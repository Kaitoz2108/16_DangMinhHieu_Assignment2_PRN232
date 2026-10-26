using System;
using FUNewsManagement_BLL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FUNewsManagement_API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class ReportsController : ControllerBase
{
    private readonly INewsArticleService _newsArticleService;

    public ReportsController(INewsArticleService newsArticleService)
    {
        _newsArticleService = newsArticleService;
    }

    [HttpGet]
    public IActionResult GetReport([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        var report = _newsArticleService.GetReport(startDate, endDate);
        return Ok(report);
    }
}
