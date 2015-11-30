using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using FreeLance.Code;
using System.Web.Mvc;
using FreeLance.Models;
using Microsoft.AspNet.Identity;



namespace FreeLance.Controllers
{
	[Authorize(Roles = "Admin, Coordinator, Freelancer, Employer")]
	public class ProblemController : Controller
	{
		private ApplicationDbContext db = new ApplicationDbContext();

		public class DetailsView
		{
			public bool showMore { get; set; }
			public String EmployerId { get; set; }
			public int ProblemId { get; set; }
			public ProblemStatus Status { get; set; }
			public String ProblemName { get; set; }
			public String EmployerName { get; set; }
			public String PhotoPath { get; set; }
			public String EmployerEmail { get; set; }
			public String ProblemShortDescription { get; set; }
			public String ProblemFullDescription { get; set; }
			public String CreatingDate { get; set; }
			public String DeadlineDate { get; set; }
			public decimal Cost { get; set; }
			public int AmountOfWorkers { get; set; }
			public List<ContractInfoModel> ContractsInProgress { get; set; }
			public List<ContractInfoModel> ContratsClosed { get; set; }
			public List<SubscriberInfoModel> Subscribers { get; set; }

			public bool IsSubscibed { get; set; }
			public bool? IsApproved { get; set; }
		}

		public class ContractInfoModel
		{
			public int ContractId { get; set; }
			public String FreelancerName { get; set; }
			public String FreelancerId { get; set; }
		}

		public class SubscriberInfoModel
		{
			public String FreelancerName { get; set; }
			public String FreelancerId { get; set; }
			public decimal FreelancerRate { get; set; }
		}

		public List<ContractInfoModel> getContractsWithStatus(ICollection<ContractModels> contracts,
			List<ContractStatus> statuses)
		{
			List<ContractInfoModel> result = new List<ContractInfoModel>();

			foreach (var c in contracts)
			{
				if (statuses.Contains(c.Status))
				{
					result.Add(new ContractInfoModel
					{
						ContractId = c.ContractId,
						FreelancerId = c.Freelancer.Id,
						FreelancerName = c.Freelancer.FIO
					});
				}
			}
			return result;
		}

		public List<SubscriberInfoModel> getProblemSubcribers(ICollection<SubscriptionModels> subscribers)
		{
			List<SubscriberInfoModel> result = new List<SubscriberInfoModel>();

			foreach (var s in subscribers)
			{
				List<ContractModels> contracts = db.ContractModels
					.Where(c => c.Freelancer.Id == s.Freelancer.Id).ToList(); 
				result.Add(new SubscriberInfoModel
				{
					FreelancerId = s.Freelancer.Id,
					FreelancerName = s.Freelancer.FIO,
					FreelancerRate = FreelancerController.getFreelancerRate(contracts)
				});
			}

			return result;
		}

		public DetailsView getProblemDetails(ProblemModels p)
		{
			DetailsView details = new DetailsView
			{
				ProblemId = p.ProblemId,
				Status = p.Status,
				EmployerId = p.Employer.Id,
				PhotoPath = Utils.GetPhotoUrl(p.Employer.PhotoPath), 
				EmployerName = p.Employer.FIO,
				EmployerEmail = p.Employer.Email,
				ProblemName = p.Name,
				ProblemShortDescription = p.SmallDescription,
				ProblemFullDescription = p.Description,
				CreatingDate = p.CreationDate.ToShortDateString(),
				DeadlineDate = p.DeadlineDate.ToShortDateString(), 
				Cost = p.Cost,
				AmountOfWorkers = p.AmountOfWorkes
			};

			details.ContractsInProgress = getContractsWithStatus(p.Contracts,
				new List<ContractStatus> { ContractStatus.InProgress,
					ContractStatus.Opened, ContractStatus.Done, ContractStatus.ClosedNotPaid});

			details.ContratsClosed = getContractsWithStatus(p.Contracts,
				new List<ContractStatus> { ContractStatus.Closed,
					ContractStatus.Failed, ContractStatus.СancelledByEmployer,
					ContractStatus.СancelledByFreelancer});

			details.Subscribers = getProblemSubcribers(p.Subscriptions);

			return details;
		}

		public ActionResult Details(int? id, bool? showMore)
		{
			string userId = User.Identity.GetUserId();
			if (User.IsInRole("Freelancer"))
			{
				ApplicationUser freelancer = db.Users.Find(userId);
				bool withLawContract = db.LawContracts.Where(c => c.User.Id == userId).Count() > 0 ? true : false;
				if (!withLawContract)
				{
					ViewBag.ErrorMessage = "Вам не заплатят за выполненную работу, пока вы не заключите ГПХ.";
				}
			}

			if (id == null)
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}

			if(showMore == null)
			{
				showMore = false;
			}

			ProblemModels problem = db.ProblemModels.Find(id);
			if (problem == null)
			{
				return HttpNotFound();
			}

			DetailsView view = getProblemDetails(problem);

			//For Freelancer view	
			SubscriptionModels[] subscriptions = db.SubscriptionModels.Where(sub => sub.Freelancer.Id == userId
													&& sub.Problem.ProblemId == id).Distinct().ToArray();
			SubscriptionModels subscription = subscriptions.Length > 0 ? subscriptions[0] : null;
			view.IsSubscibed = subscription != null;
			view.IsApproved = db.Users.Find(userId).IsApprovedByCoordinator == true;

			view.showMore = (bool)showMore;

			return View(view);
		}

		// GET: Problem
		[Authorize(Roles = "Employer")]
		public ActionResult Index()
		{
			return View(db.ProblemModels.ToList());
		}


		[Authorize(Roles = "Employer")]
		public ActionResult Create()
		{
			ApplicationUser employer = db.Users.Find(User.Identity.GetUserId());
			if (employer.IsApprovedByCoordinator != true)
			{
				ViewBag.ErrorMessage = "Ваши задачи не будут показаны исполнителям, пока ваш аккаунт не подтвердит координатор";
			}
			return View();
		}

		[HttpPost]
		[Authorize(Roles = "Employer")]
		[ValidateAntiForgeryToken]
		public ActionResult Create(ProblemModels problem)
		{
			ApplicationUser employer = db.Users.Find(User.Identity.GetUserId());
			if (problem.Description == null)
			{
				return View();
			}
			if (employer.IsApprovedByCoordinator != true)
			{
				ViewBag.ErrorMessage = "Задача создана, но не будет показана исполнителям, пока ваш аккаунт не подтвердит координатор";
			}
			problem.Employer = db.Users.Find(User.Identity.GetUserId());
			problem.Status = ProblemStatus.Opened;
			problem.CreationDate = DateTime.Now;
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
		[Authorize(Roles = "Employer")]
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
		[Authorize(Roles = "Employer")]
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
		[Authorize(Roles = "Admin")]
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
		[Authorize(Roles = "Admin")]
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
