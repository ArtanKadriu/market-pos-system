using MarketPOS.Models;
using MarketPOS.Repositories;
using MarketPOS.Services;
using MarketPOS.Utilities;

namespace MarketPOS.Forms;

public sealed class PosForm : Form
{
    private readonly List<CartItem> _cart = new();
    private readonly DataGridView _cartGrid = Ui.Grid();
    private readonly TextBox _barcode = Ui.TextBox("Scan barcode or type product name");
    private readonly NumericUpDown _qty = Ui.Money(9999);
    private readonly NumericUpDown _discount = Ui.Money();
    private readonly NumericUpDown _paid = Ui.Money();
    private readonly Label _total = Ui.Label("Total: 0.00", 20);
    private readonly SettingsService _settings = new();

    public PosForm()
    {
        BackColor = Ui.Back;
        _qty.Value = 1; _qty.DecimalPlaces = 0; _barcode.Width = 320;
        var top = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 56, Padding = new Padding(8), BackColor = Ui.Panel };
        var add = Ui.Button("Add"); add.Click += (_, _) => AddItem();
        var remove = Ui.Button("Remove"); remove.Click += (_, _) => RemoveItem();
        top.Controls.AddRange(new Control[] { _barcode, Ui.Label("Qty"), _qty, Ui.Label("Discount"), _discount, add, remove });
        var bottom = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 70, Padding = new Padding(8), BackColor = Ui.Panel };
        var cash = Ui.Button("Cash Sale"); var card = Ui.Button("Card Sale");
        cash.Click += (_, _) => Pay(PaymentMethod.Cash); card.Click += (_, _) => Pay(PaymentMethod.Card);
        bottom.Controls.AddRange(new Control[] { _total, Ui.Label("Paid"), _paid, cash, card });
        Controls.Add(_cartGrid); Controls.Add(top); Controls.Add(bottom);
        _barcode.KeyDown += (_, e) => { if (e.KeyCode == Keys.Enter) AddItem(); };
        RefreshCart();
    }

    private void AddItem()
    {
        var q = _barcode.Text.Trim();
        var t = Db.Query("SELECT * FROM Products WHERE IsActive=1 AND (Barcode=$q OR Name LIKE $like) LIMIT 1", ("$q", q), ("$like", "%" + q + "%"));
        if (t.Rows.Count == 0) { MessageBox.Show("Product not found."); return; }
        var r = t.Rows[0];
        var existing = _cart.FirstOrDefault(i => i.ProductId == Convert.ToInt32(r["Id"]));
        if (existing == null)
            _cart.Add(new CartItem { ProductId = Convert.ToInt32(r["Id"]), Barcode = r["Barcode"].ToString()!, ProductName = r["Name"].ToString()!, Quantity = _qty.Value, UnitPrice = Convert.ToDecimal(r["SalePrice"]), Discount = _discount.Value, TaxRate = _settings.Get().TaxRate });
        else existing.Quantity += _qty.Value;
        _barcode.Clear(); _qty.Value = 1; _discount.Value = 0; RefreshCart();
    }

    private void RemoveItem()
    {
        if (_cartGrid.CurrentRow == null) return;
        var id = Convert.ToInt32(_cartGrid.CurrentRow.Cells["ProductId"].Value);
        _cart.RemoveAll(i => i.ProductId == id);
        RefreshCart();
    }

    private void Pay(PaymentMethod method)
    {
        try
        {
            var total = _cart.Sum(i => i.LineTotal);
            var paid = method == PaymentMethod.Card ? total : _paid.Value;
            var id = new SaleService().CompleteSale(_cart, method, paid);
            var receipt = new SaleService().BuildReceipt(id);
            using var rf = new ReceiptForm(receipt);
            rf.ShowDialog();
            _cart.Clear(); _paid.Value = 0; RefreshCart();
        }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Sale error"); }
    }

    private void RefreshCart()
    {
        _cartGrid.DataSource = null;
        _cartGrid.DataSource = _cart.Select(i => new { i.ProductId, i.Barcode, i.ProductName, i.Quantity, i.UnitPrice, i.Discount, Tax = i.LineTax, Total = i.LineTotal }).ToList();
        _total.Text = $"Total: {_settings.Get().Currency}{_cart.Sum(i => i.LineTotal):0.00}";
    }
}
