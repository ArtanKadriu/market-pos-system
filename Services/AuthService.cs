using MarketPOS.Database;
using MarketPOS.Models;
using MarketPOS.Repositories;

namespace MarketPOS.Services;

public sealed class AuthService
{
    public User? Login(string username, string password)
    {
        var table = Db.Query("SELECT * FROM Users WHERE Username=$u AND PasswordHash=$p AND IsActive=1", ("$u", username.Trim()), ("$p", DatabaseInitializer.HashPassword(password)));
        if (table.Rows.Count == 0) return null;
        var r = table.Rows[0];
        return new User
        {
            Id = Convert.ToInt32(r["Id"]),
            Username = r["Username"].ToString()!,
            FullName = r["FullName"].ToString()!,
            Role = (UserRole)Convert.ToInt32(r["RoleId"]),
            IsActive = Convert.ToInt32(r["IsActive"]) == 1
        };
    }
}
