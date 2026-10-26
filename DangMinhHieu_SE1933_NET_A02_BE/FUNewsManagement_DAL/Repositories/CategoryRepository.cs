using System.Collections.Generic;
using FUNewsManagement_DAL.DAOs;
using FUNewsManagement_DAL.Entities;

namespace FUNewsManagement_DAL.Repositories;

public class CategoryRepository : ICategoryRepository
{
    public List<Category> GetAll() => CategoryDAO.Instance.GetAll();

    public List<Category> GetActiveCategories() => CategoryDAO.Instance.GetActiveCategories();

    public Category? GetById(short id) => CategoryDAO.Instance.GetById(id);

    public Category Create(Category category) => CategoryDAO.Instance.Create(category);

    public Category Update(Category category) => CategoryDAO.Instance.Update(category);

    public (bool Success, string Message) Delete(short id) => CategoryDAO.Instance.Delete(id);
}
