using LemonTodo.Domain;

namespace LemonTodo.Application.Interfaces;

public interface ITokenGenerator
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
    string HashToken(string token);
}
