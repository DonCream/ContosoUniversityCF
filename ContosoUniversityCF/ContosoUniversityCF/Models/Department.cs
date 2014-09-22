using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContosoUniversityCF.Models
{
    public class Department
    {
        public int DepartmentId { get; set; }
        
        [StringLength(50, MinimumLength = 3)]
        public string Name { get; set; }
        
        [DataType(DataType.Currency)]
        [Column(TypeName = "Money")]
        public decimal Budget { get; set; }
        
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; }
        
        public int? InstructorId { get; set; }
        public virtual Instructor Administrator { get; set; }
        
        public virtual ICollection<Course> Courses { get; set; }
    }
}