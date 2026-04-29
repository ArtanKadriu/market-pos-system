using MarketPOS.Services;
using MarketPOS.Utilities;

namespace MarketPOS.Forms;

public sealed class ReturnsForm : Form
{
    public ReturnsForm()
    {
        BackColor = Ui.Back;
        var invoice = Ui.TextBox("Invoice number"); invoice.Width = 260;
        var refund = Ui.Button("Refund Invoice");
        refund.Click += (_, _) => { try { new SaleService().Refund(invoice.Text.Trim()); MessageBox.Show("Refund completed and stock returned."); } catch (Exception ex) { MessageBox.Show(ex.Message); } };
        var p = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 70, Padding = new Padding(18), BackColor = Ui.Panel };
        p.Controls.AddRange(new Control[] { invoice, refund });
        var table = new TableForm("Recent Sales", "SELECT InvoiceNo, SaleDate, Total, PaymentMethod, IsReturned FROM Sales ORDER BY Id DESC")
        {
            TopLevel = false,
            FormBorderStyle = FormBorderStyle.None,
            Dock = DockStyle.Fill
        };
        Controls.Add(table);
        Controls.Add(p);
        table.Show();
    }
}
