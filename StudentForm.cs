using System.Data;
using System.Drawing;
using System.Text;

namespace StudentManagementSystem;

internal sealed class StudentForm : Form
{
    private readonly AppUser user;
    private readonly TextBox studentId = new();
    private readonly TextBox fullName = new();
    private readonly ComboBox gender = new();
    private readonly NumericUpDown age = new();
    private readonly TextBox department = new();
    private readonly TextBox email = new();
    private readonly TextBox phone = new();
    private readonly TextBox search = new();
    private readonly ComboBox departmentFilter = new();
    private readonly ComboBox genderFilter = new();
    private readonly DataGridView grid = new();
    private int selectedDatabaseId = 0;

    public StudentForm(AppUser currentUser)
    {
        user = currentUser;
        Text = "Student Records";
        StartPosition = FormStartPosition.CenterParent;
        Size = new Size(1250, 760);
        MinimumSize = new Size(1050, 650);
        BackColor = Color.FromArgb(242, 245, 250);

        var header = new Panel { Dock = DockStyle.Top, Height = 65, BackColor = Color.FromArgb(17, 36, 65) };
        header.Controls.Add(new Label { Text = "STUDENT RECORD MANAGEMENT", ForeColor = Color.White,
            Font = new Font("Segoe UI", 16, FontStyle.Bold), AutoSize = true, Location = new Point(22, 17) });
        Controls.Add(header);

        var body = new SplitContainer { Dock = DockStyle.Fill, SplitterDistance = 355, BackColor = Color.FromArgb(242, 245, 250), Padding = new Padding(12) };
        Controls.Add(body);
        BuildEntryPanel(body.Panel1);
        BuildRecordsPanel(body.Panel2);
        LoadDepartments();
        LoadRecords();
    }

    private void BuildEntryPanel(Control parent)
    {
        var panel = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(15), AutoScroll = true };
        parent.Controls.Add(panel);
        panel.Controls.Add(new Label { Text = "Student Information", Font = new Font("Segoe UI", 14, FontStyle.Bold),
            AutoSize = true, Location = new Point(15, 12) });

        int y = 55;
        AddInput(panel, "Student ID", studentId, ref y);
        AddInput(panel, "Full name", fullName, ref y);
        gender.DropDownStyle = ComboBoxStyle.DropDownList;
        gender.Items.AddRange(new object[] { "Male", "Female", "Other" });
        gender.SelectedIndex = 0;
        AddInput(panel, "Gender", gender, ref y);
        age.Minimum = 1; age.Maximum = 120; age.Value = 18;
        AddInput(panel, "Age", age, ref y);
        AddInput(panel, "Department", department, ref y);
        AddInput(panel, "Email", email, ref y);
        AddInput(panel, "Phone", phone, ref y);

