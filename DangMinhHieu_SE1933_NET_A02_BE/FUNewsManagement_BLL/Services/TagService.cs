using System.Collections.Generic;
using System.Linq;
using FUNewsManagement_BLL.DTOs;
using FUNewsManagement_DAL.Entities;
using FUNewsManagement_DAL.Repositories;

namespace FUNewsManagement_BLL.Services;

public class TagService : ITagService
{
    private readonly ITagRepository _tagRepository;

    public TagService(ITagRepository tagRepository)
    {
        _tagRepository = tagRepository;
    }

    public List<Tag> GetAllEntities()
    {
        return _tagRepository.GetAll();
    }

    public List<TagDto> GetAll()
    {
        var tags = _tagRepository.GetAll();
        return tags.Select(MapToDto).ToList();
    }

    public TagDto? GetById(int id)
    {
        var tag = _tagRepository.GetById(id);
        return tag == null ? null : MapToDto(tag);
    }

    public Tag? GetEntityById(int id)
    {
        return _tagRepository.GetById(id);
    }

    public TagDto Create(TagDto dto)
    {
        var tag = new Tag
        {
            TagId = dto.TagId,
            TagName = dto.TagName,
            Note = dto.Note
        };

        var created = _tagRepository.Create(tag);
        return MapToDto(created);
    }

    public TagDto Update(int id, TagDto dto)
    {
        var tag = new Tag
        {
            TagId = id,
            TagName = dto.TagName,
            Note = dto.Note
        };

        var updated = _tagRepository.Update(tag);
        return MapToDto(updated);
    }

    public (bool Success, string Message) Delete(int id)
    {
        return _tagRepository.Delete(id);
    }

    private static TagDto MapToDto(Tag t)
    {
        return new TagDto
        {
            TagId = t.TagId,
            TagName = t.TagName ?? string.Empty,
            Note = t.Note
        };
    }
}
