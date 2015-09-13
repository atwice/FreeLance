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

        public class HomeView
        {
            public List<ContractModels> Contracts { get; set; }
            public List<ProblemModels> Problems { get; set; }
        }

        // GET: Freelancer
        public ActionResult Home()
        {
            var viewModel = new HomeView
            {
                Contracts = db.ContractModels.ToList(),
                Problems = db.ProblemModels.ToList()
            };
            return View(viewModel);
        }

        public ActionResult Archive()
        {
            return View(db.ContractModels.ToList());
        }

        public ActionResult Contract(int id)
        {
            return View(db.ContractModels.Find(id));
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

        public ActionResult Open()
        {

            ProblemModels[] openProblems = db.ProblemModels.Where(x => x.Status == 0).ToArray();
            //return View(openProblems);
            return View(openProblems.ToList());
        }

        public ViewResult OpenProblems()
        {
            ProblemModels[] openProblems = db.ProblemModels.Where(x => x.Status == 0).ToArray();            
            return View(openProblems);
        }
    }
}