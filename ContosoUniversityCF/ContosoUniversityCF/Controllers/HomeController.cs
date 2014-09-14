using System.Linq;
using System.Web.Mvc;
using ContosoUniversityCF.DAL;
using ContosoUniversityCF.ViewModels;


namespace ContosoUniversityCF.Controllers
{
    public class HomeController : Controller
    {
        private SchoolContext db = new SchoolContext();

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            IQueryable<EnrollmentDateGroup> data =
                db.Students.GroupBy(student => student.EnrollmentDate).Select(dateGroup => new EnrollmentDateGroup
                {
                    EnrollmentDate = dateGroup.Key,
                    StudentCount = dateGroup.Count()
                });
            return View(data.ToList());
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}