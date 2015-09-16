using FreeLance.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;

namespace FreeLance.Controllers
{
	[Authorize(Roles = "Admin, Freelancer")]
    public class FreelancerController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public class HomeView
        {
            public List<ContractModels> Contracts { get; set; }
            public List<ProblemModels> Problems { get; set; }
        }

        public ActionResult Index()
        {
            return RedirectToAction("Home");
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

		public class ProblemView
		{
			public ProblemModels ProblemModels { get; set; }
			public bool IsSubscibed { get; set; }
        }

        public ActionResult Problem(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            ProblemModels problemModels = db.ProblemModels.Find(id);
            if (problemModels == null)
            {
                return HttpNotFound();
			}
			string userId = User.Identity.GetUserId();
			SubscriptionModels[] subscriptions = db.SubscriptionModels.Where(sub => sub.Freelancer.Id == userId
													&& sub.Problem.ProblemId == id).Distinct().ToArray();
			SubscriptionModels subscription = subscriptions.Length > 0 ? subscriptions[0] : null;
			ProblemView view = new ProblemView
			{
				ProblemModels = problemModels,
				IsSubscibed = subscription != null
			};
			return View(view);
        }

        public ViewResult OpenProblems()
        {
            ProblemModels[] openProblems = db.ProblemModels.Where(x => x.Status == 0).ToArray();            
            return View(openProblems);
        }
    }
}