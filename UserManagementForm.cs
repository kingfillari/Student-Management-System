using System.Drawing;

namespace StudentManagementSystem;

internal sealed class UserManagementForm : Form
{
    private readonly DataGridView grid = new();
    private readonly TextBox username = new();
    private readonly TextBox password = new();
    private readonly ComboBox role = new();

    public UserManagementForm(AppUser currentUser)
    {
        if (currentUser.Role != "Admin") throw new UnauthorizedAccessException("Administrator access is required.");
        Text = "User Management";
        StartPosition = FormStartPosition.CenterParent;
        Size = new Size(850, 570);
        BackColor = Color.FromArgb(242, 245, 250);

        var panel = new Panel { Dock = DockStyle.Top, Height = 145, BackColor = Color.White, Padding = new Padding(15) };
        Controls.Add(panel);
        panel.Controls.Add(new Label { Text = "Create account", Font = new Font("Segoe UI", 14, FontStyle.Bold), AutoSize = true, Location = new Point(15, 10) });
        AddLabel(panel, "Username", 18, 52); username.SetBounds(18, 78, 190, 30); panel.Controls.Add(username);
        AddLabel(panel, "Temporary password", 225, 52); password.SetBounds(225, 78, 190, 30); password.UseSystemPasswordChar = true; panel.Controls.Add(password);
        AddLabel(panel, "Role", 432, 52); role.SetBounds(432, 78, 130, 30); role.DropDownStyle = ComboBoxStyle.DropDownList;
        role.Items.AddRange(new object[] { "Staff", "Admin" }); role.SelectedIndex = 0; panel.Controls.Add(role);
        var add = new Button { Text = "Create User", Location = new Point(580, 76), Size = new Size(125, 34),
            BackColor = Color.FromArgb(35, 85, 145), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
        add.Click += AddUser; panel.Controls.Add(add);

        grid.Dock = DockStyle.Fill; grid.ReadOnly = true; grid.AllowUserToAddRows = false;
        grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect; grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        grid.RowHeadersVisible = false; grid.BackgroundColor = Color.White;
        Controls.Add(grid);

        var bottom = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 55, Padding = new Padding(12) };
        var delete = new Button { Text = "Delete Selected User", Width = 170, Height = 32 };
        delete.Click += DeleteUser; bottom.Controls.Add(delete);
        var refresh = new Button { Text = "Refresh", Width = 100, Height = 32 };
        refresh.Click += (_, _) => LoadUsers(); bottom.Controls.Add(refresh);
        Controls.Add(bottom);
        LoadUsers();
    }

    private static void AddLabel(Control parent, string text, int x, int y) =>
        parent.Controls.Add(new Label { Text = text, Location = new Point(x, y), AutoSize = true });

    private void LoadUsers() => grid.DataSource = Database.GetUsers();

    private void AddUser(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(username.Text) || password.Text.Length < 8)
        {
            MessageBox.Show("Enter a username and a password of at least 8 characters.", "Validation",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        try
        {
            Database.CreateUser(username.Text, password.Text, role.Text);
            MessageBox.Show("User account created.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            username.Clear(); password.Clear(); role.SelectedIndex = 0; LoadUsers();
        }
        catch (Exception ex) { MessageBox.Show("Could not create user. The username may already exist.\n" + ex.Message,
            "Create user", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    private void DeleteUser(object? sender, EventArgs e)
    {
        if (grid.CurrentRow is null) return;
        int id = Convert.ToInt32(grid.CurrentRow.Cells["Id"].Value);
        string selectedName = Convert.ToString(grid.CurrentRow.Cells["Username"].Value) ?? "";
        if (selectedName.Equals("admin", StringComparison.OrdinalIgnoreCase))
        {
            MessageBox.Show("The default admin account cannot be deleted.", "Restricted", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        if (MessageBox.Show($"Delete user '{selectedName}'?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            return;
        try { Database.DeleteUser(id); LoadUsers(); }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Delete error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }
}
