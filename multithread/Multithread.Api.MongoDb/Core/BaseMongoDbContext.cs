using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Multithread.Api.Auditing;
using Multithread.Api.Core;
using Multithread.Api.Domain.Core.Entities;
using Multithread.Api.MongoDb.Core.Context;

namespace Multithread.Api.MongoDb.Core;

public abstract class BaseMongoDbContext : MongoDbContext, IScopedDependency
{
    public TimeSpan ClientWaitQueueTimeout => Client.Settings.WaitQueueTimeout;

    private IServiceProvider ServiceProvider { get; set; }

    public IAuditPropertySetter AuditPropertySetter => ServiceProvider?.GetRequiredService<IAuditPropertySetter>();

    protected BaseMongoDbContext(IServiceProvider provider, MongoClientSettings clientSettings, string databaseName) : base(clientSettings, databaseName)
    {
        ServiceProvider = provider;
        CommandTrackerEvent += CommandTrackerEvent_Tracked;
    }

    protected BaseMongoDbContext(IServiceProvider provider, string connectionString) : this(provider, CreateClientSettings(connectionString), MongoUrl.Create(connectionString).DatabaseName)
    {
    }

    private static MongoClientSettings CreateClientSettings(string connectionString, int queryExecutionMaxSeconds = 60)
    {
        ThreadPool.GetMaxThreads(out var maxWt, out var _);

        var mongoUrl = MongoUrl.Create(connectionString);
        var clientSettings = MongoClientSettings.FromConnectionString(mongoUrl.Url);
        clientSettings.MaxConnectionPoolSize = maxWt * 2;

        // In version 2.19, MongoDB team upgraded to LinqProvider.V3, rolling back to V2 until LinQ is stable...
        // https://www.mongodb.com/community/forums/t/issue-with-2-18-to-2-19-nuget-upgrade-of-mongodb-c-driver/211894/2
        clientSettings.LinqProvider = LinqProvider.V2;

        if (queryExecutionMaxSeconds < 1) queryExecutionMaxSeconds = 60;
        clientSettings.WaitQueueTimeout = TimeSpan.FromSeconds(queryExecutionMaxSeconds);

        return clientSettings;
    }

    private void CommandTrackerEvent_Tracked(object sender, MongoEntityEventArgs e)
    {
        switch (e.CommandState)
        {
            case MongoCommandState.Added:
                ApplyBaseConceptsForAddedEntity(e.EntryEntity);
                break;
            case MongoCommandState.Modified:
                ApplyBaseConceptsForModifiedEntity(e.EntryEntity);
                break;
            case MongoCommandState.Deleted:
                ApplyBaseConceptsForDeletedEntity(e.EntryEntity);
                break;
        }
    }

    private void ApplyBaseConceptsForAddedEntity(object entity)
    {
        CheckAndSetId(entity);
        AuditPropertySetter?.SetCreationProperties(entity);
    }

    private void ApplyBaseConceptsForModifiedEntity(object entity)
    {
        AuditPropertySetter?.SetModificationProperties(entity);
        if (entity is ISoftDelete && ((ISoftDelete)entity).IsDeleted)
        {
            AuditPropertySetter?.SetDeletionProperties(entity);
        }
    }

    private void ApplyBaseConceptsForDeletedEntity(object entity)
    {
        if (!(entity is ISoftDelete))
        {
            return;
        }

        ((ISoftDelete)entity).IsDeleted = true;
        AuditPropertySetter?.SetDeletionProperties(entity);
    }

    private void CheckAndSetId(object targetObject)
    {
        if (targetObject is IEntity<Guid> entityWithGuidId)
        {
            if (entityWithGuidId.Id != default)
            {
                return;
            }

            entityWithGuidId.Id = Guid.NewGuid();
        }
    }

    public Task<int> SaveSaveEntityCommandsIfExistChangesAsync()
    {
        return SaveEntityCommandsAsync();
    }
}