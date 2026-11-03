using System.Collections.Generic;
using FUNewsManagement_BLL.DTOs;
using FUNewsManagement_DAL.Entities;

namespace FUNewsManagement_BLL.Services;

public interface ITagService
{
    List<Tag> GetAllEntities();
    List<TagDto> GetAll();
    TagDto? GetById(int id);
    Tag? GetEntityById(int id);
    TagDto Create(TagDto dto);
    TagDto Update(int id, TagDto dto);
    (bool Success, string Message) Delete(int id);
}
