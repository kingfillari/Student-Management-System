
using System;
using System.Drawing;
using System.Windows.Forms;

namespace StudentManagementSystem
{
    public class DashboardForm : Form
    {
        private readonly Action onLogout;
        private Label lblTotal = new();

        public DashboardForm(Action logoutCallback)
        {
            onLogout = logoutCallback;
            BuildInterface();
            RefreshStatistics();
        }

        private void BuildInterface()
        {
            Text = "Student Management System - Dashboard";
            Size = new Size(1100, 700);
            MinimumSize = new Size(900, 600);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.FromArgb(241, 245, 249);

            Panel header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 90,
                BackColor = Color.FromArgb(20, 42, 68)
            };
            Controls.Add(header);

            Label title = new Label
            {
                Text = "STUDENT MANAGEMENT SYSTEM",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 19, FontStyle.Bold),
                Location = new Point(30, 25),
                AutoSize = true
            };
            header.Controls.Add(title);

            Button logout = new Button
            {
                Text = "LOG OUT",
                Size = new Size(120, 40),
                Location = new Point(940, 25),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                BackColor = Color.FromArgb(190, 55, 55),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };

            logout.Click += (s, e) =>
            {
                if (MessageBox.Show(
                    "Do you want to log out?",
                    "Logout",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    Close();
                    onLogout();
                }
            };

            header.Controls.Add(logout);

            Label welcome = new Label
            {
                Text = "Welcome to your dashboard",
                Font = new Font("Segoe UI", 22, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 41, 59),
                Location = new Point(45, 125),
                AutoSize = true
            };
            Controls.Add(welcome);

            Label description = new Label
            {
                Text = "Manage student information and registration records.",
                Font = new Font("Segoe UI", 11),
                ForeColor = Color.Gray,
                Location = new Point(48, 175),
                AutoSize = true
            };
            Controls.Add(description);

            Panel stats = new Panel
            {
                Location = new Point(45, 235),
                Size = new Size(430, 160),
                BackColor = Color.White
            };
            Controls.Add(stats);

            Label statsTitle = new Label
            {
                Text = "TOTAL REGISTERED STUDENTS",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.Gray,
                Location = new Point(25, 25),
                AutoSize = true
            };
            stats.Controls.Add(statsTitle);

            lblTotal = new Label
            {
                Text = "0",
                Font = new Font("Segoe UI", 36, FontStyle.Bold),
                ForeColor = Color.FromArgb(25, 105, 180),
                Location = new Point(25, 60),
                AutoSize = true
            };
            stats.Controls.Add(lblTotal);

            Button manage = new Button
            {
                Text = "MANAGE STUDENTS",
                Location = new Point(45, 435),
                Size = new Size(300, 65),
                BackColor = Color.FromArgb(25, 105, 180),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Cursor = Cursors.Hand
            };

            manage.Click += (s, e) =>
            {
                using StudentForm studentForm = new StudentForm();
                studentForm.ShowDialog(this);
                RefreshStatistics();
            };

            Controls.Add(manage);

            Label footer = new Label
            {
                Text = "Student Management System | C# .NET 10 | SQLite",
                Dock = DockStyle.Bottom,
                Height = 35,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.Gray,
                Font = new Font("Segoe UI", 9)
            };
            Controls.Add(footer);
        }

        private void RefreshStatistics()
        {
            try
            {
                lblTotal.Text = Database.GetStudentCount().ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Unable to load dashboard statistics: " + ex.Message,
                    "Database Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}
