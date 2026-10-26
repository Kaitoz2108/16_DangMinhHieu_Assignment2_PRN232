using System.Collections.Generic;
using FUNewsManagement_DAL.Entities;

namespace FUNewsManagement_DAL.Repositories;

public interface ITagRepository
{
    List<Tag> GetAll();
    Tag? GetById(int id);
    Tag Create(Tag tag);
    Tag Update(Tag tag);
    (bool Success, string Message) Delete(int id);
}
