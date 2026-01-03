using JoyReactor.Accordion.Logic.Database.Sql.Entities;
using Microsoft.EntityFrameworkCore;

namespace JoyReactor.Accordion.Logic.Extensions;

public static class DbSetExtensions
{
    public static async Task AddIgnoreExistingAsync<TEntity>(this DbSet<TEntity> dbSet, TEntity entity, CancellationToken cancellationToken)
        where TEntity : class, ISqlEntity
    {
        var isEntityExists = await dbSet.AnyAsync(e => e.Id == entity.Id, cancellationToken);
        if (isEntityExists)
            return;

        await dbSet.AddAsync(entity, cancellationToken);
    }

    public static async Task AddRangeIgnoreExistingAsync<TEntity>(this DbSet<TEntity> dbSet, IEnumerable<TEntity> entities, CancellationToken cancellationToken)
        where TEntity : class, ISqlEntity
    {
        var entityIds = entities
            .Select(entity => entity.Id)
            .ToArray();

        var existingEntityIds = await dbSet
            .Where(entity => entityIds.Contains(entity.Id))
            .Select(entity => entity.Id)
            .ToHashSetAsync(cancellationToken);

        var nonExistingEntities = entities
            .Where(entity => !existingEntityIds.Contains(entity.Id))
            .ToArray();

        if (nonExistingEntities.Length == 0)
            return;

        await dbSet.AddRangeAsync(nonExistingEntities, cancellationToken);
    }
}