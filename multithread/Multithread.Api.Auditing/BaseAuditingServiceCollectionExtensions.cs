﻿using Microsoft.Extensions.DependencyInjection;
using Multithread.Api.Core.Security;

namespace Multithread.Api.Auditing;

public static class BaseAuditingServiceCollectionExtensions
{
    public static IServiceCollection AddBaseAuditingServiceCollection(this IServiceCollection services)
    {
        services.AddScoped<ICurrentUser, CurrentUser>();
        services.AddScoped<IAuditPropertySetter, AuditPropertySetter>();

        return services;
    }
}