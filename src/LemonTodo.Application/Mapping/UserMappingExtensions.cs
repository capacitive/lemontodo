using LemonTodo.Application.DTOs;
using LemonTodo.Domain;

namespace LemonTodo.Application.Mapping;

public static class UserMappingExtensions
{
    public static UserProfile ToProfile(this User user)
        => new(
            user.Id,
            user.Email,
            user.DisplayName,
            user.TwoFactorEnabled,
            user.ExternalLogins.Select(e => e.Provider).ToList(),
            user.CreatedAt,
            user.LastLoginAt);
}
