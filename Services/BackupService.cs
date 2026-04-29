using MarketPOS.Database;

namespace MarketPOS.Services;

public static class BackupService
{
    public static void Backup(string destinationFile)
    {
        File.Copy(DatabaseInitializer.DatabasePath, destinationFile, true);
        AuditService.Log("Backup", "Database", destinationFile);
    }

    public static void Restore(string sourceFile)
    {
        File.Copy(sourceFile, DatabaseInitializer.DatabasePath, true);
        AuditService.Log("Restore", "Database", sourceFile);
    }
}
