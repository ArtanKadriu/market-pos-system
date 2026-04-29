using MarketPOS.Repositories;
using MarketPOS.Services;
using MarketPOS.Utilities;

namespace MarketPOS.Forms;

public sealed class InventoryForm : Form
{
    private readonly DataGridView _grid = Ui.Grid();
    private readonly ComboBox _products = new() { Width = 260, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly NumericUpDown _qty = Ui.Money(99999);
    private readonly ComboBox _type = new() { Width = 130, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly TextBox _notes = Ui.TextBox("Notes");

    public InventoryForm()
    {
        BackColor = Ui.Back; _type.Items.AddRange(new object[] { "StockIn", "StockOut", "Adjustment" }); _type.SelectedIndex = 0;
        var top = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 58, Padding = new Padding(8), BackColor = Ui.Panel };
        var apply = Ui.Button("Apply Movement"); apply.Click += (_, _) => Apply();
        top.Controls.AddRange(new Control[] { _products, _type, Ui.Label("Qty"), _qty, _notes, apply });
        Controls.Add(_grid); Controls.Add(top);
        Load += (_, _) => { LoadProducts(); LoadData(); };
    }

    private void LoadProducts()
    {
        var t = Db.Query("SELECT Id, Name || ' (' || Barcode || ')' Name FROM Products WHERE IsActive=1 ORDER BY Name");
        _products.DataSource = t; _products.ValueMember = "Id"; _products.DisplayMember = "Name";
    }

    private void Apply()
    {
        if (_products.SelectedValue == null || _qty.Value <= 0) return;
        var signQty = _type.Text == "StockOut" ? -_qty.Value : _qty.Value;
        if (_type.Text == "Adjustment")
            Db.Execute("UPDATE Products SET StockQuantity=$q WHERE Id=$p", ("$q", _qty.Value), ("$p", _products.SelectedValue));
        else
            Db.Execute("UPDATE Products SET StockQuantity=StockQuantity+$q WHERE Id=$p", ("$q", signQty), ("$p", _products.SelectedValue));
        Db.Execute("INSERT INTO StockMovements(ProductId,MovementType,Quantity,Reference,Notes,UserId) VALUES($p,$t,$q,'Manual',$n,$u)", ("$p", _products.SelectedValue), ("$t", _type.Text), ("$q", signQty), ("$n", _notes.Text), ("$u", AppSession.CurrentUser.Id));
        AuditService.Log("Stock", "Inventory", $"{_type.Text} {_qty.Value}");
        LoadData();
    }

    private void LoadData() => _grid.DataSource = Db.Query("SELECT sm.Id,p.Name Product,sm.MovementType,sm.Quantity,sm.Reference,sm.Notes,sm.MovementDate FROM StockMovements sm JOIN Products p ON p.Id=sm.ProductId ORDER BY sm.Id DESC");
}
