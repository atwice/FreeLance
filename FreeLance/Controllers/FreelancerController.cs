using FreeLance.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace FreeLance.Controllers
{
    public class FreelancerController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Freelancer
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Problem(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            // check if status == open?
            ProblemModels problemModels = db.ProblemModels.Find(id);
            if (problemModels == null)
            {
                return HttpNotFound();
            }
            return View(problemModels);
        }

        public ViewResult OpenProblems()
        {
            ProblemModels[] openProblems = db.ProblemModels.Where(x => x.Status == 0).ToArray();            
            return View(openProblems);
        }
    }
}