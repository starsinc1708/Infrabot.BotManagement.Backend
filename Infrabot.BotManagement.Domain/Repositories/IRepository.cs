using Infrabot.BotManagement.Domain.Models;

namespace Infrabot.BotManagement.Domain.Repositories;

public interface IRepository<T> where T : BaseModel
{
    Task<List<T>> GetAll(CancellationToken cancellationToken);
    Task<T?> GetById(long id, CancellationToken cancellationToken);
    Task<T?> Add(T entity, CancellationToken cancellationToken);
    Task<T?> Update(T entity, CancellationToken cancellationToken);
    Task<T?> Remove(T entity, CancellationToken cancellationToken);

    Task<List<T>> GetByField(string fieldName, object fieldValue, CancellationToken cancellationToken);
    Task<List<T>> GetByMultipleFields(Dictionary<string, object> fieldValues, CancellationToken cancellationToken);
}