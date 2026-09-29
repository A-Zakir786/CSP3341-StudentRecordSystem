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
}