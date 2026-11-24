
using System;
using System.Drawing;
using System.Windows.Forms;

namespace StudentManagementSystem
{
    public class StudentForm : Form
    {
        private TextBox txtId = new();
        private TextBox txtName = new();
        private TextBox txtAge = new();
        private TextBox txtDepartment = new();
        private TextBox txtEmail = new();
        private TextBox txtPhone = new();
        private TextBox txtSearch = new();

        private ComboBox cmbGender = new();
        private DataGridView grid = new();

        public StudentForm()
        {
            BuildInterface();
            LoadStudents();
        }

        private void BuildInterface()
        {
            Text = "Student Registration and Management";
            Size = new Size(1250, 760);
            MinimumSize = new Size(1050, 650);
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.FromArgb(241, 245, 249);

            Panel header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 75,
                BackColor = Color.FromArgb(20, 42, 68)
            };
            Controls.Add(header);

            Label title = new Label
            {
                Text = "STUDENT REGISTRATION",
                Location = new Point(25, 20),
                AutoSize = true,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 19, FontStyle.Bold)
            };
            header.Controls.Add(title);

            Panel left = new Panel
            {
                Location = new Point(20, 95),
                Size = new Size(350, 590),
                BackColor = Color.White,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left
            };
            Controls.Add(left);

            Label formTitle = new Label
            {
                Text = "Student Information",
                Location = new Point(20, 15),
                AutoSize = true,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(20, 42, 68)
            };
            left.Controls.Add(formTitle);

            AddField(left, "Student ID", txtId, 65);
            AddField(left, "Full Name", txtName, 130);

