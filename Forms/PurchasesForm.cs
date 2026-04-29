using MarketPOS.Repositories;
using MarketPOS.Services;
using MarketPOS.Utilities;

namespace MarketPOS.Forms;

public sealed class PurchasesForm : Form
{
    private readonly DataGridView _grid = Ui.Grid();
    private readonly ComboBox _supplier = new() { Width = 180, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox _product = new() { Width = 260, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly NumericUpDown _qty = Ui.Money(99999);
    private readonly NumericUpDown _cost = Ui.Money();

    public PurchasesForm()
    {
        BackColor = Ui.Back;
        var top = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 58, Padding = new Padding(8), BackColor = Ui.Panel };
        var receive = Ui.Button("Receive Stock"); receive.Click += (_, _) => Receive();
        top.Controls.AddRange(new Control[] { _supplier, _product, Ui.Label("Qty"), _qty, Ui.Label("Cost"), _cost, receive });
        Controls.Add(_grid); Controls.Add(top);
        Load += (_, _) => { LoadLists(); LoadData(); };
    }
    private void LoadLists()
    {
        _supplier.DataSource = Db.Query("SELECT Id,Name FROM Suppliers ORDER BY Name"); _supplier.ValueMember = "Id"; _supplier.DisplayMember = "Name";
        _product.DataSource = Db.Query("SELECT Id,Name FROM Products WHERE IsActive=1 ORDER BY Name"); _product.ValueMember = "Id"; _product.DisplayMember = "Name";
    }
    private void Receive()
    {
        if (_qty.Value <= 0) return;
        var invoice = "PO-" + DateTime.Now.ToString("yyyyMMddHHmmss");
        var total = _qty.Value * _cost.Value;
        Db.Execute("INSERT INTO Purchases(SupplierId,InvoiceNo,Total,Status) VALUES($s,$i,$t,'Received')", ("$s", _supplier.SelectedValue), ("$i", invoice), ("$t", total));
        var id = Convert.ToInt64(Db.Scalar("SELECT last_insert_rowid()"));
        Db.Execute("INSERT INTO PurchaseItems(PurchaseId,ProductId,Quantity,CostPrice,Total) VALUES($po,$p,$q,$c,$t)", ("$po", id), ("$p", _product.SelectedValue), ("$q", _qty.Value), ("$c", _cost.Value), ("$t", total));
        Db.Execute("UPDATE Products SET StockQuantity=StockQuantity+$q, CostPrice=$c WHERE Id=$p", ("$q", _qty.Value), ("$c", _cost.Value), ("$p", _product.SelectedValue));
        Db.Execute("INSERT INTO StockMovements(ProductId,MovementType,Quantity,Reference,Notes,UserId) VALUES($p,'Purchase',$q,$r,'Purchase received',$u)", ("$p", _product.SelectedValue), ("$q", _qty.Value), ("$r", invoice), ("$u", AppSession.CurrentUser.Id));
        AuditService.Log("Receive", "Purchase", invoice); LoadData();
    }
    private void LoadData() => _grid.DataSource = Db.Query("SELECT pu.Id,pu.InvoiceNo,s.Name Supplier,pu.Total,pu.Status,pu.PurchaseDate FROM Purchases pu JOIN Suppliers s ON s.Id=pu.SupplierId ORDER BY pu.Id DESC");
}
