using System.Collections.Generic;
using FUNewsManagement_BLL.DTOs;
using FUNewsManagement_DAL.Entities;

namespace FUNewsManagement_BLL.Services;

public interface ICategoryService
{
    List<Category> GetAllEntities();
    List<CategoryDto> GetAll();
    List<CategoryDto> GetActiveCategories();
    CategoryDto? GetById(short id);
    Category? GetEntityById(short id);
    CategoryDto Create(CategoryDto dto);
    CategoryDto Update(short id, CategoryDto dto);
    (bool Success, string Message) Delete(short id);
}
