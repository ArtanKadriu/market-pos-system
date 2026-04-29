using System.Text;
using MarketPOS.Models;
using MarketPOS.Repositories;
using Microsoft.Data.Sqlite;

namespace MarketPOS.Services;

public sealed class SaleService
{
    public int CompleteSale(List<CartItem> items, PaymentMethod method, decimal paid)
    {
        if (items.Count == 0) throw new InvalidOperationException("Cart is empty.");
        var subtotal = items.Sum(i => i.LineSubtotal);
        var discount = items.Sum(i => i.Discount);
        var tax = items.Sum(i => i.LineTax);
        var total = items.Sum(i => i.LineTotal);
        if (method == PaymentMethod.Cash && paid < total) throw new InvalidOperationException("Paid amount is less than total.");
        using var c = Db.Open();
        using var tx = c.BeginTransaction();
        var invoice = "INV-" + DateTime.Now.ToString("yyyyMMddHHmmss");
        using var saleCmd = c.CreateCommand();
        saleCmd.Transaction = tx;
        saleCmd.CommandText = "INSERT INTO Sales(InvoiceNo,UserId,Subtotal,Discount,Tax,Total,Paid,ChangeAmount,PaymentMethod) VALUES($i,$u,$s,$d,$t,$to,$p,$c,$m); SELECT last_insert_rowid();";
        Add(saleCmd, "$i", invoice); Add(saleCmd, "$u", AppSession.CurrentUser.Id); Add(saleCmd, "$s", subtotal); Add(saleCmd, "$d", discount); Add(saleCmd, "$t", tax); Add(saleCmd, "$to", total); Add(saleCmd, "$p", paid); Add(saleCmd, "$c", Math.Max(0, paid - total)); Add(saleCmd, "$m", method.ToString());
        var saleId = Convert.ToInt32((long)saleCmd.ExecuteScalar()!);

        foreach (var item in items)
        {
            var stock = Convert.ToDecimal(Scalar(c, tx, "SELECT StockQuantity FROM Products WHERE Id=$id", ("$id", item.ProductId))!);
            if (stock < item.Quantity) throw new InvalidOperationException($"Insufficient stock for {item.ProductName}.");
            Exec(c, tx, "INSERT INTO SaleItems(SaleId,ProductId,Quantity,UnitPrice,Discount,Tax,Total) VALUES($s,$p,$q,$u,$d,$t,$to)", ("$s", saleId), ("$p", item.ProductId), ("$q", item.Quantity), ("$u", item.UnitPrice), ("$d", item.Discount), ("$t", item.LineTax), ("$to", item.LineTotal));
            Exec(c, tx, "UPDATE Products SET StockQuantity=StockQuantity-$q, UpdatedAt=CURRENT_TIMESTAMP WHERE Id=$p", ("$q", item.Quantity), ("$p", item.ProductId));
            Exec(c, tx, "INSERT INTO StockMovements(ProductId,MovementType,Quantity,Reference,Notes,UserId) VALUES($p,'Sale',$q,$r,'POS sale',$u)", ("$p", item.ProductId), ("$q", -item.Quantity), ("$r", invoice), ("$u", AppSession.CurrentUser.Id));
        }
        Exec(c, tx, "INSERT INTO AuditLogs(UserId,Action,Entity,Details) VALUES($u,'Create','Sale',$d)", ("$u", AppSession.CurrentUser.Id), ("$d", invoice));
        tx.Commit();
        return saleId;
    }

    public void Refund(string invoiceNo)
    {
        using var c = Db.Open();
        using var tx = c.BeginTransaction();
        var saleIdObj = Scalar(c, tx, "SELECT Id FROM Sales WHERE InvoiceNo=$i AND IsReturned=0", ("$i", invoiceNo));
        if (saleIdObj is null) throw new InvalidOperationException("Invoice not found or already returned.");
        var saleId = Convert.ToInt32((long)saleIdObj);
        using var cmd = c.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText = "SELECT ProductId,Quantity FROM SaleItems WHERE SaleId=$s";
        cmd.Parameters.AddWithValue("$s", saleId);
        using var r = cmd.ExecuteReader();
        var rows = new List<(int ProductId, decimal Quantity)>();
        while (r.Read()) rows.Add((r.GetInt32(0), r.GetDecimal(1)));
        foreach (var row in rows)
        {
            Exec(c, tx, "UPDATE Products SET StockQuantity=StockQuantity+$q WHERE Id=$p", ("$q", row.Quantity), ("$p", row.ProductId));
            Exec(c, tx, "INSERT INTO StockMovements(ProductId,MovementType,Quantity,Reference,Notes,UserId) VALUES($p,'Return',$q,$r,'Refund',$u)", ("$p", row.ProductId), ("$q", row.Quantity), ("$r", invoiceNo), ("$u", AppSession.CurrentUser.Id));
        }
        Exec(c, tx, "UPDATE Sales SET IsReturned=1 WHERE Id=$s", ("$s", saleId));
        tx.Commit();
        AuditService.Log("Refund", "Sale", invoiceNo);
    }

    public string BuildReceipt(int saleId)
    {
        var settings = new SettingsService().Get();
        var sale = Db.Query("SELECT * FROM Sales WHERE Id=$id", ("$id", saleId)).Rows[0];
        var items = Db.Query("SELECT p.Name, si.Quantity, si.UnitPrice, si.Total FROM SaleItems si JOIN Products p ON p.Id=si.ProductId WHERE si.SaleId=$id", ("$id", saleId));
        var sb = new StringBuilder();
        sb.AppendLine(settings.StoreName);
        sb.AppendLine("Invoice: " + sale["InvoiceNo"]);
        sb.AppendLine(DateTime.Now.ToString("g"));
        sb.AppendLine(new string('-', 32));
        foreach (System.Data.DataRow i in items.Rows) sb.AppendLine($"{i["Name"],-16}{i["Quantity"],4} x {settings.Currency}{i["UnitPrice"],6} {settings.Currency}{i["Total"],7}");
        sb.AppendLine(new string('-', 32));
        sb.AppendLine($"TOTAL: {settings.Currency}{sale["Total"]}");
        sb.AppendLine($"PAID: {settings.Currency}{sale["Paid"]}");
        sb.AppendLine($"CHANGE: {settings.Currency}{sale["ChangeAmount"]}");
        sb.AppendLine(settings.ReceiptFooter);
        return sb.ToString();
    }

    private static void Add(SqliteCommand c, string n, object v) => c.Parameters.AddWithValue(n, v);
    private static object? Scalar(SqliteConnection c, SqliteTransaction tx, string sql, params (string, object?)[] p) { using var cmd = c.CreateCommand(); cmd.Transaction = tx; cmd.CommandText = sql; foreach (var (k, v) in p) cmd.Parameters.AddWithValue(k, v ?? DBNull.Value); return cmd.ExecuteScalar(); }
    private static void Exec(SqliteConnection c, SqliteTransaction tx, string sql, params (string, object?)[] p) { using var cmd = c.CreateCommand(); cmd.Transaction = tx; cmd.CommandText = sql; foreach (var (k, v) in p) cmd.Parameters.AddWithValue(k, v ?? DBNull.Value); cmd.ExecuteNonQuery(); }
}
