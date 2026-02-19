using System.Security.Claims;

namespace LemonTodo.Api.Auth;

public static class ClaimsExtensions
{
    public static string GetUserId(this ClaimsPrincipal principal)
    {
        var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? principal.FindFirst("sub")?.Value;

        return userId ?? throw new UnauthorizedAccessException("User ID not found in claims.");
    }
}
