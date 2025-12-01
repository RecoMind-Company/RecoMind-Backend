using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace Authentication.Core.Helpers;

public static class SecurePasswordGenerator
{
    private const string AllValidChars = "ABCDEFGHIJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz0123456789!@#$%^&*()_+-=[]{};:.,<>?";

    public static string GenPassword(int length = 12)
    {
        const string StrengthRegex = @"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d)(?=.*[!@#$%^&*()_+\-=[\]{};:.,<>?]).*$";

        string password;

        do
        {
            password = GenerateRandomString(length);
        }
        while (!Regex.IsMatch(password, StrengthRegex));

        return password;
    }

    private static string GenerateRandomString(int length)
    {
        var password = new char[length];

        for (int i = 0; i < length; i++)
        {
            int randomIndex = RandomNumberGenerator.GetInt32(AllValidChars.Length);
            password[i] = AllValidChars[randomIndex];
        }

        return new string(password);
    }
}

