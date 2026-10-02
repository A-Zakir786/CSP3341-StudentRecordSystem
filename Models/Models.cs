//This is the models file which defines all the core data models and OOP 
//class hierarchies used across the Student Record and WAM calculation system.

namespace StudentRecordSystem.Models
{
    //Base abstract user account class demonstrating OOP Abstraction
    public abstract class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty; // "Admin" or "Student"
        public string FullName { get; set; } = string.Empty;
        public abstract string GetRoleInfo();
    }
    //Administrator account model inheriting from User
    public class AdminUser : User
    {
        public AdminUser() => Role = "Admin";
        public override string GetRoleInfo() => "System Administrator";
    }
    //Student user account model inheriting from User
    public class StudentUser : User
    {
        public string StudentId { get; set; } = string.Empty;
        public StudentUser() => Role = "Student";
        public override string GetRoleInfo() => $"Student (ID: {StudentId})";
    }
    //student profile record entity
    public class Student
    {
        public string StudentId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string CourseTitle { get; set; } = string.Empty;
        public int YearLevel { get; set; } = 1;
    }
    //Academic course unit entity
    public class Unit
    {
        public string UnitCode { get; set; } = string.Empty;
        public string UnitTitle { get; set; } = string.Empty;
        public int CreditPoints { get; set; } = 15;
    }
    //Student grade record for a unit
    public class AcademicRecord
    {
        public int RecordId { get; set; }
        public string StudentId { get; set; } = string.Empty;
        public string UnitCode { get; set; } = string.Empty;
        public string UnitTitle { get; set; } = string.Empty;
        public int CreditPoints { get; set; } = 15;
        public double Mark { get; set; }
        public string Grade { get; set; } = "N";
    }
    //this Holds calculated academic metrics
    public class WamResult
    {
        public double Wam { get; set; }
        public double Gpa { get; set; }
        public int TotalCredits { get; set; }
        public string Standing { get; set; } = "Good Standing";
    }
}