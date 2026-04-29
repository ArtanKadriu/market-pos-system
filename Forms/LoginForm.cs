using MarketPOS.Services;
using MarketPOS.Utilities;

namespace MarketPOS.Forms;

public sealed class LoginForm : Form
{
    private readonly TextBox _user = Ui.TextBox("Username");
    private readonly TextBox _pass = Ui.TextBox("Password");

    public LoginForm()
    {
        Text = "MarketPOS Login";
        Size = new Size(420, 300);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Ui.Back;
        _pass.UseSystemPasswordChar = true;
        var login = Ui.Button("Login");
        login.Click += (_, _) => DoLogin();
        var panel = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(45), RowCount = 7 };
        panel.Controls.Add(Ui.Label("MarketPOS", 24));
        panel.Controls.Add(Ui.Label("Default admin: admin / admin123"));
        panel.Controls.Add(_user);
        panel.Controls.Add(_pass);
        panel.Controls.Add(login);
        Controls.Add(panel);
        AcceptButton = login;
    }

    private void DoLogin()
    {
        try
        {
            var user = new AuthService().Login(_user.Text, _pass.Text);
            if (user is null) { MessageBox.Show("Invalid login."); return; }
            AppSession.CurrentUser = user;
            AuditService.Log("Login", "User", user.Username);
            Hide();
            new MainForm().ShowDialog();
            Close();
        }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Login error"); }
    }
}
