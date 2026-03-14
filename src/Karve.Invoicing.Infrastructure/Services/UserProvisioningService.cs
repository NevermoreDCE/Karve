using Karve.Invoicing.Application.Interfaces;
using Karve.Invoicing.Application.Services;
using Karve.Invoicing.Domain.Entities;
using Karve.Invoicing.Domain.ValueObjects;

namespace Karve.Invoicing.Infrastructure.Services;

/// <summary>
/// Creates a local <see cref="AppUser"/> record for authenticated users that do not yet exist.
/// </summary>
public sealed class UserProvisioningService : IUserProvisioningService
{
    private readonly IUserRepository _userRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserProvisioningService"/> class.
    /// </summary>
    /// <param name="userRepository">User repository used to query and persist users.</param>
    public UserProvisioningService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    /// <inheritdoc />
    public async Task<Guid> EnsureUserProvisionedAsync(string externalUserId, string email)
    {
        var existingUser = await _userRepository.GetByExternalUserIdAsync(externalUserId);
        if (existingUser is not null)
        {
            return existingUser.Id;
        }

        var user = new AppUser
        {
            Id = Guid.NewGuid(),
            ExternalUserId = externalUserId,
            Email = new EmailAddress(email)
        };

        await _userRepository.AddAsync(user);
        return user.Id;
    }
}
