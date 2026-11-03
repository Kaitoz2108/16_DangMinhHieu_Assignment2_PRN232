using System;
using System.Collections.Generic;
using System.Linq;
using FUNewsManagement_BLL.DTOs;
using FUNewsManagement_DAL.Entities;
using FUNewsManagement_DAL.Repositories;

namespace FUNewsManagement_BLL.Services;

public class SystemAccountService : ISystemAccountService
{
    private readonly ISystemAccountRepository _accountRepository;

    public SystemAccountService(ISystemAccountRepository accountRepository)
    {
        _accountRepository = accountRepository;
    }

    public List<SystemAccount> GetAllEntities()
    {
        return _accountRepository.GetAll();
    }

    public List<SystemAccountDto> GetAll()
    {
        var accounts = _accountRepository.GetAll();
        return accounts.Select(MapToDto).ToList();
    }

    public SystemAccountDto? GetById(short id)
    {
        var account = _accountRepository.GetById(id);
        return account == null ? null : MapToDto(account);
    }

    public SystemAccount? GetEntityById(short id)
    {
        return _accountRepository.GetById(id);
    }

    public SystemAccountDto? GetByEmail(string email)
    {
        var account = _accountRepository.GetByEmail(email);
        return account == null ? null : MapToDto(account);
    }

    public SystemAccountDto Create(SystemAccountDto dto)
    {
        var entity = new SystemAccount
        {
            AccountName = dto.AccountName,
            AccountEmail = dto.AccountEmail,
            AccountRole = dto.AccountRole,
            AccountPassword = dto.AccountPassword
        };

        var created = _accountRepository.Create(entity);
        return MapToDto(created);
    }

    public SystemAccountDto Update(short id, SystemAccountDto dto)
    {
        var entity = new SystemAccount
        {
            AccountId = id,
            AccountName = dto.AccountName,
            AccountEmail = dto.AccountEmail,
            AccountRole = dto.AccountRole,
            AccountPassword = dto.AccountPassword
        };

        var updated = _accountRepository.Update(entity);
        return MapToDto(updated);
    }

    public (bool Success, string Message) Delete(short id)
    {
        return _accountRepository.Delete(id);
    }

    private static SystemAccountDto MapToDto(SystemAccount a)
    {
        return new SystemAccountDto
        {
            AccountId = a.AccountId,
            AccountName = a.AccountName,
            AccountEmail = a.AccountEmail,
            AccountRole = a.AccountRole,
            AccountPassword = a.AccountPassword
        };
    }
}
