using System.Data;
using System.Security.Cryptography;
using Microsoft.Data.Sqlite;

namespace StudentManagementSystem;

internal sealed record AppUser(int Id, string Username, string Role);

internal static class Database
{
    private static readonly string DataFolder = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "AdvancedStudentManagementSystem");

    public static string DatabasePath => Path.Combine(DataFolder, "students.db");
    private static string ConnectionString => $"Data Source={DatabasePath};Foreign Keys=True";

    private static SqliteConnection OpenConnection()
    {
        var connection = new SqliteConnection(ConnectionString);
        connection.Open();
        return connection;
    }

    public static void Initialize()
    {
        Directory.CreateDirectory(DataFolder);

        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = """
            CREATE TABLE IF NOT EXISTS Users (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Username TEXT NOT NULL UNIQUE COLLATE NOCASE,
                PasswordHash TEXT NOT NULL,
                PasswordSalt TEXT NOT NULL,
                Role TEXT NOT NULL CHECK(Role IN ('Admin','Staff')),
                CreatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP
            );

            CREATE TABLE IF NOT EXISTS Students (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                StudentId TEXT NOT NULL UNIQUE COLLATE NOCASE,
                FullName TEXT NOT NULL,
                Gender TEXT NOT NULL,
                Age INTEGER NOT NULL,
                Department TEXT NOT NULL,
                Email TEXT NOT NULL,
                Phone TEXT NOT NULL,
                CreatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
                UpdatedAt TEXT
            );

            CREATE TABLE IF NOT EXISTS AuditLogs (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Username TEXT NOT NULL,
                Action TEXT NOT NULL,
                Details TEXT NOT NULL,
                CreatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP
            );
            """;
        command.ExecuteNonQuery();

        using var check = connection.CreateCommand();
        check.CommandText = "SELECT COUNT(*) FROM Users;";
        if (Convert.ToInt32(check.ExecuteScalar()) == 0)
            CreateUser("admin", "Admin@123", "Admin");
    }

    private static (string Hash, string Salt) HashPassword(string password)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(16);
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
            password, salt, 150_000, HashAlgorithmName.SHA256, 32);
        return (Convert.ToBase64String(hash), Convert.ToBase64String(salt));
    }

    private static bool VerifyPassword(string password, string hashText, string saltText)
    {
        byte[] expected = Convert.FromBase64String(hashText);
        byte[] salt = Convert.FromBase64String(saltText);
        byte[] actual = Rfc2898DeriveBytes.Pbkdf2(
            password, salt, 150_000, HashAlgorithmName.SHA256, 32);
        return CryptographicOperations.FixedTimeEquals(expected, actual);
    }

    public static bool ValidateUser(string username, string password, out AppUser? user)
    {
        user = null;
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT Id, Username, PasswordHash, PasswordSalt, Role
            FROM Users WHERE Username = $username COLLATE NOCASE;
            """;
        command.Parameters.AddWithValue("$username", username.Trim());

        using var reader = command.ExecuteReader();
        if (!reader.Read()) return false;

        string hash = reader.GetString(2);
        string salt = reader.GetString(3);
        if (!VerifyPassword(password, hash, salt)) return false;

        user = new AppUser(reader.GetInt32(0), reader.GetString(1), reader.GetString(4));
        Log(user.Username, "Login", "Successful login");
        return true;
    }

    public static void CreateUser(string username, string password, string role)
    {
        var (hash, salt) = HashPassword(password);
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO Users (Username, PasswordHash, PasswordSalt, Role)
            VALUES ($username, $hash, $salt, $role);
            """;
        command.Parameters.AddWithValue("$username", username.Trim());
        command.Parameters.AddWithValue("$hash", hash);
        command.Parameters.AddWithValue("$salt", salt);
        command.Parameters.AddWithValue("$role", role);
        command.ExecuteNonQuery();
    }

    public static DataTable GetUsers()
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, Username, Role, CreatedAt FROM Users ORDER BY Username;";
        using var reader = command.ExecuteReader();
        var table = new DataTable();
        table.Load(reader);
        return table;
    }

    public static void DeleteUser(int id)
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Users WHERE Id = $id;";
        command.Parameters.AddWithValue("$id", id);
        command.ExecuteNonQuery();
    }

    public static void ChangePassword(int userId, string currentPassword, string newPassword)
    {
        using var connection = OpenConnection();
        using var select = connection.CreateCommand();
        select.CommandText = "SELECT PasswordHash, PasswordSalt FROM Users WHERE Id = $id;";
        select.Parameters.AddWithValue("$id", userId);

        using var reader = select.ExecuteReader();
        if (!reader.Read()) throw new InvalidOperationException("User account was not found.");
        string oldHash = reader.GetString(0);
        string oldSalt = reader.GetString(1);
        reader.Close();

        if (!VerifyPassword(currentPassword, oldHash, oldSalt))
            throw new InvalidOperationException("Your current password is incorrect.");

        var (hash, salt) = HashPassword(newPassword);
        using var update = connection.CreateCommand();
        update.CommandText = "UPDATE Users SET PasswordHash=$hash, PasswordSalt=$salt WHERE Id=$id;";
        update.Parameters.AddWithValue("$hash", hash);
        update.Parameters.AddWithValue("$salt", salt);
        update.Parameters.AddWithValue("$id", userId);
        update.ExecuteNonQuery();
        Log("User " + userId, "Password changed", "Password updated");
    }

    public static void AddStudent(string studentId, string fullName, string gender,
        int age, string department, string email, string phone, string username)
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO Students (StudentId, FullName, Gender, Age, Department, Email, Phone)
            VALUES ($studentId, $name, $gender, $age, $department, $email, $phone);
            """;
        AddStudentParameters(command, studentId, fullName, gender, age, department, email, phone);
        command.ExecuteNonQuery();
        Log(username, "Student registered", $"Student ID: {studentId}");
    }

    public static void UpdateStudent(int id, string studentId, string fullName, string gender,
        int age, string department, string email, string phone, string username)
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = """
            UPDATE Students SET StudentId=$studentId, FullName=$name, Gender=$gender,
                Age=$age, Department=$department, Email=$email, Phone=$phone,
                UpdatedAt=CURRENT_TIMESTAMP WHERE Id=$id;
            """;
        AddStudentParameters(command, studentId, fullName, gender, age, department, email, phone);
        command.Parameters.AddWithValue("$id", id);
        if (command.ExecuteNonQuery() == 0)
            throw new InvalidOperationException("The selected student no longer exists.");
        Log(username, "Student updated", $"Student ID: {studentId}");
    }

    private static void AddStudentParameters(SqliteCommand command, string studentId,
        string fullName, string gender, int age, string department, string email, string phone)
    {
        command.Parameters.AddWithValue("$studentId", studentId.Trim());
        command.Parameters.AddWithValue("$name", fullName.Trim());
        command.Parameters.AddWithValue("$gender", gender);
        command.Parameters.AddWithValue("$age", age);
        command.Parameters.AddWithValue("$department", department.Trim());
        command.Parameters.AddWithValue("$email", email.Trim());
        command.Parameters.AddWithValue("$phone", phone.Trim());
    }

    public static void DeleteStudent(int id, string username)
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Students WHERE Id=$id;";
        command.Parameters.AddWithValue("$id", id);
        if (command.ExecuteNonQuery() == 0)
            throw new InvalidOperationException("The selected student no longer exists.");
        Log(username, "Student deleted", $"Database record: {id}");
    }

    public static DataTable GetStudents(string search = "", string department = "All", string gender = "All")
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT Id, StudentId, FullName, Gender, Age, Department, Email, Phone, CreatedAt
            FROM Students
            WHERE ($search = '' OR StudentId LIKE $pattern OR FullName LIKE $pattern
                   OR Email LIKE $pattern OR Phone LIKE $pattern)
              AND ($department = 'All' OR Department = $department)
              AND ($gender = 'All' OR Gender = $gender)
            ORDER BY FullName COLLATE NOCASE;
            """;
        command.Parameters.AddWithValue("$search", search.Trim());
        command.Parameters.AddWithValue("$pattern", "%" + search.Trim() + "%");
        command.Parameters.AddWithValue("$department", department);
        command.Parameters.AddWithValue("$gender", gender);
        using var reader = command.ExecuteReader();
        var table = new DataTable();
        table.Load(reader);
        return table;
    }

    public static int StudentCount()
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM Students;";
        return Convert.ToInt32(command.ExecuteScalar());
    }

    public static int UserCount()
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM Users;";
        return Convert.ToInt32(command.ExecuteScalar());
    }

    public static DataTable DepartmentSummary()
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT Department, COUNT(*) AS Total FROM Students GROUP BY Department ORDER BY Total DESC;";
        using var reader = command.ExecuteReader();
        var table = new DataTable();
        table.Load(reader);
        return table;
    }

    public static void Log(string username, string action, string details)
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "INSERT INTO AuditLogs (Username, Action, Details) VALUES ($user,$action,$details);";
        command.Parameters.AddWithValue("$user", username);
        command.Parameters.AddWithValue("$action", action);
        command.Parameters.AddWithValue("$details", details);
        command.ExecuteNonQuery();
    }
}
