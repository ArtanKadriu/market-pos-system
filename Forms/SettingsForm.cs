using MarketPOS.Database;
using MarketPOS.Services;
using MarketPOS.Utilities;

namespace MarketPOS.Forms;

public sealed class SettingsForm : Form
{
    private readonly TextBox _store = Ui.TextBox("Store name");
    private readonly TextBox _currency = Ui.TextBox("Currency");
    private readonly NumericUpDown _tax = Ui.Money(100);
    private readonly TextBox _footer = Ui.TextBox("Receipt footer");
    public SettingsForm()
    {
        BackColor = Ui.Back;
        var save = Ui.Button("Save Settings"); save.Click += (_, _) => Save();
        var backup = Ui.Button("Backup Database"); backup.Click += (_, _) => Backup();
        var restore = Ui.Button("Restore Database"); restore.Click += (_, _) => Restore();
        var p = new TableLayoutPanel { Dock = DockStyle.Top, Height = 300, Padding = new Padding(18) };
        foreach (var c in new Control[] { Ui.Label("Store Name"), _store, Ui.Label("Currency"), _currency, Ui.Label("Tax Rate %"), _tax, Ui.Label("Receipt Footer"), _footer, new FlowLayoutPanel { Height = 42, Controls = { save, backup, restore } }, Ui.Label("Database: " + DatabaseInitializer.DatabasePath) }) p.Controls.Add(c);
        Controls.Add(p);
        Load += (_, _) => { var s = new SettingsService().Get(); _store.Text = s.StoreName; _currency.Text = s.Currency; _tax.Value = s.TaxRate; _footer.Text = s.ReceiptFooter; };
    }
    private void Save() { new SettingsService().Save(new() { StoreName = _store.Text, Currency = _currency.Text, TaxRate = _tax.Value, ReceiptFooter = _footer.Text }); MessageBox.Show("Settings saved."); }
    private void Backup() { using var d = new SaveFileDialog { Filter = "SQLite database|*.db", FileName = "marketpos-backup.db" }; if (d.ShowDialog() == DialogResult.OK) BackupService.Backup(d.FileName); }
    private void Restore() { using var d = new OpenFileDialog { Filter = "SQLite database|*.db;*.sqlite" }; if (d.ShowDialog() == DialogResult.OK) { BackupService.Restore(d.FileName); MessageBox.Show("Database restored. Restart the app."); } }
}
