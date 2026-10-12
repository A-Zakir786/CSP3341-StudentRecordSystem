using Microsoft.Data.Sqlite;
using StudentRecordSystem.Models;

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

        //checks the login details and confirms that the selected role matches
        public User? Authenticate(string username, string password, string expectedRole)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            string sql = @"SELECT Id,Username,Password,Role,FullName,StudentId
                           FROM Users
                           WHERE LOWER(Username)=LOWER(@username)
                           AND Password=@password;";

            using var command = new SqliteCommand(sql, connection);

            //Parameters keep the entered values separate from the SQL itself.
            command.Parameters.AddWithValue("@username", username.Trim());
            command.Parameters.AddWithValue("@password", password);

            using var reader = command.ExecuteReader();

            //Stop here when the account does not exist or the selected role is wrong.
            if (!reader.Read() || reader.GetString(3) != expectedRole)
                return null;

            //Return the correct user object after the account has been verified.
            if (expectedRole == "Admin")
                return new AdminUser
                {
                    Id = reader.GetInt32(0),
                    Username = reader.GetString(1),
                    Password = reader.GetString(2),
                    FullName = reader.GetString(4)
                };

            return new StudentUser
            {
                Id = reader.GetInt32(0),
                Username = reader.GetString(1),
                Password = reader.GetString(2),
                FullName = reader.GetString(4),
                StudentId = reader.GetString(5)
            };
        }

        //returns all student profiles for the Admin grid.
        public List<Student> GetStudents()
        {
            var students = new List<Student>();

            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            using var command = new SqliteCommand(
                "SELECT StudentId,FullName,Email,CourseTitle,YearLevel " +
                "FROM Students ORDER BY StudentId;",
                connection);

            using var reader = command.ExecuteReader();

            //convert each database row into a Student object for the interface.
            while (reader.Read())
                students.Add(new Student
                {
                    StudentId = reader.GetString(0),
                    FullName = reader.GetString(1),
                    Email = reader.GetString(2),
                    CourseTitle = reader.GetString(3),
                    YearLevel = reader.GetInt32(4)
                });

            return students;
        }

        //Adds a new student or updates the existing profile with the same Student ID.
        public bool SaveStudent(Student student)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            //The main demo student uses "student"; other students use their ID.
            string username =
                student.StudentId == "S001" ? "student" : student.StudentId;

            //The upsert lets the same method handle both adding and editing.
            string sql = @"
                INSERT INTO Students VALUES (@id,@name,@email,@course,@year)
                ON CONFLICT(StudentId) DO UPDATE SET
                    FullName=@name,
                    Email=@email,
                    CourseTitle=@course,
                    YearLevel=@year;

                INSERT OR IGNORE INTO Users
                (Username,Password,Role,FullName,StudentId)
                VALUES (@username,'student123','Student',@name,@id);

                UPDATE Users SET FullName=@name
                WHERE StudentId=@id AND Role='Student';";

            using var command = new SqliteCommand(sql, connection);

            //Supply the student values separately from the SQL command text.
            command.Parameters.AddWithValue("@id", student.StudentId);
            command.Parameters.AddWithValue("@username", username);
            command.Parameters.AddWithValue("@name", student.FullName);
            command.Parameters.AddWithValue("@email", student.Email);
            command.Parameters.AddWithValue("@course", student.CourseTitle);
            command.Parameters.AddWithValue("@year", student.YearLevel);

            return command.ExecuteNonQuery() > 0;
        }

        //deletes the selected student together with their login and saved results.
        public bool DeleteStudent(string studentId)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            //remove the linked records before removing the student profile.
            string sql = @"
                DELETE FROM AcademicRecords WHERE StudentId=@id;
                DELETE FROM Users WHERE StudentId=@id;
                DELETE FROM Students WHERE StudentId=@id;";

            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@id", studentId);

            return command.ExecuteNonQuery() > 0;
        }

        //Returns the units available when entering a grade.
        public List<Unit> GetUnits()
        {
            var units = new List<Unit>();
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            using var command = new SqliteCommand(
                "SELECT UnitCode,UnitTitle,CreditPoints " +
                "FROM Units ORDER BY UnitCode;",
                connection);
            using var reader = command.ExecuteReader();
            while (reader.Read())
                units.Add(new Unit
                {
                    UnitCode = reader.GetString(0),
                    UnitTitle = reader.GetString(1),
                    CreditPoints = reader.GetInt32(2)
                });
            return units;
        }

        //Returns all academic records belonging to one student.
        public List<AcademicRecord> GetRecords(string studentId)
        {
            var records = new List<AcademicRecord>();
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            string sql =
                @"SELECT RecordId,StudentId,UnitCode,UnitTitle,CreditPoints,Mark,Grade
                  FROM AcademicRecords
                  WHERE StudentId=@id
                  ORDER BY UnitCode;";
            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@id", studentId);
            using var reader = command.ExecuteReader();
            while (reader.Read())
                records.Add(new AcademicRecord
                {
                    RecordId = reader.GetInt32(0),
                    StudentId = reader.GetString(1),
                    UnitCode = reader.GetString(2),
                    UnitTitle = reader.GetString(3),
                    CreditPoints = reader.GetInt32(4),
                    Mark = reader.GetDouble(5),
                    Grade = reader.GetString(6)
                });
            return records;
        }

        //Adds a new grade or updates the same student and unit combination.
        public bool SaveRecord(AcademicRecord record)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            //Work out the grade from the mark before saving the result.
            record.Grade = WamCalculator.GetGrade(record.Mark);
            string sql = @"
                INSERT INTO AcademicRecords
                (StudentId,UnitCode,UnitTitle,CreditPoints,Mark,Grade)
                VALUES (@studentId,@unitCode,@unitTitle,@credits,@mark,@grade)
                ON CONFLICT(StudentId,UnitCode) DO UPDATE SET
                    UnitTitle=@unitTitle,
                    CreditPoints=@credits,
                    Mark=@mark,
                    Grade=@grade;";

            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@studentId", record.StudentId);
            command.Parameters.AddWithValue("@unitCode", record.UnitCode);
            command.Parameters.AddWithValue("@unitTitle", record.UnitTitle);
            command.Parameters.AddWithValue("@credits", record.CreditPoints);
            command.Parameters.AddWithValue("@mark", record.Mark);
            command.Parameters.AddWithValue("@grade", record.Grade);
            return command.ExecuteNonQuery() > 0;
        }

        //Deletes one academic result selected from the Admin records grid.
        public bool DeleteRecord(int recordId)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            using var command = new SqliteCommand(
                "DELETE FROM AcademicRecords WHERE RecordId=@id;",
                connection);
            command.Parameters.AddWithValue("@id", recordId);
            return command.ExecuteNonQuery() > 0;
        }
    }
}