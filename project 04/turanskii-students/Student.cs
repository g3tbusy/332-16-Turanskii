using System;

namespace turanskii_students
{
    public class Student
    {
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public int Course { get; set; }
        public string Group { get; set; }
        public DateTime BirthDate { get; set; }
        public string Email { get; set; }

        public Student(string lastName, string firstName, string middleName, int course, 
                      string group, DateTime birthDate, string email)
        {
            LastName = lastName;
            FirstName = firstName;
            MiddleName = middleName;
            Course = course;
            Group = group;
            BirthDate = birthDate;
            Email = email;
        }
    }
} 