namespace BlogsApi.Infrastructure;

public class BCryptHelper
{
    public static bool ComparePasswords(string dtoPass, string entityPass)
    {
        return BCrypt.Net.BCrypt.Verify(dtoPass, entityPass);
    }

    public static string EncryptPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }
}