        var register = ActionButton("Register", 15, y + 8, Color.FromArgb(30, 130, 90));
        register.Click += AddStudent; panel.Controls.Add(register);
        var update = ActionButton("Update", 175, y + 8, Color.FromArgb(35, 85, 145));
        update.Click += UpdateStudent; panel.Controls.Add(update);
        var clear = ActionButton("Clear", 15, y + 55, Color.FromArgb(100, 110, 125));
        clear.Click += (_, _) => ClearFields(); panel.Controls.Add(clear);
        var delete = ActionButton("Delete", 175, y + 55, Color.FromArgb(165, 55, 55));
        delete.Click += DeleteStudent; panel.Controls.Add(delete);
    }

    private void BuildRecordsPanel(Control parent)
    {
        var panel = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(12) };
        parent.Controls.Add(panel);
        panel.Controls.Add(new Label { Text = "Student Records", Font = new Font("Segoe UI", 14, FontStyle.Bold),
            AutoSize = true, Location = new Point(12, 12) });

        search.SetBounds(12, 48, 260, 32);
        search.Font = new Font("Segoe UI", 10);
        search.PlaceholderText = "Search ID, name, email or phone...";
        search.TextChanged += (_, _) => LoadRecords();
        panel.Controls.Add(search);

        departmentFilter.SetBounds(285, 48, 150, 32);
        departmentFilter.DropDownStyle = ComboBoxStyle.DropDownList;
        departmentFilter.SelectedIndexChanged += (_, _) => LoadRecords();
        panel.Controls.Add(departmentFilter);

        genderFilter.SetBounds(445, 48, 110, 32);
        genderFilter.DropDownStyle = ComboBoxStyle.DropDownList;
        genderFilter.Items.AddRange(new object[] { "All", "Male", "Female", "Other" });
        genderFilter.SelectedIndex = 0;
        genderFilter.SelectedIndexChanged += (_, _) => LoadRecords();
        panel.Controls.Add(genderFilter);

        var export = new Button { Text = "Export CSV", Location = new Point(565, 47), Size = new Size(105, 34),
            BackColor = Color.FromArgb(30, 130, 90), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
        export.Click += ExportCsv; panel.Controls.Add(export);

        var refresh = new Button { Text = "Refresh", Location = new Point(680, 47), Size = new Size(90, 34) };
        refresh.Click += (_, _) => { search.Clear(); LoadDepartments(); LoadRecords(); };
        panel.Controls.Add(refresh);

        grid.SetBounds(12, 92, 900, 520);
        grid.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        grid.ReadOnly = true; grid.AllowUserToAddRows = false; grid.MultiSelect = false;
        grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        grid.BackgroundColor = Color.White; grid.RowHeadersVisible = false;
        grid.SelectionChanged += GridSelectionChanged;
        panel.Controls.Add(grid);
    }

    private static void AddInput(Control parent, string label, Control input, ref int y)
    {
        parent.Controls.Add(new Label { Text = label, Location = new Point(15, y), AutoSize = true,
            Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.FromArgb(55, 65, 80) });
        input.SetBounds(15, y + 21, 300, 30);
        if (input is TextBox box) box.Font = new Font("Segoe UI", 10);
        if (input is ComboBox combo) combo.Font = new Font("Segoe UI", 10);
        parent.Controls.Add(input);
        y += 62;
    }

    private static Button ActionButton(string text, int x, int y, Color color) =>
        new() { Text = text, Location = new Point(x, y), Size = new Size(140, 38),
            BackColor = color, ForeColor = Color.White, FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9, FontStyle.Bold) };

    private void LoadDepartments()
    {
        string current = departmentFilter.SelectedItem?.ToString() ?? "All";
        departmentFilter.Items.Clear();
        departmentFilter.Items.Add("All");
        foreach (DataRow row in Database.DepartmentSummary().Rows)
            departmentFilter.Items.Add(row["Department"].ToString());
        departmentFilter.SelectedItem = departmentFilter.Items.Contains(current) ? current : "All";
    }

    private void LoadRecords()
    {
        if (departmentFilter.Items.Count == 0) return;
        string dept = departmentFilter.SelectedItem?.ToString() ?? "All";
        string gen = genderFilter.SelectedItem?.ToString() ?? "All";
        grid.DataSource = Database.GetStudents(search.Text, dept, gen);
        if (grid.Columns.Contains("Id")) grid.Columns["Id"].Visible = false;
    }

    private void AddStudent(object? sender, EventArgs e)
    {
        if (!ValidateForm()) return;
        try
        {
            Database.AddStudent(studentId.Text, fullName.Text, gender.Text, (int)age.Value,
                department.Text, email.Text, phone.Text, user.Username);
            MessageBox.Show("Student registered successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            ClearFields(); LoadDepartments(); LoadRecords();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Could not register student. Check that the Student ID is unique.\n" + ex.Message,
                "Registration error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void UpdateStudent(object? sender, EventArgs e)
    {
        if (selectedDatabaseId == 0)
        {
            MessageBox.Show("Select a student record from the table first.", "Select student",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        if (!ValidateForm()) return;
        try
        {
            Database.UpdateStudent(selectedDatabaseId, studentId.Text, fullName.Text, gender.Text,
                (int)age.Value, department.Text, email.Text, phone.Text, user.Username);
            MessageBox.Show("Student updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            ClearFields(); LoadDepartments(); LoadRecords();
        }
        catch (Exception ex) { MessageBox.Show("Update failed. Student ID may already be in use.\n" + ex.Message,
            "Update error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    private void DeleteStudent(object? sender, EventArgs e)
    {
        if (selectedDatabaseId == 0)
        {
            MessageBox.Show("Select a student record first.", "Select student", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        if (MessageBox.Show("Permanently delete this student record?", "Confirm deletion",
            MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
        try
        {
            Database.DeleteStudent(selectedDatabaseId, user.Username);
            MessageBox.Show("Student deleted.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            ClearFields(); LoadRecords();
        }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Delete error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    private bool ValidateForm()
    {
        if (string.IsNullOrWhiteSpace(studentId.Text) || string.IsNullOrWhiteSpace(fullName.Text) ||
            string.IsNullOrWhiteSpace(department.Text) || string.IsNullOrWhiteSpace(email.Text) ||
            string.IsNullOrWhiteSpace(phone.Text))
        {
            MessageBox.Show("Complete all student fields.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }
        if (!email.Text.Contains('@') || !email.Text.Contains('.'))
        {
            MessageBox.Show("Enter a valid email address.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            email.Focus(); return false;
        }
        if (phone.Text.Trim().Length < 7)
        {
            MessageBox.Show("Enter a valid phone number.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            phone.Focus(); return false;
        }
        return true;
    }

    private void GridSelectionChanged(object? sender, EventArgs e)
    {
        if (grid.CurrentRow is null || grid.CurrentRow.DataBoundItem is not DataRowView row) return;
        selectedDatabaseId = Convert.ToInt32(row["Id"]);
        studentId.Text = row["StudentId"].ToString();
        fullName.Text = row["FullName"].ToString();
        gender.SelectedItem = row["Gender"].ToString();
        age.Value = Math.Clamp(Convert.ToInt32(row["Age"]), 1, 120);
        department.Text = row["Department"].ToString();
        email.Text = row["Email"].ToString();
        phone.Text = row["Phone"].ToString();
    }

    private void ClearFields()
    {
        selectedDatabaseId = 0;
        studentId.Clear(); fullName.Clear(); department.Clear(); email.Clear(); phone.Clear();
        gender.SelectedIndex = 0; age.Value = 18;
        grid.ClearSelection();
    }

    private void ExportCsv(object? sender, EventArgs e)
    {
        using var dialog = new SaveFileDialog { Filter = "CSV file (*.csv)|*.csv", FileName = "StudentRecords.csv" };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        try
        {
            DataTable table = Database.GetStudents(search.Text,
                departmentFilter.SelectedItem?.ToString() ?? "All",
                genderFilter.SelectedItem?.ToString() ?? "All");
            var output = new StringBuilder();
            output.AppendLine("Student ID,Full Name,Gender,Age,Department,Email,Phone,Registered At");
            foreach (DataRow row in table.Rows)
            {
                string[] values = { "StudentId", "FullName", "Gender", "Age", "Department", "Email", "Phone", "CreatedAt" };
                output.AppendLine(string.Join(",", values.Select(column => Csv(row[column]?.ToString() ?? ""))));
            }
            File.WriteAllText(dialog.FileName, output.ToString(), new UTF8Encoding(true));
            MessageBox.Show("CSV export completed.", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Export error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    private static string Csv(string value) => "\"" + value.Replace("\"", "\"\"") + "\"";
}
