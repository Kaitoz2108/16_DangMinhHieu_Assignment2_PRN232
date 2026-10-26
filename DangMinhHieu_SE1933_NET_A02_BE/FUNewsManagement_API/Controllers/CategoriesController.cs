using System;
using System.Linq;
using FUNewsManagement_BLL.DTOs;
using FUNewsManagement_BLL.Services;
using FUNewsManagement_DAL.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;

namespace FUNewsManagement_API.Controllers;

public class CategoriesController : ODataController
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [EnableQuery]
    [HttpGet]
    public IActionResult Get()
    {
        var categories = _categoryService.GetAllEntities().AsQueryable();
        return Ok(categories);
    }

    [EnableQuery]
    [HttpGet]
    public IActionResult Get([FromRoute] short key)
    {
        var category = _categoryService.GetEntityById(key);
        if (category == null)
        {
            return NotFound($"Category with ID {key} not found.");
        }
        return Ok(category);
    }

    [Authorize(Roles = "Staff,Admin")]
    [HttpPost]
    public IActionResult Post([FromBody] Category category)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var dto = new CategoryDto
            {
                CategoryName = category.CategoryName,
                CategoryDesciption = category.CategoryDesciption,
                ParentCategoryId = category.ParentCategoryId,
                IsActive = category.IsActive ?? true
            };
            var created = _categoryService.Create(dto);
            return Created(created);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [Authorize(Roles = "Staff,Admin")]
    [HttpPut]
    public IActionResult Put([FromRoute] short key, [FromBody] Category category)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var dto = new CategoryDto
            {
                CategoryId = key,
                CategoryName = category.CategoryName,
                CategoryDesciption = category.CategoryDesciption,
                ParentCategoryId = category.ParentCategoryId,
                IsActive = category.IsActive
            };
            var updated = _categoryService.Update(key, dto);
            return Ok(updated);
        }
        catch (KeyNotFoundException)
        {
            return NotFound($"Category with ID {key} not found.");
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [Authorize(Roles = "Staff,Admin")]
    [HttpDelete]
    public IActionResult Delete([FromRoute] short key)
    {
        var result = _categoryService.Delete(key);
        if (!result.Success)
        {
            return BadRequest(new { message = result.Message });
        }

        return NoContent();
    }
}
