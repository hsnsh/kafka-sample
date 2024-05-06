﻿using System.Linq.Expressions;
using JetBrains.Annotations;
using Multithread.Api.Domain.Core.Entities;

namespace Multithread.Api.Domain.Core.Repositories;

public interface IGenericRepository<TEntity, in TKey> : IRepository
    where TEntity : class, IEntity<TKey>
{
    Task<TEntity> FindAsync(TKey id, bool includeDetails = true, CancellationToken cancellationToken = default);
    Task<TEntity> FindAsync([NotNull] Expression<Func<TEntity, bool>> predicate, bool includeDetails = true, CancellationToken cancellationToken = default);

    [NotNull]
    Task<TEntity> GetAsync(TKey id, bool includeDetails = true, CancellationToken cancellationToken = default);

    Task<TEntity> GetAsync([NotNull] Expression<Func<TEntity, bool>> predicate, bool includeDetails = true, CancellationToken cancellationToken = default);

    Task<List<TEntity>> GetListAsync(bool includeDetails = false, CancellationToken cancellationToken = default);

    Task<List<TEntity>> GetListAsync([NotNull] Expression<Func<TEntity, bool>> predicate, bool includeDetails = false, CancellationToken cancellationToken = default);

    Task<long> GetCountAsync(CancellationToken cancellationToken = default);

    Task<List<TEntity>> GetPagedListAsync(
        int skipCount,
        int maxResultCount,
        string sorting,
        bool includeDetails = false,
        CancellationToken cancellationToken = default);

    [NotNull]
    Task<TEntity> InsertAsync([NotNull] TEntity entity, CancellationToken cancellationToken = default);

    Task InsertManyAsync([NotNull] IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

    [NotNull]
    Task<TEntity> UpdateAsync([NotNull] TEntity entity, CancellationToken cancellationToken = default);

    Task UpdateManyAsync([NotNull] IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(TKey id, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync([NotNull] Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync([NotNull] TEntity entity, CancellationToken cancellationToken = default);
    Task DeleteManyAsync([NotNull] IEnumerable<TKey> ids, CancellationToken cancellationToken = default);
    Task DeleteManyAsync([NotNull] IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
}