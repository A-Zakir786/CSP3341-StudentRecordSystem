using Microsoft.Data.Sqlite;

namespace StudentRecordSystem.Services
{
    //Handles all SQLite storage used by the Student Record System.
    public class DatabaseService
    {
        private readonly string _connectionString;

        public DatabaseService()
        {
            //Store the database beside the running application.
            string path = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "student_records.db");

            _connectionString = $"Data Source={path}";
        }

        //creates the tables and adds demo data when the database is first created.
        public void Initialize()
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            //Keep the small database structure together so it is easy to see.
            string sql = @"
                CREATE TABLE IF NOT EXISTS Users (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Username TEXT UNIQUE,
                    Password TEXT,
                    Role TEXT,
                    FullName TEXT,
                    StudentId TEXT);

                CREATE TABLE IF NOT EXISTS Students (
                    StudentId TEXT PRIMARY KEY,
                    FullName TEXT,
                    Email TEXT,
                    CourseTitle TEXT,
                    YearLevel INTEGER);

                CREATE TABLE IF NOT EXISTS Units (
                    UnitCode TEXT PRIMARY KEY,
                    UnitTitle TEXT,
                    CreditPoints INTEGER);

                CREATE TABLE IF NOT EXISTS AcademicRecords (
                    RecordId INTEGER PRIMARY KEY AUTOINCREMENT,
                    StudentId TEXT,
                    UnitCode TEXT,
                    UnitTitle TEXT,
                    CreditPoints INTEGER,
                    Mark REAL,
                    Grade TEXT,
                    UNIQUE(StudentId, UnitCode));";

            new SqliteCommand(sql, connection).ExecuteNonQuery();

            //only add the starting records when the database is empty.
            SeedData(connection);
        }

        //adds the demo accounts, students, units and sample grades.
        private static void SeedData(SqliteConnection connection)
        {
            using var check = new SqliteCommand(
                "SELECT COUNT(*) FROM Users",
                connection);

            //Existing users mean that the database has already been set up.
            if ((long)(check.ExecuteScalar() ?? 0) > 0) return;

            string sql = @"
                INSERT INTO Users (Username,Password,Role,FullName,StudentId) VALUES
                ('admin','admin123','Admin','Dr Leisa Armstrong',NULL),
                ('student','student123','Student','Aqeeb Zakir','S001'),
                ('S002','student123','Student','Bob Jones','S002');

                INSERT INTO Students VALUES
                ('S001','Aqeeb Zakir','aqeeb.zakir@ecu.edu.au',
                 'Bachelor of Computer Science',3),
                ('S002','Bob Jones','bob.jones@ecu.edu.au',
                 'Bachelor of Science (Cyber Security)',2);

                INSERT INTO Units VALUES
                ('CSP3341','Programming Languages and Paradigms',15),
                ('CSI1241','Systems Analysis',15),
                ('CSP2104','Object-Oriented Programming',15),
                ('CSG1105','Applied Communications',15);

                INSERT INTO AcademicRecords
                (StudentId,UnitCode,UnitTitle,CreditPoints,Mark,Grade) VALUES
                ('S001','CSP3341','Programming Languages and Paradigms',15,88,'HD'),
                ('S001','CSI1241','Systems Analysis',15,78.5,'D'),
                ('S001','CSP2104','Object-Oriented Programming',15,92,'HD'),
                ('S002','CSI1241','Systems Analysis',15,66,'CR');";

            new SqliteCommand(sql, connection).ExecuteNonQuery();
        }
    }
}