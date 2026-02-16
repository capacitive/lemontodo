using LemonTodo.Domain.Interfaces;

namespace LemonTodo.Infrastructure.IdGeneration;

public class NanoIdGenerator : IIdGenerator
{
    private static readonly char[] Alphabet = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz_-".ToCharArray();

    public string NewId()
    {
        var bytes = new byte[21];
        Random.Shared.NextBytes(bytes);
        var chars = new char[21];
        for (int i = 0; i < 21; i++)
            chars[i] = Alphabet[bytes[i] & 63];
        return new string(chars);
    }
}
