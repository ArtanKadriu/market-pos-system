using MarketPOS.Repositories;
using MarketPOS.Services;
using MarketPOS.Utilities;

namespace MarketPOS.Forms;

public sealed class ProductsForm : Form
{
    private readonly DataGridView _grid = Ui.Grid();
    private readonly TextBox _search = Ui.TextBox("Search product or barcode");

    public ProductsForm()
    {
        BackColor = Ui.Back;
        var top = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 52, Padding = new Padding(8), BackColor = Ui.Panel };
        var add = Ui.Button("Add/Edit");
        var del = Ui.Button("Delete");
        add.Click += (_, _) => EditSelected();
        del.Click += (_, _) => DeleteSelected();
        _search.Width = 300; _search.TextChanged += (_, _) => LoadData();
        top.Controls.Add(_search); top.Controls.Add(add); top.Controls.Add(del);
        Controls.Add(_grid); Controls.Add(top);
        Load += (_, _) => LoadData();
    }

    private void LoadData()
    {
        _grid.DataSource = Db.Query(@"SELECT p.Id,p.Barcode,p.Name,c.Name Category,p.CostPrice,p.SalePrice,p.StockQuantity,p.MinStockAlert,p.IsActive
FROM Products p JOIN Categories c ON c.Id=p.CategoryId WHERE p.Name LIKE $q OR p.Barcode LIKE $q ORDER BY p.Name", ("$q", "%" + _search.Text + "%"));
    }

    private void EditSelected()
    {
        var id = _grid.CurrentRow == null ? 0 : Convert.ToInt32(_grid.CurrentRow.Cells["Id"].Value);
        using var f = new ProductEditForm(id);
        if (f.ShowDialog() == DialogResult.OK) LoadData();
    }

    private void DeleteSelected()
    {
        if (_grid.CurrentRow == null) return;
        var id = Convert.ToInt32(_grid.CurrentRow.Cells["Id"].Value);
        if (MessageBox.Show("Deactivate selected product?", "Confirm", MessageBoxButtons.YesNo) != DialogResult.Yes) return;
        Db.Execute("UPDATE Products SET IsActive=0 WHERE Id=$id", ("$id", id));
        AuditService.Log("Delete", "Product", id.ToString());
        LoadData();
    }
}

public sealed class ProductEditForm : Form
{
    private readonly int _id;
    private readonly TextBox _barcode = Ui.TextBox("Barcode");
    private readonly TextBox _name = Ui.TextBox("Name");
    private readonly ComboBox _category = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 180 };
    private readonly NumericUpDown _cost = Ui.Money();
    private readonly NumericUpDown _sale = Ui.Money();
    private readonly NumericUpDown _stock = Ui.Money();
    private readonly NumericUpDown _min = Ui.Money();

    public ProductEditForm(int id)
    {
        _id = id;
        Text = id == 0 ? "Add Product" : "Edit Product";
        Size = new Size(420, 430); StartPosition = FormStartPosition.CenterParent; BackColor = Ui.Back;
        var cats = Db.Query("SELECT Id,Name FROM Categories ORDER BY Name");
        _category.DataSource = cats; _category.ValueMember = "Id"; _category.DisplayMember = "Name";
        var save = Ui.Button("Save"); save.Click += (_, _) => Save();
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(18), RowCount = 9 };
        foreach (var c in new Control[] { Ui.Label("Barcode"), _barcode, Ui.Label("Name"), _name, Ui.Label("Category"), _category, Ui.Label("Cost / Sale / Stock / Minimum"), new FlowLayoutPanel { Height = 36, Controls = { _cost, _sale, _stock, _min } }, save }) layout.Controls.Add(c);
        Controls.Add(layout);
        if (_id > 0) LoadProduct();
    }

    private void LoadProduct()
    {
        var r = Db.Query("SELECT * FROM Products WHERE Id=$id", ("$id", _id)).Rows[0];
        _barcode.Text = r["Barcode"].ToString(); _name.Text = r["Name"].ToString(); _category.SelectedValue = Convert.ToInt32(r["CategoryId"]);
        _cost.Value = Convert.ToDecimal(r["CostPrice"]); _sale.Value = Convert.ToDecimal(r["SalePrice"]); _stock.Value = Convert.ToDecimal(r["StockQuantity"]); _min.Value = Convert.ToDecimal(r["MinStockAlert"]);
    }

    private void Save()
    {
        if (string.IsNullOrWhiteSpace(_barcode.Text) || string.IsNullOrWhiteSpace(_name.Text)) { MessageBox.Show("Barcode and name are required."); return; }
        if (_id == 0)
            Db.Execute("INSERT INTO Products(Barcode,Name,CategoryId,CostPrice,SalePrice,StockQuantity,MinStockAlert) VALUES($b,$n,$c,$co,$s,$q,$m)", ("$b", _barcode.Text), ("$n", _name.Text), ("$c", _category.SelectedValue), ("$co", _cost.Value), ("$s", _sale.Value), ("$q", _stock.Value), ("$m", _min.Value));
        else
            Db.Execute("UPDATE Products SET Barcode=$b,Name=$n,CategoryId=$c,CostPrice=$co,SalePrice=$s,StockQuantity=$q,MinStockAlert=$m,UpdatedAt=CURRENT_TIMESTAMP WHERE Id=$id", ("$b", _barcode.Text), ("$n", _name.Text), ("$c", _category.SelectedValue), ("$co", _cost.Value), ("$s", _sale.Value), ("$q", _stock.Value), ("$m", _min.Value), ("$id", _id));
        AuditService.Log("Save", "Product", _name.Text);
        DialogResult = DialogResult.OK;
    }
}
