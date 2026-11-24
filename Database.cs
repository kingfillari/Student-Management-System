
using System;
using System.Data;
using System.IO;
using System.Security.Cryptography;
using Microsoft.Data.Sqlite;

namespace StudentManagementSystem
{
    public static class Database
    {
        private static readonly string FolderPath =
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "StudentManagementSystem");

        private static readonly string DatabasePath =
            Path.Combine(FolderPath, "students.db");

        private static readonly string ConnectionString =
            $"Data Source={DatabasePath}";

        private const int Iterations = 100000;

        public static void Initialize()
        {
            Directory.CreateDirectory(FolderPath);

            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            string studentsSql = @"
                CREATE TABLE IF NOT EXISTS Students (
                    StudentId TEXT PRIMARY KEY,
                    FullName TEXT NOT NULL,
                    Gender TEXT NOT NULL,
                    Age INTEGER NOT NULL,
                    Department TEXT NOT NULL,
                    Email TEXT,
                    Phone TEXT
                );";

            string usersSql = @"
                CREATE TABLE IF NOT EXISTS Users (
                    Username TEXT PRIMARY KEY,
                    PasswordHash TEXT NOT NULL,
                    PasswordSalt TEXT NOT NULL
                );";

            using var command = connection.CreateCommand();
            command.CommandText = studentsSql + usersSql;
            command.ExecuteNonQuery();

            SeedAdmin(connection);
        }

        private static void SeedAdmin(SqliteConnection connection)
        {
            using var check = connection.CreateCommand();
            check.CommandText =
                "SELECT COUNT(*) FROM Users WHERE Username = @username;";
            check.Parameters.AddWithValue("@username", "admin");

            if (Convert.ToInt32(check.ExecuteScalar()) > 0)
                return;

            byte[] salt = RandomNumberGenerator.GetBytes(16);
            byte[] hash = HashPassword("Admin@123", salt);

            using var insert = connection.CreateCommand();
            insert.CommandText = @"
                INSERT INTO Users (Username, PasswordHash, PasswordSalt)
                VALUES (@username, @hash, @salt);";

            insert.Parameters.AddWithValue("@username", "admin");
            insert.Parameters.AddWithValue("@hash", Convert.ToBase64String(hash));
            insert.Parameters.AddWithValue("@salt", Convert.ToBase64String(salt));
            insert.ExecuteNonQuery();
        }

        private static byte[] HashPassword(string password, byte[] salt)
        {
            return Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                Iterations,
                HashAlgorithmName.SHA256,
                32);
        }

        public static bool ValidateUser(string username, string password)
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT PasswordHash, PasswordSalt
                FROM Users
                WHERE Username = @username;";

            command.Parameters.AddWithValue("@username", username);

            using var reader = command.ExecuteReader();

            if (!reader.Read())
                return false;

            byte[] storedHash =
                Convert.FromBase64String(reader.GetString(0));

            byte[] salt =
                Convert.FromBase64String(reader.GetString(1));

            byte[] enteredHash = HashPassword(password, salt);

            return CryptographicOperations.FixedTimeEquals(
                storedHash, enteredHash);
        }

        public static void AddStudent(
            string id, string name, string gender, int age,
            string department, string email, string phone)
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            using var command = connection.CreateCommand();

            command.CommandText = @"
                INSERT INTO Students
                (StudentId, FullName, Gender, Age, Department, Email, Phone)
                VALUES
                (@id, @name, @gender, @age, @department, @email, @phone);";

            command.Parameters.AddWithValue("@id", id);
            command.Parameters.AddWithValue("@name", name);
            command.Parameters.AddWithValue("@gender", gender);
            command.Parameters.AddWithValue("@age", age);
            command.Parameters.AddWithValue("@department", department);
            command.Parameters.AddWithValue("@email", email);
            command.Parameters.AddWithValue("@phone", phone);

            command.ExecuteNonQuery();
        }

        public static DataTable GetAllStudents()
        {
            return ExecuteStudentQuery(
                "SELECT * FROM Students ORDER BY FullName;",
                null);
        }

        public static DataTable SearchStudents(string keyword)
        {
            return ExecuteStudentQuery(
                @"SELECT * FROM Students
                  WHERE StudentId LIKE @keyword
                  OR FullName LIKE @keyword
                  ORDER BY FullName;",
                keyword);
        }

        private static DataTable ExecuteStudentQuery(
            string sql, string? keyword)
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = sql;

            if (keyword != null)
                command.Parameters.AddWithValue("@keyword", "%" + keyword + "%");

            using var reader = command.ExecuteReader();

            DataTable table = new DataTable();
            table.Load(reader);

            return table;
        }

        public static void UpdateStudent(
            string id, string name, string gender, int age,
            string department, string email, string phone)
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            using var command = connection.CreateCommand();

            command.CommandText = @"
                UPDATE Students
                SET FullName = @name,
                    Gender = @gender,
                    Age = @age,
                    Department = @department,
                    Email = @email,
                    Phone = @phone
                WHERE StudentId = @id;";

            command.Parameters.AddWithValue("@id", id);
            command.Parameters.AddWithValue("@name", name);
            command.Parameters.AddWithValue("@gender", gender);
            command.Parameters.AddWithValue("@age", age);
            command.Parameters.AddWithValue("@department", department);
            command.Parameters.AddWithValue("@email", email);
            command.Parameters.AddWithValue("@phone", phone);

            command.ExecuteNonQuery();
        }

        public static void DeleteStudent(string id)
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText =
                "DELETE FROM Students WHERE StudentId = @id;";

            command.Parameters.AddWithValue("@id", id);
            command.ExecuteNonQuery();
        }

        public static int GetStudentCount()
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM Students;";

            return Convert.ToInt32(command.ExecuteScalar());
        }
    }
}
