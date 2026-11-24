
using System;
using System.Data;
using System.IO;
using Microsoft.Data.Sqlite;

namespace StudentManagementSystem
{
    public static class Database
    {
        private static readonly string FolderPath =
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "StudentManagementSystem"
            );

        private static readonly string DatabasePath =
            Path.Combine(FolderPath, "students.db");

        private static readonly string ConnectionString =
            $"Data Source={DatabasePath}";

        // Create database and student table
        public static void Initialize()
        {
            Directory.CreateDirectory(FolderPath);

            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            string query = @"
                CREATE TABLE IF NOT EXISTS Students (
                    StudentId TEXT PRIMARY KEY,
                    FullName TEXT NOT NULL,
                    Gender TEXT NOT NULL,
                    Age INTEGER NOT NULL,
                    Department TEXT NOT NULL,
                    Email TEXT,
                    Phone TEXT
                );";

            using var command = new SqliteCommand(query, connection);
            command.ExecuteNonQuery();
        }

        // Register a new student
        public static void AddStudent(
            string id,
            string name,
            string gender,
            int age,
            string department,
            string email,
            string phone)
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            string query = @"
                INSERT INTO Students
                (StudentId, FullName, Gender, Age, Department, Email, Phone)
                VALUES
                (@id, @name, @gender, @age, @department, @email, @phone);";

            using var command = new SqliteCommand(query, connection);

            command.Parameters.AddWithValue("@id", id);
            command.Parameters.AddWithValue("@name", name);
            command.Parameters.AddWithValue("@gender", gender);
            command.Parameters.AddWithValue("@age", age);
            command.Parameters.AddWithValue("@department", department);
            command.Parameters.AddWithValue("@email", email);
            command.Parameters.AddWithValue("@phone", phone);

            command.ExecuteNonQuery();
        }

        // View all students
        public static DataTable GetAllStudents()
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            string query = "SELECT * FROM Students ORDER BY FullName;";

            using var command = new SqliteCommand(query, connection);
            using var reader = command.ExecuteReader();

            DataTable table = new DataTable();
            table.Load(reader);

            return table;
        }

        // Search students by ID or name
        public static DataTable SearchStudents(string keyword)
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            string query = @"
                SELECT * FROM Students
                WHERE StudentId LIKE @keyword
                   OR FullName LIKE @keyword
                ORDER BY FullName;";

            using var command = new SqliteCommand(query, connection);
            command.Parameters.AddWithValue("@keyword", "%" + keyword + "%");

            using var reader = command.ExecuteReader();

            DataTable table = new DataTable();
            table.Load(reader);

            return table;
        }

        // Update student information
        public static void UpdateStudent(
            string id,
            string name,
            string gender,
            int age,
            string department,
            string email,
            string phone)
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            string query = @"
                UPDATE Students
                SET FullName = @name,
                    Gender = @gender,
                    Age = @age,
                    Department = @department,
                    Email = @email,
                    Phone = @phone
                WHERE StudentId = @id;";

            using var command = new SqliteCommand(query, connection);

            command.Parameters.AddWithValue("@id", id);
            command.Parameters.AddWithValue("@name", name);
            command.Parameters.AddWithValue("@gender", gender);
            command.Parameters.AddWithValue("@age", age);
            command.Parameters.AddWithValue("@department", department);
            command.Parameters.AddWithValue("@email", email);
            command.Parameters.AddWithValue("@phone", phone);

            command.ExecuteNonQuery();
        }

        // Delete student
        public static void DeleteStudent(string id)
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            string query =
                "DELETE FROM Students WHERE StudentId = @id;";

            using var command = new SqliteCommand(query, connection);
            command.Parameters.AddWithValue("@id", id);

            command.ExecuteNonQuery();
        }
    }
}
