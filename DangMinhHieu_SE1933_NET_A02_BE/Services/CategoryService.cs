using System;
using System.Collections.Generic;
using System.Linq;
using FUNewsManagement_BLL.DTOs;
using FUNewsManagement_DAL.Entities;
using FUNewsManagement_DAL.Repositories;

namespace FUNewsManagement_BLL.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;

    public CategoryService(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public List<Category> GetAllEntities()
    {
        return _categoryRepository.GetAll();
    }

    public List<CategoryDto> GetAll()
    {
        var entities = _categoryRepository.GetAll();
        return entities.Select(MapToDto).ToList();
    }

    public List<CategoryDto> GetActiveCategories()
    {
        var entities = _categoryRepository.GetActiveCategories();
        return entities.Select(MapToDto).ToList();
    }

    public CategoryDto? GetById(short id)
    {
        var entity = _categoryRepository.GetById(id);
        return entity == null ? null : MapToDto(entity);
    }

    public Category? GetEntityById(short id)
    {
        return _categoryRepository.GetById(id);
    }

    public CategoryDto Create(CategoryDto dto)
    {
        var entity = new Category
        {
            CategoryName = dto.CategoryName,
            CategoryDesciption = dto.CategoryDesciption,
            ParentCategoryId = dto.ParentCategoryId,
            IsActive = dto.IsActive ?? true
        };

        var created = _categoryRepository.Create(entity);
        return MapToDto(created);
    }

    public CategoryDto Update(short id, CategoryDto dto)
    {
        var entity = new Category
        {
            CategoryId = id,
            CategoryName = dto.CategoryName,
            CategoryDesciption = dto.CategoryDesciption,
            ParentCategoryId = dto.ParentCategoryId,
            IsActive = dto.IsActive
        };

        var updated = _categoryRepository.Update(entity);
        return MapToDto(updated);
    }

    public (bool Success, string Message) Delete(short id)
    {
        return _categoryRepository.Delete(id);
    }

    private static CategoryDto MapToDto(Category c)
    {
        return new CategoryDto
        {
            CategoryId = c.CategoryId,
            CategoryName = c.CategoryName,
            CategoryDesciption = c.CategoryDesciption,
            ParentCategoryId = c.ParentCategoryId,
            ParentCategoryName = c.ParentCategory?.CategoryName,
            IsActive = c.IsActive,
            NewsArticleCount = c.NewsArticles?.Count ?? 0
        };
    }
}
