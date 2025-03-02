﻿using System.Security.Claims;
using JetBrains.Annotations;

namespace GeneralLibrary.Base.Core;

public interface ICurrentUser
{
    bool IsAuthenticated { get; }

    [CanBeNull]
    Guid? Id { get; }

    [CanBeNull]
    string UserName { get; }

    [CanBeNull]
    string Name { get; }

    [CanBeNull]
    string SurName { get; }

    [CanBeNull]
    string PhoneNumber { get; }

    bool PhoneNumberVerified { get; }

    [CanBeNull]
    string Email { get; }

    bool EmailVerified { get; }

    Guid? TenantId { get; }

    [CanBeNull]
    string TenantDomain { get; }

    [NotNull]
    string[] Roles { get; }

    [CanBeNull]
    Claim FindClaim(string claimType);

    [NotNull]
    Claim[] FindClaims(string claimType);

    [NotNull]
    Claim[] GetAllClaims();

    bool IsInRole(string roleName);
}
