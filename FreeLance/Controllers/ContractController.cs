using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using FreeLance.Models;
using Microsoft.AspNet.Identity;

namespace FreeLance.Controllers
{
	public class ContractController : Controller
	{
		private ApplicationDbContext db = new ApplicationDbContext();

		// GET: Contract
		public ActionResult Index()
		{
			return View(db.ContractModels.ToList());
		}

		// GET: Contract/Details/5
		public ActionResult Details(int? id)
		{
			if (id == null)
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}
			ContractModels contractModels = db.ContractModels.Find(id);
			if (contractModels == null)
			{
				return HttpNotFound();
			}
			return View(contractModels);
		}

		// GET: Contract/Create
		[Authorize(Roles = "Employer")]
		public ActionResult Create(int problemId, string freelancerId)
		{
			ProblemModels problem = db.ProblemModels.Find(problemId);
			ApplicationUser freelancer = db.Users.Find(freelancerId);
			if (problem == null || freelancer == null || problem.Employer.Id != User.Identity.GetUserId())
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}
			return View(new ContractModels { Problem = problem, Freelancer = freelancer });
		}

		// POST: Contract/Create
		// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
		// more details see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		[Authorize(Roles = "Employer")]
		public ActionResult Create([Bind(Include = "Details")] ContractModels contract, 
			int problemId, string freelancerId, string redirect)
		{
			string userId = User.Identity.GetUserId();
			ProblemModels problem = db.ProblemModels.Find(problemId);
			ApplicationUser freelancer = db.Users.Find(freelancerId);
			SubscriptionModels[] subscriptions = db.SubscriptionModels.Where(sub => sub.Freelancer.Id == freelancerId
													&& sub.Problem.ProblemId == problemId).Distinct().ToArray();
			SubscriptionModels subscription = subscriptions.Length > 0 ? subscriptions[0] : null;
			if (subscription == null || problem == null || freelancer == null || problem.Employer.Id != userId) // || problem.Status == Opened
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}
			contract.Problem = problem;
			contract.Status = ContractStatus.Opened;
			contract.Freelancer = freelancer;

			db.ContractModels.Add(contract);
			db.SubscriptionModels.Remove(subscription);
			db.SaveChanges();
			return Redirect(redirect == null ? "/Contract/Details/" + contract.ContractId.ToString() : redirect);
		}

		[HttpPost]
		[Authorize(Roles = "Employer, Freelancer")]
		public ActionResult ChangeStatus(int id, ContractStatus status, string redirect)
		{
			string userId = User.Identity.GetUserId();
			ContractModels contract = db.ContractModels.Include( c => c.Problem ).Single( c => c.ContractId == id );
			if (contract == null || contract.Freelancer == null)
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}
			contract.Status = status;
			db.SaveChanges();
			return Redirect(redirect == null ? "/Contract/Details/" + id.ToString() : redirect);
		}


		// GET: Contract/Edit/5
		public ActionResult Edit(int? id)
		{
			if (id == null)
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}
			ContractModels contractModels = db.ContractModels.Find(id);
			if (contractModels == null)
			{
				return HttpNotFound();
			}
			return View(contractModels);
		}

		// POST: Contract/Edit/5
		// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
		// more details see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Edit([Bind(Include = "ContractId,Details,Status")] ContractModels contractModels)
		{
			if (ModelState.IsValid)
			{
				db.Entry(contractModels).State = EntityState.Modified;
				db.SaveChanges();
				return RedirectToAction("Index");
			}
			return View(contractModels);
		}

		// GET: Contract/Delete/5
		public ActionResult Delete(int? id)
		{
			if (id == null)
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}
			ContractModels contractModels = db.ContractModels.Find(id);
			if (contractModels == null)
			{
				return HttpNotFound();
			}
			return View(contractModels);
		}

		// POST: Contract/Delete/5
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public ActionResult DeleteConfirmed(int id)
		{
			ContractModels contractModels = db.ContractModels.Find(id);
			db.ContractModels.Remove(contractModels);
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
