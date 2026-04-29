namespace MarketPOS.Utilities;

public static class Ui
{
    public static readonly Color Back = Color.FromArgb(24, 27, 33);
    public static readonly Color Panel = Color.FromArgb(34, 39, 47);
    public static readonly Color Accent = Color.FromArgb(38, 166, 154);
    public static readonly Color Text = Color.WhiteSmoke;

    public static Button Button(string text) => new() { Text = text, Height = 36, BackColor = Accent, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
    public static Label Label(string text, int size = 10) => new() { Text = text, ForeColor = Text, Font = new Font("Segoe UI", size, FontStyle.Regular), AutoSize = true };
    public static TextBox TextBox(string placeholder = "") => new() { PlaceholderText = placeholder, Height = 30 };
    public static NumericUpDown Money(decimal max = 1000000) => new() { DecimalPlaces = 2, Maximum = max, Minimum = 0, Width = 120 };
    public static DataGridView Grid() => new() { Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, BackgroundColor = Panel, ForeColor = Color.Black, SelectionMode = DataGridViewSelectionMode.FullRowSelect, MultiSelect = false };
}
