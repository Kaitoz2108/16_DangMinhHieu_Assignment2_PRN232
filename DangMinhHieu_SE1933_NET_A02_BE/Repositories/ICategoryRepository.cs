using System.Collections.Generic;
using FUNewsManagement_DAL.Entities;

namespace FUNewsManagement_DAL.Repositories;

public interface ICategoryRepository
{
    List<Category> GetAll();
    List<Category> GetActiveCategories();
    Category? GetById(short id);
    Category Create(Category category);
    Category Update(Category category);
    (bool Success, string Message) Delete(short id);
}
