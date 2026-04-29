using MarketPOS.Repositories;
using MarketPOS.Services;
using MarketPOS.Utilities;

namespace MarketPOS.Forms;

public sealed class SuppliersForm : Form
{
    private readonly DataGridView _grid = Ui.Grid();
    public SuppliersForm()
    {
        BackColor = Ui.Back;
        var top = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 52, Padding = new Padding(8), BackColor = Ui.Panel };
        var add = Ui.Button("Add/Edit"); var del = Ui.Button("Delete");
        add.Click += (_, _) => Edit(); del.Click += (_, _) => Delete();
        top.Controls.AddRange(new Control[] { add, del });
        Controls.Add(_grid); Controls.Add(top);
        Load += (_, _) => LoadData();
    }
    private void LoadData() => _grid.DataSource = Db.Query("SELECT * FROM Suppliers ORDER BY Name");
    private void Edit()
    {
        var id = _grid.CurrentRow == null ? 0 : Convert.ToInt32(_grid.CurrentRow.Cells["Id"].Value);
        using var f = new SupplierEditForm(id);
        if (f.ShowDialog() == DialogResult.OK) LoadData();
    }
    private void Delete()
    {
        if (_grid.CurrentRow == null) return;
        Db.Execute("DELETE FROM Suppliers WHERE Id=$id", ("$id", Convert.ToInt32(_grid.CurrentRow.Cells["Id"].Value)));
        LoadData();
    }
}

public sealed class SupplierEditForm : Form
{
    private readonly int _id; private readonly TextBox _name = Ui.TextBox("Name"); private readonly TextBox _phone = Ui.TextBox("Phone"); private readonly TextBox _email = Ui.TextBox("Email"); private readonly TextBox _address = Ui.TextBox("Address");
    public SupplierEditForm(int id)
    {
        _id = id; Text = "Supplier"; Size = new Size(380, 330); StartPosition = FormStartPosition.CenterParent; BackColor = Ui.Back;
        var save = Ui.Button("Save"); save.Click += (_, _) => Save();
        var p = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(18) };
        foreach (var c in new Control[] { _name, _phone, _email, _address, save }) p.Controls.Add(c);
        Controls.Add(p);
        if (id > 0) { var r = Db.Query("SELECT * FROM Suppliers WHERE Id=$id", ("$id", id)).Rows[0]; _name.Text = r["Name"].ToString(); _phone.Text = r["Phone"].ToString(); _email.Text = r["Email"].ToString(); _address.Text = r["Address"].ToString(); }
    }
    private void Save()
    {
        if (_id == 0) Db.Execute("INSERT INTO Suppliers(Name,Phone,Email,Address) VALUES($n,$p,$e,$a)", ("$n", _name.Text), ("$p", _phone.Text), ("$e", _email.Text), ("$a", _address.Text));
        else Db.Execute("UPDATE Suppliers SET Name=$n,Phone=$p,Email=$e,Address=$a WHERE Id=$id", ("$n", _name.Text), ("$p", _phone.Text), ("$e", _email.Text), ("$a", _address.Text), ("$id", _id));
        AuditService.Log("Save", "Supplier", _name.Text); DialogResult = DialogResult.OK;
    }
}
