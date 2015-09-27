using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using FreeLance.Models;
using Microsoft.AspNet.Identity;


using System.Data.Entity.Validation;
using System.Diagnostics;

namespace FreeLance.Controllers
{
	[Authorize]
	public class ProblemController : Controller
	{
		private ApplicationDbContext db = new ApplicationDbContext();

		public class DetailsView
		{
			public ProblemModels ProblemModels { get; set; }
			public bool IsSubscibed { get; set; }
			public List<SubscriptionModels> Subscriptions { get; set; }
		}
		
		public ActionResult Details(int? id)
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
			DetailsView view = new DetailsView
			{
				ProblemModels = problemModels,
				IsSubscibed = subscription != null,
				Subscriptions = db.SubscriptionModels.Where(x => x.Problem.ProblemId == id).ToList()
			};
			return View(view);
		}

		// GET: Problem
		public ActionResult Index()
		{
			return View(db.ProblemModels.ToList());
		}

		[Authorize(Roles = "Employer")]
		public ActionResult Create()
		{
			ApplicationUser employer = db.Users.Find(User.Identity.GetUserId());
			if (!employer.IsApprovedByCoordinator)
                return RedirectToAction("Home", "Employer");
			return View();
		}

		[HttpPost]
		[Authorize(Roles = "Employer")]
		[ValidateAntiForgeryToken]
        public ActionResult Create(ProblemModels problem)
		{
			ApplicationUser employer = db.Users.Find(User.Identity.GetUserId());
			if (!employer.IsApprovedByCoordinator) {
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}
			problem.Employer = db.Users.Find(User.Identity.GetUserId());
			problem.Status = ProblemStatus.Opened;
			db.ProblemModels.Add(problem);
			db.SaveChanges();
			return RedirectToAction("Details", new { id = problem.ProblemId });
			//return View(problem);
		}

        [HttpPost]
        [Authorize(Roles = "Employer")]
        public ActionResult ChangeStatus(int id, ProblemStatus status, string redirect)
        {
            ProblemModels problem = db.ProblemModels.Include(c => c.Employer).Single(c => c.ProblemId == id);
            if (problem == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            problem.Status = status;
            db.SaveChanges();
            return Redirect(redirect == null ? "/Problem/Details/" + id.ToString() : redirect);
        }

        // GET: Problem/Edit/5
        public ActionResult Edit(int? id)
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
			return View(problemModels);
		}

		// POST: Problem/Edit/5
		// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
		// more details see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Edit([Bind(Include = "ProblemId,Name,Description,Status")] ProblemModels problemModels)
		{
			if (ModelState.IsValid)
			{
				db.Entry(problemModels).State = EntityState.Modified;
				db.SaveChanges();
				return RedirectToAction("Index");
			}
			return View(problemModels);
		}

		// GET: Problem/Delete/5
		public ActionResult Delete(int? id)
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
			return View(problemModels);
		}

		// POST: Problem/Delete/5
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public ActionResult DeleteConfirmed(int id)
		{
			ProblemModels problemModels = db.ProblemModels.Find(id);
			db.ProblemModels.Remove(problemModels);
			db.SaveChanges();
			return RedirectToAction("Index");
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				db.Dispose();
			}
			base.Dispose(disposing);
		}
	}
}
