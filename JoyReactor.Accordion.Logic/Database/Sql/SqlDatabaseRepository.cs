using JoyReactor.Accordion.Logic.Database.Sql.Entities;
using Microsoft.EntityFrameworkCore;

namespace JoyReactor.Accordion.Logic.Database.Sql;

public class SqlDatabaseRepository<T>(SqlDatabaseContext sqlDatabaseContext)
    : ISqlDatabaseRepository<T>
    where T : class, ISqlEntity
{
    public async Task AddRangeIgnoreExistingAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
    {
        var dbSet = sqlDatabaseContext.Set<T>();

        var entityIds = entities
            .Select(entity => entity.Id)
            .ToArray();

        var existingEntityIds = await dbSet
            .Where(entity => entityIds.Contains(entity.Id))
            .Select(entity => entity.Id)
            .ToHashSetAsync(cancellationToken);

        var notExistingEntities = entities
            .Where(entity => !existingEntityIds.Contains(entity.Id))
            .ToArray();

        await dbSet.AddRangeAsync(notExistingEntities, cancellationToken);
    }
}

public interface ISqlDatabaseRepository<T>
    where T : class, ISqlEntity
{
    Task AddRangeIgnoreExistingAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);
}