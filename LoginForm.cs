using System.Drawing;

namespace StudentManagementSystem;

internal sealed class LoginForm : Form
{
    private readonly TextBox usernameBox = new();
    private readonly TextBox passwordBox = new();
    private readonly CheckBox showPassword = new();

    public LoginForm()
    {
        Text = "Student Management System | Sign In";
        StartPosition = FormStartPosition.CenterScreen;
        Size = new Size(520, 540);
        MinimumSize = new Size(480, 500);
        BackColor = Color.FromArgb(14, 25, 45);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;

        var card = new Panel { BackColor = Color.White, Size = new Size(390, 410), Location = new Point(55, 45) };
        Controls.Add(card);

        var title = new Label { Text = "STUDENT MANAGEMENT", Font = new Font("Segoe UI", 17, FontStyle.Bold),
            ForeColor = Color.FromArgb(25, 45, 75), AutoSize = false, TextAlign = ContentAlignment.MiddleCenter,
            Location = new Point(20, 35), Size = new Size(350, 38) };
        card.Controls.Add(title);
        card.Controls.Add(new Label { Text = "Sign in to continue", Font = new Font("Segoe UI", 10),
            ForeColor = Color.Gray, AutoSize = false, TextAlign = ContentAlignment.MiddleCenter,
            Location = new Point(20, 75), Size = new Size(350, 25) });

        AddLabel(card, "Username", 35, 125);
        usernameBox.SetBounds(35, 153, 320, 38);
        StyleTextBox(usernameBox);
        card.Controls.Add(usernameBox);

        AddLabel(card, "Password", 35, 207);
        passwordBox.SetBounds(35, 235, 320, 38);
        passwordBox.UseSystemPasswordChar = true;
        StyleTextBox(passwordBox);
        card.Controls.Add(passwordBox);

        showPassword.Text = "Show password";
        showPassword.SetBounds(35, 280, 150, 25);
        showPassword.CheckedChanged += (_, _) => passwordBox.UseSystemPasswordChar = !showPassword.Checked;
        card.Controls.Add(showPassword);

        var login = MakeButton("SIGN IN", 35, 325, 150, 42, Color.FromArgb(32, 92, 170));
        login.Click += Login;
        card.Controls.Add(login);
        var exit = MakeButton("EXIT", 205, 325, 150, 42, Color.FromArgb(95, 105, 120));
        exit.Click += (_, _) => Close();
        card.Controls.Add(exit);

        AcceptButton = login;
        Shown += (_, _) => usernameBox.Focus();
    }

    private static void AddLabel(Control parent, string text, int x, int y) =>
        parent.Controls.Add(new Label { Text = text, Location = new Point(x, y), AutoSize = true,
            Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.FromArgb(50, 60, 75) });

    private static void StyleTextBox(TextBox box)
    {
        box.Font = new Font("Segoe UI", 11);
        box.BorderStyle = BorderStyle.FixedSingle;
    }

    private static Button MakeButton(string text, int x, int y, int w, int h, Color color) =>
        new() { Text = text, Location = new Point(x, y), Size = new Size(w, h),
            BackColor = color, ForeColor = Color.White, FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9, FontStyle.Bold), Cursor = Cursors.Hand };

    private void Login(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(usernameBox.Text) || string.IsNullOrEmpty(passwordBox.Text))
        {
            MessageBox.Show("Enter both username and password.", "Validation",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (!Database.ValidateUser(usernameBox.Text, passwordBox.Text, out AppUser? user) || user is null)
        {
            MessageBox.Show("Invalid username or password.", "Login failed",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            passwordBox.Clear();
            passwordBox.Focus();
            return;
        }

        Hide();
        using (var dashboard = new DashboardForm(user))
            dashboard.ShowDialog();
        passwordBox.Clear();
        Show();
        Activate();
    }
}
