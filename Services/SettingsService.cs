using MarketPOS.Models;
using MarketPOS.Repositories;

namespace MarketPOS.Services;

public sealed class SettingsService
{
    public AppSettings Get()
    {
        var t = Db.Query("SELECT Key,Value FROM Settings");
        var d = t.Rows.Cast<System.Data.DataRow>().ToDictionary(r => r["Key"].ToString()!, r => r["Value"].ToString()!);
        return new AppSettings
        {
            StoreName = d.GetValueOrDefault("StoreName", "MarketPOS Supermarket"),
            Currency = d.GetValueOrDefault("Currency", "$"),
            TaxRate = decimal.TryParse(d.GetValueOrDefault("TaxRate", "0"), out var tax) ? tax : 0,
            ReceiptFooter = d.GetValueOrDefault("ReceiptFooter", "Thank you for shopping with us!")
        };
    }

    public void Save(AppSettings s)
    {
        foreach (var pair in new[] { ("StoreName", s.StoreName), ("Currency", s.Currency), ("TaxRate", s.TaxRate.ToString("0.##")), ("ReceiptFooter", s.ReceiptFooter) })
            Db.Execute("INSERT INTO Settings(Key,Value) VALUES($k,$v) ON CONFLICT(Key) DO UPDATE SET Value=excluded.Value", ("$k", pair.Item1), ("$v", pair.Item2));
        AuditService.Log("Update", "Settings", "Updated store settings");
    }
}
