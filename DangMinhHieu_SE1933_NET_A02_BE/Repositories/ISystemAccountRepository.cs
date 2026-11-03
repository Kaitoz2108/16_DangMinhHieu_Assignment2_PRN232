using System.Collections.Generic;
using FUNewsManagement_DAL.Entities;

namespace FUNewsManagement_DAL.Repositories;

public interface ISystemAccountRepository
{
    List<SystemAccount> GetAll();
    SystemAccount? GetById(short id);
    SystemAccount? GetByEmail(string email);
    SystemAccount? Authenticate(string email, string password);
    SystemAccount Create(SystemAccount account);
    SystemAccount Update(SystemAccount account);
    (bool Success, string Message) Delete(short accountId);
}
