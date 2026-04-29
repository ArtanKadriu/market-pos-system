using MarketPOS.Repositories;
using MarketPOS.Utilities;

namespace MarketPOS.Forms;

public sealed class DashboardForm : Form
{
    public DashboardForm()
    {
        BackColor = Ui.Back;
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(16), ColumnCount = 2, RowCount = 3 };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50)); layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        Controls.Add(layout);
        Load += (_, _) =>
        {
            layout.Controls.Clear();
            layout.Controls.Add(Card("Sales Today", Db.Scalar("SELECT COUNT(*) FROM Sales WHERE date(SaleDate)=date('now')")?.ToString() ?? "0"));
            layout.Controls.Add(Card("Revenue Today", Convert.ToDecimal(Db.Scalar("SELECT COALESCE(SUM(Total),0) FROM Sales WHERE date(SaleDate)=date('now')")).ToString("C")));
            layout.Controls.Add(Card("Total Products", Db.Scalar("SELECT COUNT(*) FROM Products WHERE IsActive=1")?.ToString() ?? "0"));
            layout.Controls.Add(Card("Low Stock", Db.Scalar("SELECT COUNT(*) FROM Products WHERE StockQuantity<=MinStockAlert AND IsActive=1")?.ToString() ?? "0"));
            layout.Controls.Add(ChartPanel(), 0, 2);
            layout.Controls.Add(EmbeddedTable("Top Selling Products", "SELECT p.Name, SUM(si.Quantity) Quantity FROM SaleItems si JOIN Products p ON p.Id=si.ProductId GROUP BY p.Id ORDER BY Quantity DESC LIMIT 10"), 1, 2);
        };
    }

    private static Control Card(string title, string value)
    {
        var p = new Panel { BackColor = Ui.Panel, Margin = new Padding(8), Padding = new Padding(18), Height = 120, Dock = DockStyle.Fill };
        p.Controls.Add(new Label { Text = value, ForeColor = Ui.Accent, Font = new Font("Segoe UI", 24, FontStyle.Bold), Dock = DockStyle.Bottom, Height = 55 });
        p.Controls.Add(Ui.Label(title, 12));
        return p;
    }

    private static Control ChartPanel()
    {
        var panel = new FlowLayoutPanel { Dock = DockStyle.Fill, BackColor = Ui.Panel, Padding = new Padding(12), FlowDirection = FlowDirection.TopDown };
        panel.Controls.Add(Ui.Label("7 Day Revenue", 14));
        var t = Db.Query("SELECT date(SaleDate) Day, SUM(Total) Total FROM Sales GROUP BY date(SaleDate) ORDER BY Day DESC LIMIT 7");
        foreach (System.Data.DataRow r in t.Rows)
        {
            var total = Convert.ToDecimal(r["Total"]);
            panel.Controls.Add(new Label { Text = $"{r["Day"]}: {total:C}", ForeColor = Ui.Text, Width = 350, Height = 26 });
            panel.Controls.Add(new Panel { BackColor = Ui.Accent, Width = Math.Max(20, Math.Min(420, (int)total * 2)), Height = 12 });
        }
        if (t.Rows.Count == 0) panel.Controls.Add(Ui.Label("No sales yet."));
        return panel;
    }

    private static Control EmbeddedTable(string title, string sql)
    {
        var form = new TableForm(title, sql)
        {
            TopLevel = false,
            FormBorderStyle = FormBorderStyle.None,
            Dock = DockStyle.Fill
        };
        form.Show();
        return form;
    }
}
