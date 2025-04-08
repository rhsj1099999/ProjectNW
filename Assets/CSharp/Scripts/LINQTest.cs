using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class LINQTest : MonoBehaviour
{
    class Student
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public int Age { get; set; }
        public override string ToString()
        {
            return $"Id: {Id}, Name: {Name}, Age: {Age}";
        }
    }





    private void Start()
    {
        List<Student> students = new List<Student>
        {
            new Student { Id = 1, Age = 20, Name = "Alice" },
            new Student { Id = 2, Age = 22, Name = "Bob" },
            new Student { Id = 3, Age = 23, Name = "Charlie" },
            new Student { Id = 4, Age = 21, Name = "David" },
            new Student { Id = 5, Age = 20, Name = "Eve" }
        };


        var selectStudents = from student in students
                             where student.Age >= 21
                             select student;
    }
}
