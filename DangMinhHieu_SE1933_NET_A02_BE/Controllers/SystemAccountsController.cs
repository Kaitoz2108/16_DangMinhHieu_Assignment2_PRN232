using System;
using System.Linq;
using FUNewsManagement_BLL.DTOs;
using FUNewsManagement_BLL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;

namespace FUNewsManagement_API.Controllers;

[Authorize(Roles = "Admin")]
public class SystemAccountsController : ODataController
{
    private readonly ISystemAccountService _accountService;

    public SystemAccountsController(ISystemAccountService accountService)
    {
        _accountService = accountService;
    }

    [EnableQuery]
    [HttpGet]
    public IActionResult Get()
    {
        var accounts = _accountService.GetAllEntities().AsQueryable();
        return Ok(accounts);
    }

    [EnableQuery]
    [HttpGet]
    public IActionResult Get([FromRoute] short key)
    {
        var account = _accountService.GetEntityById(key);
        if (account == null)
        {
            return NotFound($"Account with ID {key} not found.");
        }
        return Ok(account);
    }

    [HttpPost]
    public IActionResult Post([FromBody] SystemAccountDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var created = _accountService.Create(dto);
            return Created(created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut]
    public IActionResult Put([FromRoute] short key, [FromBody] SystemAccountDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var updated = _accountService.Update(key, dto);
            return Ok(updated);
        }
        catch (KeyNotFoundException)
        {
            return NotFound($"Account with ID {key} not found.");
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete]
    public IActionResult Delete([FromRoute] short key)
    {
        var result = _accountService.Delete(key);
        if (!result.Success)
        {
            return BadRequest(new { message = result.Message });
        }

        return NoContent();
    }
}
