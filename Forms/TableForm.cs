using System.Data;
using MarketPOS.Repositories;
using MarketPOS.Utilities;

namespace MarketPOS.Forms;

public class TableForm : Form
{
    protected readonly DataGridView Grid = Ui.Grid();
    private readonly string _title;
    private readonly string _sql;

    public TableForm(string title, string sql)
    {
        _title = title; _sql = sql;
        BackColor = Ui.Back;
        var header = new Panel { Dock = DockStyle.Top, Height = 52, Padding = new Padding(12), BackColor = Ui.Panel };
        var refresh = Ui.Button("Refresh");
        refresh.Dock = DockStyle.Right;
        refresh.Click += (_, _) => LoadData();
        header.Controls.Add(refresh);
        header.Controls.Add(Ui.Label(title, 16));
        Controls.Add(Grid);
        Controls.Add(header);
        Load += (_, _) => LoadData();
    }

    protected virtual void LoadData()
    {
        try { Grid.DataSource = Db.Query(_sql); }
        catch (Exception ex) { MessageBox.Show(ex.Message, _title); }
    }
}
