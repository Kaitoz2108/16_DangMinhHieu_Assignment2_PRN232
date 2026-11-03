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

[Authorize(Roles = "Staff,Admin")]
public class TagsController : ODataController
{
    private readonly ITagService _tagService;

    public TagsController(ITagService tagService)
    {
        _tagService = tagService;
    }

    [EnableQuery]
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Get()
    {
        var tags = _tagService.GetAllEntities().AsQueryable();
        return Ok(tags);
    }

    [EnableQuery]
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Get([FromRoute] int key)
    {
        var tag = _tagService.GetEntityById(key);
        if (tag == null)
        {
            return NotFound($"Tag with ID {key} not found.");
        }
        return Ok(tag);
    }

    [HttpPost]
    public IActionResult Post([FromBody] TagDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var created = _tagService.Create(dto);
            return Created(created);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut]
    public IActionResult Put([FromRoute] int key, [FromBody] TagDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var updated = _tagService.Update(key, dto);
            return Ok(updated);
        }
        catch (KeyNotFoundException)
        {
            return NotFound($"Tag with ID {key} not found.");
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete]
    public IActionResult Delete([FromRoute] int key)
    {
        var result = _tagService.Delete(key);
        if (!result.Success)
        {
            return BadRequest(new { message = result.Message });
        }

        return NoContent();
    }
}
