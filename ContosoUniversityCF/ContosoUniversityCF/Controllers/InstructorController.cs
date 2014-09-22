using ContosoUniversityCF.DAL;
using ContosoUniversityCF.Models;
using ContosoUniversityCF.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace ContosoUniversityCF.Controllers
{
    public class InstructorController : Controller
    {
        private readonly SchoolContext _db = new SchoolContext();

        // GET: Instructor
        public ActionResult Index(int? id, int? courseId)
        {
            var viewModel = new InstructorIndexData
            {
                Instructors = _db.Instructors
                    .Include(i => i.OfficeAssignment)
                    .Include(i => i.Courses.Select(c => c.Department))
                    .OrderBy(i => i.LastName)
            };

            if (id != null)
            {
                ViewBag.InstructorId = id.Value;
                viewModel.Courses = viewModel.Instructors.Single(i => i.Id == id.Value).Courses;
            }
            if (courseId != null)
            {
                ViewBag.CourseId = courseId.Value;
                // Lazy loading
                //viewModel.Enrollments = viewModel.Courses.Where(
                // x => x.CourseID == courseID).Single().Enrollments;
                // Explicit loading
                Course selectedCourse = viewModel.Courses.Single(x => x.CourseId == courseId);
                _db.Entry(selectedCourse).Collection(x => x.Enrollments).Load();

                foreach (Enrollment enrollment in selectedCourse.Enrollments)
                {
                    _db.Entry(enrollment).Reference(x => x.Student).Load();
                }
                viewModel.Enrollments = selectedCourse.Enrollments;
            }
            return View(viewModel);
        }

        // GET: Instructor/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Instructor instructor = _db.Instructors.Find(id);
            if (instructor == null)
            {
                return HttpNotFound();
            }
            return View(instructor);
        }

        // GET: Instructor/Create
        public ActionResult Create()
        {
            var instructor = new Instructor { Courses = new List<Course>() };
            PopulateAssignedCourseData(instructor);
            return View();
        }

        // POST: Instructor/Create
        // To protect from overpost attacks, please enable the specific properties you want to bind to, for
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "LastName,FirstMidName,HireDate,OfficeAssignment")]Instructor instructor, string[] selectedCourses)
        {
            if (selectedCourses != null)
            {
                instructor.Courses = new List<Course>();
                foreach (var courseToAdd in selectedCourses.Select(course => _db.Courses.Find(int.Parse(course))))
                {
                    instructor.Courses.Add(courseToAdd);
                }
            }
            if (ModelState.IsValid)
            {
                _db.Instructors.Add(instructor);
                _db.SaveChanges();
                return RedirectToAction("Index");
            }
            PopulateAssignedCourseData(instructor);
            return View(instructor);
        }

        // GET: Instructor/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Instructor instructor = _db.Instructors.Include(i => i.OfficeAssignment)
                .Include(i => i.Courses)
                .Single(i => i.Id == id);
            PopulateAssignedCourseData(instructor);
            if (instructor == null)
            {
                return HttpNotFound();
            }
            return View(instructor);
        }

        private void PopulateAssignedCourseData(Instructor instructor)
        {
            DbSet<Course> allCourses = _db.Courses;
            var instructorCourses = new HashSet<int>(instructor.Courses.Select(c => c.CourseId));
            var viewModel = allCourses.Select(course => new AssignedCourseData
            {
                CourseId = course.CourseId,
                Title = course.Title,
                Assigned = instructorCourses.Contains(course.CourseId)
            }).ToList();
            ViewBag.Courses = viewModel;
        }

        // POST: Instructor/Edit/5
        // To protect from overpost attacks, please enable the specific properties you want to bind to, for
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public ActionResult EditPost(int? id, string[] selectedCourses)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Instructor instructorToUpdate = _db.Instructors
                .Include(i => i.OfficeAssignment).Single(i => i.Id == id);

            if (TryUpdateModel(instructorToUpdate, "",
                new[] { "LastName", "FirstMidName", "HireDate", "OfficeAssignment" }))
            {
                try
                {
                    if (String.IsNullOrWhiteSpace(instructorToUpdate.OfficeAssignment.Location))
                    {
                        instructorToUpdate.OfficeAssignment = null;
                    }
                    UpdateInstructorCourses(selectedCourses, instructorToUpdate);
                    _db.Entry(instructorToUpdate).State = EntityState.Modified;
                    _db.SaveChanges();
                    return RedirectToAction("Index");
                }
                catch (RetryLimitExceededException /* dex */)
                {
                    //Log the error (uncomment dex variable name and add a line here to write a log.
                    ModelState.AddModelError("",
                        "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
                }
            }
            PopulateAssignedCourseData(instructorToUpdate);
            return View(instructorToUpdate);
        }

        private void UpdateInstructorCourses(IEnumerable<string> selectedCourses, Instructor instructorToUpdate)
        {
            if (selectedCourses == null)
            {
                instructorToUpdate.Courses = new List<Course>();
                return;
            }
            var selectedCoursesHs = new HashSet<string>(selectedCourses);
            var instructorCourses = new HashSet<int>(instructorToUpdate.Courses.Select(c => c.CourseId));
            foreach (var course in _db.Courses)
            {
                if (selectedCoursesHs.Contains(course.CourseId.ToString(CultureInfo.InvariantCulture)))
                {
                    if (!instructorCourses.Contains(course.CourseId))
                    {
                        instructorToUpdate.Courses.Add(course);
                    }
                }
                else
                {
                    if (instructorCourses.Contains(course.CourseId))
                    {
                        instructorToUpdate.Courses.Remove(course);
                    }
                }
            }
        }

        // POST: Instructor/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Instructor instructor = _db.Instructors.Include(i => i.OfficeAssignment).Single(i => i.Id == id);
            instructor.OfficeAssignment = null;
            _db.Instructors.Remove(instructor);
            Department department = _db.Departments.SingleOrDefault(d => d.InstructorId == id);
            if (department != null)
            {
                department.InstructorId = null;
            }
            _db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}