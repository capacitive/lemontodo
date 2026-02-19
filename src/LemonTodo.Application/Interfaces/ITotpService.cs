namespace LemonTodo.Application.Interfaces;

public interface ITotpService
{
    string GenerateSecret();
    string GetQrCodeUri(string secret, string email, string issuer = "LemonTodo");
    bool ValidateCode(string secret, string code);
}
