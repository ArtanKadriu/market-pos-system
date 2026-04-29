using System.Drawing.Printing;
using MarketPOS.Utilities;

namespace MarketPOS.Forms;

public sealed class ReceiptForm : Form
{
    private readonly TextBox _text = new() { Dock = DockStyle.Fill, Multiline = true, ReadOnly = true, Font = new Font("Consolas", 10), ScrollBars = ScrollBars.Vertical };
    public ReceiptForm(string receipt)
    {
        Text = "Receipt"; Size = new Size(430, 560); StartPosition = FormStartPosition.CenterParent;
        _text.Text = receipt;
        var print = Ui.Button("Print / ESC-POS Placeholder");
        print.Dock = DockStyle.Bottom;
        print.Click += (_, _) => PrintReceipt();
        Controls.Add(_text); Controls.Add(print);
    }

    private void PrintReceipt()
    {
        var doc = new PrintDocument();
        doc.PrintPage += (_, e) => e.Graphics?.DrawString(_text.Text, _text.Font, Brushes.Black, 10, 10);
        using var dlg = new PrintDialog { Document = doc };
        if (dlg.ShowDialog() == DialogResult.OK) doc.Print();
    }
}
