using MarketPOS.Services;
using MarketPOS.Utilities;

namespace MarketPOS.Forms;

public sealed class MainForm : Form
{
    private readonly Panel _content = new() { Dock = DockStyle.Fill, BackColor = Ui.Back };

    public MainForm()
    {
        Text = "MarketPOS - Supermarket POS and Inventory";
        WindowState = FormWindowState.Maximized;
        BackColor = Ui.Back;
        var side = new FlowLayoutPanel { Dock = DockStyle.Left, Width = 190, BackColor = Color.FromArgb(18, 21, 26), FlowDirection = FlowDirection.TopDown, Padding = new Padding(10), WrapContents = false };
        side.Controls.Add(Ui.Label($"MarketPOS\n{AppSession.CurrentUser.FullName}", 12));
        AddNav(side, "Dashboard", new DashboardForm());
        AddNav(side, "POS Cashier", new PosForm());
        AddNav(side, "Products", new ProductsForm(), AppSession.IsManagerOrAdmin);
        AddNav(side, "Inventory", new InventoryForm(), AppSession.IsManagerOrAdmin);
        AddNav(side, "Suppliers", new SuppliersForm(), AppSession.IsManagerOrAdmin);
        AddNav(side, "Purchases", new PurchasesForm(), AppSession.IsManagerOrAdmin);
        AddNav(side, "Reports", new ReportsForm(), AppSession.IsManagerOrAdmin);
        AddNav(side, "Returns", new ReturnsForm());
        AddNav(side, "Settings", new SettingsForm(), AppSession.IsAdmin);
        AddNav(side, "Audit Log", new TableForm("AuditLogs", "SELECT * FROM AuditLogs ORDER BY Id DESC"), AppSession.IsAdmin);
        var logout = Ui.Button("Logout");
        logout.Click += (_, _) => Close();
        side.Controls.Add(logout);
        Controls.Add(_content);
        Controls.Add(side);
        LoadForm(new DashboardForm());
    }

    private void AddNav(Control side, string text, Form form, bool enabled = true)
    {
        var b = Ui.Button(text);
        b.Width = 165;
        b.Enabled = enabled;
        b.Click += (_, _) => LoadForm(form);
        side.Controls.Add(b);
    }

    private void LoadForm(Form f)
    {
        _content.Controls.Clear();
        f.TopLevel = false;
        f.FormBorderStyle = FormBorderStyle.None;
        f.Dock = DockStyle.Fill;
        _content.Controls.Add(f);
        f.Show();
    }
}
