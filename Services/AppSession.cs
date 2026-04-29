using MarketPOS.Models;

namespace MarketPOS.Services;

public static class AppSession
{
    public static User CurrentUser { get; set; } = new();
    public static bool IsAdmin => CurrentUser.Role == UserRole.Admin;
    public static bool IsManagerOrAdmin => CurrentUser.Role is UserRole.Admin or UserRole.Manager;
}
