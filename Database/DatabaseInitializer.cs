using System.Security.Cryptography;
using System.Text;
using Microsoft.Data.Sqlite;

namespace MarketPOS.Database;

public static class DatabaseInitializer
{
    public static string DataDirectory => Path.Combine(AppContext.BaseDirectory, "Data");
    public static string DatabasePath => Path.Combine(DataDirectory, "marketpos.db");
    public static string ConnectionString => $"Data Source={DatabasePath}";

    public static void Initialize()
    {
        Directory.CreateDirectory(DataDirectory);
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Database", "schema.sql"));
        command.ExecuteNonQuery();
        Seed(connection);
    }

    private static void Seed(SqliteConnection connection)
    {
        using var tx = connection.BeginTransaction();
        Execute(connection, tx, "INSERT OR IGNORE INTO Users (Username, PasswordHash, FullName, RoleId) VALUES ('admin', $hash, 'System Administrator', 1)", ("$hash", HashPassword("admin123")));
        Execute(connection, tx, "INSERT OR IGNORE INTO Suppliers (Id, Name, Phone, Email, Address) VALUES (1, 'Default Supplier', '+1 555 0100', 'supplier@example.com', 'Main warehouse')");
        Execute(connection, tx, "INSERT OR IGNORE INTO Products (Barcode, Name, CategoryId, CostPrice, SalePrice, StockQuantity, MinStockAlert) VALUES ('1000001','Milk 1L',2,0.80,1.25,80,10),('1000002','Bread Loaf',1,0.65,1.10,50,8),('1000003','Cola 500ml',3,0.55,1.00,120,20),('1000004','Rice 1kg',1,1.20,2.10,70,10),('1000005','Dish Soap',4,1.10,1.95,35,6)");
        tx.Commit();
    }

    public static string HashPassword(string password) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(password)));

    private static void Execute(SqliteConnection connection, SqliteTransaction tx, string sql, params (string, object)[] parameters)
    {
        using var command = connection.CreateCommand();
        command.Transaction = tx;
        command.CommandText = sql;
        foreach (var (name, value) in parameters) command.Parameters.AddWithValue(name, value);
        command.ExecuteNonQuery();
    }
}