            AddLabel(left, "Gender", 195);
            cmbGender = new ComboBox
            {
                Location = new Point(20, 220),
                Size = new Size(305, 32),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10)
            };
            cmbGender.Items.AddRange(new object[] { "Male", "Female", "Other" });
            left.Controls.Add(cmbGender);

            AddField(left, "Age", txtAge, 260);
            AddField(left, "Department", txtDepartment, 325);
            AddField(left, "Email", txtEmail, 390);
            AddField(left, "Phone", txtPhone, 455);

            Button add = CreateButton(
                "REGISTER", 20, 520, Color.FromArgb(16, 135, 90));

            add.Click += AddStudent;
            left.Controls.Add(add);

            Button update = CreateButton(
                "UPDATE", 190, 520, Color.FromArgb(25, 105, 180));

            update.Click += UpdateStudent;
            left.Controls.Add(update);

            Button clear = CreateButton(
                "CLEAR", 20, 565, Color.FromArgb(100, 116, 139));

            clear.Click += (s, e) => ClearFields();
            left.Controls.Add(clear);

            Button delete = CreateButton(
                "DELETE", 190, 565, Color.FromArgb(190, 55, 55));

            delete.Click += DeleteStudent;
            left.Controls.Add(delete);

            Panel right = new Panel
            {
                Location = new Point(390, 95),
                Size = new Size(825, 590),
                BackColor = Color.White,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom |
                         AnchorStyles.Left | AnchorStyles.Right
            };
            Controls.Add(right);

            Label recordsTitle = new Label
            {
                Text = "Student Records",
                Location = new Point(20, 18),
                AutoSize = true,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(20, 42, 68)
            };
            right.Controls.Add(recordsTitle);

            txtSearch = new TextBox
            {
                Location = new Point(20, 60),
                Size = new Size(570, 35),
                Font = new Font("Segoe UI", 11),
                PlaceholderText = "Search by student ID or full name..."
            };
            txtSearch.TextChanged += (s, e) => SearchStudents();
            right.Controls.Add(txtSearch);

            Button view = CreateButton(
                "VIEW ALL", 610, 57, Color.FromArgb(25, 105, 180));
            view.Click += (s, e) => LoadStudents();
            right.Controls.Add(view);

            grid = new DataGridView
            {
                Location = new Point(20, 115),
                Size = new Size(785, 450),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom |
                         AnchorStyles.Left | AnchorStyles.Right,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                MultiSelect = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                RowHeadersVisible = false,
                Font = new Font("Segoe UI", 9)
            };

            grid.ColumnHeadersDefaultCellStyle.BackColor =
                Color.FromArgb(20, 42, 68);
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            grid.ColumnHeadersDefaultCellStyle.Font =
                new Font("Segoe UI", 9, FontStyle.Bold);
            grid.EnableHeadersVisualStyles = false;
            grid.RowTemplate.Height = 32;
            grid.SelectionChanged += GridSelectionChanged;

            right.Controls.Add(grid);
        }

        private void AddStudent(object? sender, EventArgs e)
        {
            if (!ValidateFields(out int age))
                return;

            try
            {
                Database.AddStudent(
                    txtId.Text.Trim(),
                    txtName.Text.Trim(),
                    cmbGender.Text,
                    age,
                    txtDepartment.Text.Trim(),
                    txtEmail.Text.Trim(),
                    txtPhone.Text.Trim());

                MessageBox.Show(
                    "Student registered successfully!",
                    "Success",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                ClearFields();
                LoadStudents();
            }
            catch (Microsoft.Data.Sqlite.SqliteException ex)
                when (ex.SqliteErrorCode == 19)
            {
                MessageBox.Show(
                    "This Student ID already exists.",
                    "Duplicate ID",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                ShowError(ex);
            }
        }

        private void UpdateStudent(object? sender, EventArgs e)
        {
            if (!ValidateFields(out int age))
                return;

            try
            {
                Database.UpdateStudent(
                    txtId.Text.Trim(),
                    txtName.Text.Trim(),
                    cmbGender.Text,
                    age,
                    txtDepartment.Text.Trim(),
                    txtEmail.Text.Trim(),
                    txtPhone.Text.Trim());

                MessageBox.Show(
                    "Student record updated successfully.",
                    "Success",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                LoadStudents();
                ClearFields();
            }
            catch (Exception ex)
            {
                ShowError(ex);
            }
        }

        private void DeleteStudent(object? sender, EventArgs e)
        {
            string id = txtId.Text.Trim();

            if (string.IsNullOrWhiteSpace(id))
            {
                MessageBox.Show("Select a student to delete.");
                return;
            }

            if (MessageBox.Show(
                "Are you sure you want to delete student " + id + "?",
                "Confirm Delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning) != DialogResult.Yes)
                return;

            try
            {
                Database.DeleteStudent(id);
                MessageBox.Show("Student deleted successfully.");

                LoadStudents();
                ClearFields();
            }
            catch (Exception ex)
            {
                ShowError(ex);
            }
        }

        private bool ValidateFields(out int age)
        {
            age = 0;

            if (string.IsNullOrWhiteSpace(txtId.Text) ||
                string.IsNullOrWhiteSpace(txtName.Text) ||
                string.IsNullOrWhiteSpace(cmbGender.Text) ||
                string.IsNullOrWhiteSpace(txtAge.Text) ||
                string.IsNullOrWhiteSpace(txtDepartment.Text))
            {
                MessageBox.Show(
                    "Please complete Student ID, Name, Gender, Age and Department.",
                    "Validation",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return false;
            }

            if (!int.TryParse(txtAge.Text, out age) || age < 1 || age > 120)
            {
                MessageBox.Show("Enter a valid age between 1 and 120.");
                return false;
            }

            return true;
        }

        private void LoadStudents()
        {
            try
            {
                grid.DataSource = Database.GetAllStudents();
            }
            catch (Exception ex)
            {
                ShowError(ex);
            }
        }

        private void SearchStudents()
        {
            try
            {
                grid.DataSource =
                    Database.SearchStudents(txtSearch.Text.Trim());
            }
            catch (Exception ex)
            {
                ShowError(ex);
            }
        }

        private void GridSelectionChanged(object? sender, EventArgs e)
        {
            if (grid.CurrentRow == null ||
                grid.CurrentRow.IsNewRow ||
                grid.CurrentRow.Cells["StudentId"].Value == null)
                return;

            txtId.Text =
                grid.CurrentRow.Cells["StudentId"].Value.ToString() ?? "";

            txtName.Text =
                grid.CurrentRow.Cells["FullName"].Value.ToString() ?? "";

            cmbGender.Text =
                grid.CurrentRow.Cells["Gender"].Value.ToString() ?? "";

            txtAge.Text =
                grid.CurrentRow.Cells["Age"].Value.ToString() ?? "";

            txtDepartment.Text =
                grid.CurrentRow.Cells["Department"].Value.ToString() ?? "";

            txtEmail.Text =
                grid.CurrentRow.Cells["Email"].Value?.ToString() ?? "";

            txtPhone.Text =
                grid.CurrentRow.Cells["Phone"].Value?.ToString() ?? "";
        }

        private void ClearFields()
        {
            txtId.Clear();
            txtName.Clear();
            txtAge.Clear();
            txtDepartment.Clear();
            txtEmail.Clear();
            txtPhone.Clear();
            cmbGender.SelectedIndex = -1;
            grid.ClearSelection();
            txtId.Focus();
        }

        private static void AddLabel(Control parent, string text, int y)
        {
            parent.Controls.Add(new Label
            {
                Text = text,
                Location = new Point(20, y),
                AutoSize = true,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.FromArgb(45, 55, 72)
            });
        }

        private static void AddField(
            Control parent, string label, TextBox textbox, int y)
        {
            AddLabel(parent, label, y);

            textbox.Location = new Point(20, y + 25);
            textbox.Size = new Size(305, 30);
            textbox.Font = new Font("Segoe UI", 10);

            parent.Controls.Add(textbox);
        }

        private static Button CreateButton(
            string text, int x, int y, Color color)
        {
            return new Button
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(155, 38),
                BackColor = color,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
        }

        private void ShowError(Exception ex)
        {
            MessageBox.Show(
                ex.Message,
                "Application Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }
}
