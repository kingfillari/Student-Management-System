
using System;
using System.Drawing;
using System.Windows.Forms;

namespace StudentManagementSystem
{
    public class LoginForm : Form
    {
        private TextBox txtUsername = new();
        private TextBox txtPassword = new();
        private CheckBox chkShowPassword = new();
        private Label lblMessage = new();

        public LoginForm()
        {
            BuildInterface();
        }

        private void BuildInterface()
        {
            Text = "Student Management System - Login";
            Size = new Size(900, 600);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.FromArgb(13, 27, 42);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;

            Panel card = new Panel
            {
                Size = new Size(430, 470),
                Location = new Point(225, 45),
                BackColor = Color.White
            };
            Controls.Add(card);

            Label title = new Label
            {
                Text = "STUDENT MANAGEMENT",
                Font = new Font("Segoe UI", 19, FontStyle.Bold),
                ForeColor = Color.FromArgb(22, 48, 76),
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(25, 35),
                Size = new Size(380, 45)
            };
            card.Controls.Add(title);

            Label subtitle = new Label
            {
                Text = "Sign in to your account",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.Gray,
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(25, 85),
                Size = new Size(380, 30)
            };
            card.Controls.Add(subtitle);

            AddLabel(card, "Username", 55, 145);

            txtUsername = new TextBox
            {
                Location = new Point(55, 175),
                Size = new Size(320, 38),
                Font = new Font("Segoe UI", 12)
            };
            card.Controls.Add(txtUsername);

            AddLabel(card, "Password", 55, 235);

            txtPassword = new TextBox
            {
                Location = new Point(55, 265),
                Size = new Size(320, 38),
                Font = new Font("Segoe UI", 12),
                UseSystemPasswordChar = true
            };
            card.Controls.Add(txtPassword);

            chkShowPassword = new CheckBox
            {
                Text = "Show password",
                Location = new Point(55, 310),
                AutoSize = true,
                Font = new Font("Segoe UI", 9)
            };
            chkShowPassword.CheckedChanged += (s, e) =>
                txtPassword.UseSystemPasswordChar = !chkShowPassword.Checked;
            card.Controls.Add(chkShowPassword);

            Button btnLogin = CreateButton(
                "LOGIN", 55, 350, Color.FromArgb(25, 105, 180));

            btnLogin.Click += Login;
            card.Controls.Add(btnLogin);

            Button btnClear = CreateButton(
                "CLEAR", 55, 405, Color.FromArgb(100, 116, 139));

            btnClear.Click += (s, e) => ClearFields();
            card.Controls.Add(btnClear);

            Button btnExit = CreateButton(
                "EXIT", 220, 405, Color.FromArgb(190, 55, 55));

            btnExit.Click += (s, e) =>
            {
                if (MessageBox.Show(
                    "Are you sure you want to exit?",
                    "Exit Application",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    Application.Exit();
                }
            };
            card.Controls.Add(btnExit);

            lblMessage = new Label
            {
                Location = new Point(55, 325),
                Size = new Size(320, 22),
                ForeColor = Color.Red,
                TextAlign = ContentAlignment.MiddleCenter
            };
            card.Controls.Add(lblMessage);

            AcceptButton = btnLogin;
            txtUsername.Focus();
        }

        private void Login(object? sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text;

            lblMessage.Text = "";

            if (string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(password))
            {
                lblMessage.Text = "Enter your username and password.";
                return;
            }

            try
            {
                if (!Database.ValidateUser(username, password))
                {
                    lblMessage.Text = "Invalid username or password.";
                    txtPassword.Clear();
                    txtPassword.Focus();
                    return;
                }

                Hide();

                DashboardForm dashboard = new DashboardForm(() =>
                {
                    txtPassword.Clear();
                    Show();
                    txtUsername.Focus();
                });

                dashboard.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Login failed: " + ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void ClearFields()
        {
            txtUsername.Clear();
            txtPassword.Clear();
            lblMessage.Text = "";
            txtUsername.Focus();
        }

        private static void AddLabel(
            Control parent, string text, int x, int y)
        {
            parent.Controls.Add(new Label
            {
                Text = text,
                Location = new Point(x, y),
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(45, 55, 72)
            });
        }

        private static Button CreateButton(
            string text, int x, int y, Color color)
        {
            return new Button
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(155, 42),
                BackColor = color,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
        }
    }
}
