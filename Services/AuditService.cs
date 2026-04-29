using MarketPOS.Repositories;

namespace MarketPOS.Services;

public static class AuditService
{
    public static void Log(string action, string entity, string details = "")
    {
        var userId = AppSession.CurrentUser.Id == 0 ? null : (object)AppSession.CurrentUser.Id;
        Db.Execute("INSERT INTO AuditLogs(UserId,Action,Entity,Details) VALUES($u,$a,$e,$d)", ("$u", userId), ("$a", action), ("$e", entity), ("$d", details));
    }
}
