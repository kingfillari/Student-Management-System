using System.Data;
using System.Drawing;

namespace StudentManagementSystem;

internal sealed class DashboardForm : Form
{
    private readonly AppUser user;
    private readonly Label studentsValue = new();
    private readonly Label usersValue = new();
    private readonly DataGridView departmentGrid = new();

    public DashboardForm(AppUser currentUser)
    {
        user = currentUser;
        Text = "Student Management System | Dashboard";
        StartPosition = FormStartPosition.CenterScreen;
        Size = new Size(1100, 720);
        MinimumSize = new Size(950, 620);
        BackColor = Color.FromArgb(242, 245, 250);

        var header = new Panel { Dock = DockStyle.Top, Height = 82, BackColor = Color.FromArgb(17, 36, 65) };
        Controls.Add(header);
        header.Controls.Add(new Label { Text = "STUDENT MANAGEMENT SYSTEM", ForeColor = Color.White,
            Font = new Font("Segoe UI", 17, FontStyle.Bold), AutoSize = true, Location = new Point(25, 13) });
        header.Controls.Add(new Label { Text = $"Signed in: {user.Username}  |  Role: {user.Role}",
            ForeColor = Color.FromArgb(210, 220, 235), Font = new Font("Segoe UI", 9), AutoSize = true,
            Location = new Point(28, 48) });

        var actions = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 66, Padding = new Padding(18, 12, 0, 0),
            BackColor = Color.White, WrapContents = false };
        Controls.Add(actions);
        AddAction(actions, "Manage Students", () => new StudentForm(user).ShowDialog(this));
        AddAction(actions, "Change Password", () => new ChangePasswordForm(user).ShowDialog(this));
        if (user.Role == "Admin")
            AddAction(actions, "Manage Users", () => new UserManagementForm(user).ShowDialog(this));
        AddAction(actions, "Backup Database", BackupDatabase);
        AddAction(actions, "Restore Database", RestoreDatabase);
        AddAction(actions, "Refresh", RefreshDashboard);
        AddAction(actions, "Logout", Logout, Color.FromArgb(160, 55, 55));

        var content = new Panel { Dock = DockStyle.Fill, Padding = new Padding(24) };
        Controls.Add(content);
        content.BringToFront();

        var welcome = new Label { Text = "Dashboard Overview", Font = new Font("Segoe UI", 20, FontStyle.Bold),
            ForeColor = Color.FromArgb(25, 45, 75), AutoSize = true, Location = new Point(25, 20) };
        content.Controls.Add(welcome);

        AddStatCard(content, "TOTAL STUDENTS", studentsValue, 25, 75, Color.FromArgb(35, 105, 180));
        AddStatCard(content, "SYSTEM USERS", usersValue, 340, 75, Color.FromArgb(30, 135, 110));

        content.Controls.Add(new Label { Text = "Students by Department", Font = new Font("Segoe UI", 14, FontStyle.Bold),
            AutoSize = true, Location = new Point(25, 245) });
        departmentGrid.SetBounds(25, 280, 650, 300);
        departmentGrid.ReadOnly = true;
        departmentGrid.AllowUserToAddRows = false;
        departmentGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        departmentGrid.BackgroundColor = Color.White;
        departmentGrid.RowHeadersVisible = false;
        departmentGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        content.Controls.Add(departmentGrid);

        var note = new Label { Text = "Tip: Use Manage Students to register, search, update, delete, filter and export records.",
            AutoSize = false, Size = new Size(340, 100), Location = new Point(710, 285),
            Font = new Font("Segoe UI", 11), ForeColor = Color.FromArgb(70, 80, 95) };
        content.Controls.Add(note);

        RefreshDashboard();
        FormClosing += (_, e) =>
        {
            if (DialogResult != DialogResult.OK && !Confirm("Close the application?"))
                e.Cancel = true;
        };
    }

    private static void AddStatCard(Control parent, string caption, Label value, int x, int y, Color color)
    {
        var card = new Panel { Location = new Point(x, y), Size = new Size(285, 135), BackColor = color };
        card.Controls.Add(new Label { Text = caption, ForeColor = Color.White, Font = new Font("Segoe UI", 10, FontStyle.Bold),
            Location = new Point(20, 20), AutoSize = true });
        value.Text = "0";
        value.ForeColor = Color.White;
        value.Font = new Font("Segoe UI", 30, FontStyle.Bold);
        value.Location = new Point(20, 52);
        value.AutoSize = true;
        card.Controls.Add(value);
        parent.Controls.Add(card);
    }

    private static void AddAction(FlowLayoutPanel panel, string text, Action action, Color? color = null)
    {
        var button = new Button { Text = text, Width = 135, Height = 38, Margin = new Padding(5, 0, 5, 0),
            BackColor = color ?? Color.FromArgb(35, 85, 145), ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 8, FontStyle.Bold) };
        button.Click += (_, _) => action();
        panel.Controls.Add(button);
    }

    private void RefreshDashboard()
    {
        studentsValue.Text = Database.StudentCount().ToString();
        usersValue.Text = Database.UserCount().ToString();
        departmentGrid.DataSource = Database.DepartmentSummary();
    }

    private static bool Confirm(string message) =>
        MessageBox.Show(message, "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;

    private void BackupDatabase()
    {
        using var dialog = new SaveFileDialog { Filter = "SQLite database (*.db)|*.db",
            FileName = $"students-backup-{DateTime.Now:yyyyMMdd-HHmm}.db" };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        try
        {
            File.Copy(Database.DatabasePath, dialog.FileName, true);
            MessageBox.Show("Database backup completed.", "Backup", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Backup error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    private void RestoreDatabase()
    {
        if (!Confirm("Restore a backup? Current database data will be replaced.")) return;
        using var dialog = new OpenFileDialog { Filter = "SQLite database (*.db)|*.db" };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        try
        {
            File.Copy(dialog.FileName, Database.DatabasePath, true);
            RefreshDashboard();
            MessageBox.Show("Database restored successfully.", "Restore", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Restore error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    private void Logout()
    {
        if (!Confirm("Are you sure you want to log out?")) return;
        Database.Log(user.Username, "Logout", "User logged out");
        DialogResult = DialogResult.OK;
        Close();
    }
}
