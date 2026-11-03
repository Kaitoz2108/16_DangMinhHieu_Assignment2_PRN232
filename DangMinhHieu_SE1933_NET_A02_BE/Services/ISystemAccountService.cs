using System.Collections.Generic;
using FUNewsManagement_BLL.DTOs;
using FUNewsManagement_DAL.Entities;

namespace FUNewsManagement_BLL.Services;

public interface ISystemAccountService
{
    List<SystemAccount> GetAllEntities();
    List<SystemAccountDto> GetAll();
    SystemAccountDto? GetById(short id);
    SystemAccount? GetEntityById(short id);
    SystemAccountDto? GetByEmail(string email);
    SystemAccountDto Create(SystemAccountDto dto);
    SystemAccountDto Update(short id, SystemAccountDto dto);
    (bool Success, string Message) Delete(short id);
}
