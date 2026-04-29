using System.Text;
using MarketPOS.Repositories;
using MarketPOS.Utilities;

namespace MarketPOS.Forms;

public sealed class ReportsForm : Form
{
    private readonly DataGridView _grid = Ui.Grid();
    private string _currentSql = "";
    public ReportsForm()
    {
        BackColor = Ui.Back;
        var top = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 58, Padding = new Padding(8), BackColor = Ui.Panel };
        foreach (var (name, sql) in new[] {
            ("Daily Sales","SELECT date(SaleDate) Day, COUNT(*) Sales, SUM(Total) Revenue FROM Sales GROUP BY date(SaleDate) ORDER BY Day DESC"),
            ("Monthly Sales","SELECT strftime('%Y-%m',SaleDate) Month, COUNT(*) Sales, SUM(Total) Revenue FROM Sales GROUP BY Month ORDER BY Month DESC"),
            ("Profit","SELECT p.Name, SUM(si.Quantity) Qty, SUM((si.UnitPrice-p.CostPrice)*si.Quantity) Profit FROM SaleItems si JOIN Products p ON p.Id=si.ProductId GROUP BY p.Id ORDER BY Profit DESC"),
            ("Top Selling","SELECT p.Name, SUM(si.Quantity) Quantity, SUM(si.Total) Revenue FROM SaleItems si JOIN Products p ON p.Id=si.ProductId GROUP BY p.Id ORDER BY Quantity DESC"),
            ("Inventory Value","SELECT Name, StockQuantity, CostPrice, SalePrice, StockQuantity*CostPrice CostValue, StockQuantity*SalePrice RetailValue FROM Products WHERE IsActive=1")})
        {
            var b = Ui.Button(name); b.Click += (_, _) => Run(sql); top.Controls.Add(b);
        }
        var export = Ui.Button("Export CSV/Excel"); export.Click += (_, _) => Export();
        top.Controls.Add(export); Controls.Add(_grid); Controls.Add(top);
    }
    private void Run(string sql) { _currentSql = sql; _grid.DataSource = Db.Query(sql); }
    private void Export()
    {
        if (_grid.DataSource == null) return;
        using var dlg = new SaveFileDialog { Filter = "CSV compatible with Excel|*.csv", FileName = "report.csv" };
        if (dlg.ShowDialog() != DialogResult.OK) return;
        var sb = new StringBuilder();
        var cols = _grid.Columns.Cast<DataGridViewColumn>().Where(c => c.Visible).ToList();
        sb.AppendLine(string.Join(",", cols.Select(c => Csv(c.HeaderText))));
        foreach (DataGridViewRow row in _grid.Rows) sb.AppendLine(string.Join(",", cols.Select(c => Csv(row.Cells[c.Index].Value?.ToString() ?? ""))));
        File.WriteAllText(dlg.FileName, sb.ToString());
    }
    private static string Csv(string s) => "\"" + s.Replace("\"", "\"\"") + "\"";
}
