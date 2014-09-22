using System.ComponentModel.DataAnnotations;

namespace ContosoUniversityCF.Models
{
    public enum Grade
    {
        A, B, C, D, F
    }
    public class Enrollment
    {
        public int EnrollmentId { get; set; }
        
        [DisplayFormat(NullDisplayText = "No grade")]
        public Grade? Grade { get; set; }

        public int CourseId { get; set; }
        public virtual Course Course { get; set; }

        public int StudentId { get; set; }
        public virtual Student Student { get; set; }
    }
}