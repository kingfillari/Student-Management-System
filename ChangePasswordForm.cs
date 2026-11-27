using System.Drawing;

namespace StudentManagementSystem;

internal sealed class ChangePasswordForm : Form
{
    public ChangePasswordForm(AppUser user)
    {
        Text = "Change Password";
        StartPosition = FormStartPosition.CenterParent;
        Size = new Size(430, 360);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(22), ColumnCount = 1, RowCount = 7 };
        Controls.Add(layout);
        layout.Controls.Add(new Label { Text = "Change your account password", Font = new Font("Segoe UI", 14, FontStyle.Bold), AutoSize = true });
        var current = AddField(layout, "Current password");
        var next = AddField(layout, "New password");
        var confirm = AddField(layout, "Confirm new password");
        next.UseSystemPasswordChar = current.UseSystemPasswordChar = confirm.UseSystemPasswordChar = true;
        var show = new CheckBox { Text = "Show passwords", AutoSize = true };
        show.CheckedChanged += (_, _) => current.UseSystemPasswordChar = next.UseSystemPasswordChar = confirm.UseSystemPasswordChar = !show.Checked;
        layout.Controls.Add(show);
        var save = new Button { Text = "Update Password", Height = 38, BackColor = Color.FromArgb(35, 85, 145), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
        save.Click += (_, _) =>
        {
            if (next.Text.Length < 8)
            {
                MessageBox.Show("Use at least 8 characters for the new password.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (next.Text != confirm.Text)
            {
                MessageBox.Show("The new passwords do not match.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            try
            {
                Database.ChangePassword(user.Id, current.Text, next.Text);
                MessageBox.Show("Password changed successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Close();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Unable to change password", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        };
        layout.Controls.Add(save);
    }

    private static TextBox AddField(Control parent, string label)
    {
        parent.Controls.Add(new Label { Text = label, AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold) });
        var box = new TextBox { Dock = DockStyle.Top, Font = new Font("Segoe UI", 10), Margin = new Padding(0, 3, 0, 8) };
        parent.Controls.Add(box);
        return box;
    }
}
