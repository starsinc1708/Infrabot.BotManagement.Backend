using System.Linq.Expressions;
using System.Reflection;
using Infrabot.BotManagement.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Infrabot.BotManagement.Domain.Repositories;

public abstract class BaseRepository<TEntity>(BotManagementDbContext dbContext) 
    : IRepository<TEntity> where TEntity : BaseModel
{
    public virtual async Task<List<TEntity>> GetAll(CancellationToken cancellationToken)
    {
        return await dbContext.Set<TEntity>().AsNoTracking().ToListAsync(cancellationToken);
    }
    
    public virtual async Task<TEntity?> GetById(long id, CancellationToken cancellationToken)
    {
        return await dbContext.Set<TEntity>().Where(x => x.Id == id).AsNoTracking().FirstOrDefaultAsync(cancellationToken);
    }
    
    public virtual async Task<TEntity?> Add(TEntity entity, CancellationToken cancellationToken)
    {
        var addedEntity = (await dbContext.Set<TEntity>().AddAsync(entity, cancellationToken)).Entity;
        await dbContext.SaveChangesAsync(cancellationToken);
        return addedEntity;
    }

    public virtual async Task<TEntity?> Update(TEntity entity, CancellationToken cancellationToken)
    {
        var updatedEntity =  dbContext.Set<TEntity>().Update(entity).Entity;
        await dbContext.SaveChangesAsync(cancellationToken);
        return updatedEntity;
    }

    public virtual async Task<TEntity?> Remove(TEntity entity, CancellationToken cancellationToken)
    {
        var deletedEntity = dbContext.Set<TEntity>().Remove(entity).Entity;
        await dbContext.SaveChangesAsync(cancellationToken);
        return deletedEntity;
    }

    public virtual async Task<List<TEntity>> GetByField(string fieldName, object fieldValue, CancellationToken cancellationToken)
    {
        var entityType = typeof(TEntity);
        var property = entityType.GetProperty(fieldName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
        
        if (property == null)
        {
            throw new ArgumentException($"Field '{fieldName}' not found in entity '{entityType.Name}'.");
        }
        
        var parameter = Expression.Parameter(entityType, "x");
        var propertyAccess = Expression.Property(parameter, property);
        var value = Expression.Constant(fieldValue);
        var equals = Expression.Equal(propertyAccess, value);
        var lambda = Expression.Lambda<Func<TEntity, bool>>(equals, parameter);
        
        return await dbContext.Set<TEntity>()
            .AsNoTracking()
            .Where(lambda)
            .ToListAsync(cancellationToken);
    }
    
    public virtual async Task<List<TEntity>> GetByMultipleFields(Dictionary<string, object> fieldValues, CancellationToken cancellationToken)
    {
        var entityType = typeof(TEntity);
        var parameter = Expression.Parameter(entityType, "x");
        var expressions = new List<Expression>();

        foreach (var field in fieldValues)
        {
            var property = entityType.GetProperty(field.Key, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            if (property == null)
            {
                throw new ArgumentException($"Field '{field.Key}' not found in entity '{entityType.Name}'.");
            }
            
            var propertyAccess = Expression.Property(parameter, property);
            var convertedValue = Convert.ChangeType(field.Value, property.PropertyType);
            var value = Expression.Constant(convertedValue);
            var equals = Expression.Equal(propertyAccess, value);

            expressions.Add(equals);
        }
        
        var combinedExpression = expressions.Aggregate(Expression.AndAlso);
        
        var lambda = Expression.Lambda<Func<TEntity, bool>>(combinedExpression, parameter);
        
        return await dbContext.Set<TEntity>()
            .AsNoTracking()
            .Where(lambda)
            .ToListAsync(cancellationToken);
    }
}