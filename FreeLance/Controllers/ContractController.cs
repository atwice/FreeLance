﻿using System;
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
	[Authorize]
	public class ContractController : Controller
	{
		private ApplicationDbContext db = new ApplicationDbContext();

		// GET: Contract
		[Authorize(Roles = "Admin")]
		public ActionResult Index()
		{
			return View(db.ContractModels.Where(c => !c.IsHidden).ToList());
		}

		public class DetailsView
		{
			public bool showMore { get; set; }
			public String Comment { get; set; }
			public String EmployerId { get; set; }
			public string EmployerName { get; set; }
			public string EmployerEmail { get; set; }
			public int ProblemId { get; set; }
			public ContractStatus Status { get; set; }
			public String ProblemName { get; set; }
			public String FreelancerName { get; set; }
			public String FreelancerId { get; set; }
			public String FreelancerPhotoPath { get; set; }
			public String EmployerPhotoPath { get; set; }
			public String FreelancerEmail { get; set; }
			public String Details { get; set; }
			public String CreatingDate { get; set; }
			public String DeadlineDate { get; set; }
			public String EndingDate { get; set; }
			public decimal Cost { get; set; }
			public int ContractId { get; set; }
			public decimal Rate { get; set; }
			public bool IsHidden { get; set; }


			public class ChangeStatusButton
			{
				public string Text { get; set; }
				public string Classes { get; set; }
				public string Redirect { get; set; }
				public Models.ContractStatus Status { get; set; }
			}

			public List<ChangeStatusButton> ChangeStatusButtons;
			public ChangeStatusButton finishButton;
		}

		public DetailsView getContractDetails(ContractModels c)
		{
			DetailsView details = new DetailsView
			{
				Comment = c.Comment,
				Rate = c.Rate,
				EmployerId = c.Problem.Employer.Id,
				EmployerName = c.Problem.Employer.FIO,
				EmployerEmail = c.Problem.Employer.Email,
				FreelancerName = c.Freelancer.FIO,
				FreelancerEmail = c.Freelancer.Email,
				FreelancerId = c.Freelancer.Id,
				ProblemName = c.Problem.Name,
				IsHidden = c.IsHidden,
				CreatingDate = c.CreationDate.ToShortDateString(),
				EndingDate = c.EndingDate.ToShortDateString(),
				DeadlineDate = DateTime.Now.AddDays(100).ToShortDateString(), //TODO
				Cost = c.Cost,
				ProblemId = c.Problem.ProblemId,
				Status = c.Status,
				Details = c.Details,
				EmployerPhotoPath = Utils.GetPhotoUrl(c.Problem.Employer.PhotoPath),
				FreelancerPhotoPath = Utils.GetPhotoUrl(c.Freelancer.PhotoPath),
				ContractId = c.ContractId
			};

			return details;
		}

		// GET: Contract/Details/5
		[Authorize(Roles = "Employer,Freelancer,Admin,Coordinator")]
		public ActionResult Details(int? id, bool? showMore)
		{
			if (id == null)
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}

			ContractModels contract = db.ContractModels.Include(c => c.Problem).Single(c => c.ContractId == id);
			// only coordinator can view hidden contracts
			if (contract == null || (contract.IsHidden && !User.IsInRole("Coordinator")))
			{
				return HttpNotFound();
			}
			if ((User.IsInRole("Freelancer") && User.Identity.GetUserId() != contract.Freelancer.Id)
				|| (User.IsInRole("Employer") && User.Identity.GetUserId() != contract.Problem.Employer.Id))
			{
				return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
			}

			if (showMore == null)
			{
				showMore = false;
			}
			
			DetailsView view = getContractDetails(contract);

			view.showMore = (bool)showMore;

			view.ChangeStatusButtons = new List<DetailsView.ChangeStatusButton>();

			if (User.IsInRole("Freelancer"))
			{
				if (view.Status == ContractStatus.Opened)
				{
					view.ChangeStatusButtons.Add(new DetailsView.ChangeStatusButton
					{
						Text = "Подписать контракт",
						Classes = "btn-success",
						Status = ContractStatus.InProgress,
						Redirect = "/Contract/Details/" + view.ContractId
					});
					view.ChangeStatusButtons.Add(new DetailsView.ChangeStatusButton
					{
						Text = "Отклонить контракт",
						Classes = "btn-danger",
						Status = ContractStatus.Failed,
						Redirect = "/Freelancer/Home"
					});
				}
				else if (view.Status == ContractStatus.InProgress)
				{
					view.ChangeStatusButtons.Add(new DetailsView.ChangeStatusButton
					{
						Text = "Завершить контракт",
						Classes = "btn-success",
						Status = ContractStatus.Done,
						Redirect = "/Contract/Details/" + view.ContractId
					});
					view.ChangeStatusButtons.Add(new DetailsView.ChangeStatusButton
					{
						Text = "Прервать контракт",
						Classes = "btn-danger",
						Status = ContractStatus.СancelledByFreelancer,
						Redirect = "/Contract/Details/" + view.ContractId
					});
				}
				else if (view.Status == ContractStatus.Done)
				{
					view.ChangeStatusButtons.Add(new DetailsView.ChangeStatusButton
					{
						Text = "Открыть заново",
						Classes = "btn-danger",
						Status = ContractStatus.InProgress,
						Redirect = "/Contract/Details/" + view.ContractId
					});
				}
			}
			else if (User.IsInRole("Employer"))
			{
				if (view.Status == ContractStatus.Done)
				{
					view.ChangeStatusButtons.Add(new DetailsView.ChangeStatusButton
					{
						Text = "Отправить на доработку",
						Classes = "btn-danger",
						Status = ContractStatus.InProgress,
						Redirect = "/Contract/Details/" + view.ContractId
					});
					view.finishButton = new DetailsView.ChangeStatusButton
					{
						Text = "Принять работу",
						Classes = "btn-success",
						Status = ContractStatus.ClosedNotPaid,
						Redirect = "/Contract/Details/" + view.ContractId
					};
				}
				else if (view.Status == ContractStatus.InProgress
						  || view.Status == ContractStatus.Opened)
				{
					view.finishButton = new DetailsView.ChangeStatusButton
					{
						Text = "Прервать контракт",
						Classes = "btn-danger",
						Status = ContractStatus.СancelledByEmployer,
						Redirect = "/Employer/Archive"
					};
				}
			}
			return View(view);
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
			return View(new ContractModels
			{
				Problem = problem,
				Freelancer = freelancer,
				CreationDate = DateTime.Now,
				EndingDate = DateTime.Now.AddDays(15).AddHours(3)
			});
		}

		// POST: Contract/Create
		// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
		// more details see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		[Authorize(Roles = "Employer")]
		public ActionResult Create([Bind(Include = "Details,Cost,DeadlineDate")] ContractModels contract,
			int problemId, string freelancerId, string redirect)
		{
			string userId = User.Identity.GetUserId();
			ProblemModels problem = db.ProblemModels.Find(problemId);
			ApplicationUser freelancer = db.Users.Find(freelancerId);
			SubscriptionModels[] subscriptions = db.SubscriptionModels.Where(sub => sub.Freelancer.Id == freelancerId
													&& sub.Problem.ProblemId == problemId).Distinct().ToArray();
			SubscriptionModels subscription = subscriptions.Length > 0 ? subscriptions[0] : null;
			if (subscription == null || problem == null || freelancer == null || problem.Employer.Id != userId || problem.IsHidden) // || problem.Status == Opened
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}
			contract.Problem = problem;
			contract.Status = ContractStatus.Opened;
			contract.Freelancer = freelancer;
			contract.CreationDate = DateTime.Now;
			contract.EndingDate = DateTime.Now;


			db.ContractModels.Add(contract);
			db.SubscriptionModels.Remove(subscription);
			db.SaveChanges();
			return Redirect(redirect == null ? "/Contract/Details/" + contract.ContractId.ToString() : redirect);
		}

		[HttpPost]
		[Authorize(Roles = "Employer, Freelancer")]
		public ActionResult ChangeStatus(int id, ContractStatus status, string redirect, 
			string freelancerId, string employerId, string contractName)
		{
			ContractModels contract = db.ContractModels.Include(c => c.Problem).Single(c => c.ContractId == id);
			if (contract == null || contract.Freelancer == null || contract.IsHidden
				|| (User.IsInRole("Employer") && contract.Problem.Employer.Id != User.Identity.GetUserId())
				|| (User.IsInRole("Freelancer") && contract.Freelancer.Id != User.Identity.GetUserId()))
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}
			ContractStatus previousStatus = contract.Status;
			contract.Status = status;
            //  Failed, СancelledByFreelancer, СancelledByEmployer, ClosedNotPaid
            if (status == ContractStatus.ClosedNotPaid || status == ContractStatus.СancelledByEmployer
                || status == ContractStatus.СancelledByFreelancer || status == ContractStatus.Failed)
			{
				contract.EndingDate = DateTime.Now;
			}
			db.SaveChanges();

			EmailManager.Send(new OnStatusChangeBuilder(freelancerId, contractName, "/Contract/Details/" + id.ToString(), previousStatus, status));
			EmailManager.Send(new OnStatusChangeBuilder(employerId, contractName, "/Contract/Details/" + id.ToString(), previousStatus, status));

			return Redirect(redirect == null ? "/Contract/Details/" + id.ToString() : redirect);
		}

		[HttpPost]
		[Authorize(Roles = "Employer, Freelancer")]
		public ActionResult Close(int id, string comment, int rate, ContractStatus newStatus, 
			string freelancerId, string employerId, string contractName)
		{
			ContractModels contract = db.ContractModels.Include(c => c.Problem).Single(c => c.ContractId == id);
			if (contract == null || contract.Freelancer == null || contract.IsHidden
				|| contract.Problem.Employer.Id != User.Identity.GetUserId())
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}
			contract.Rate = rate;
			contract.Comment = comment;

			ContractStatus previousStatus = contract.Status;
			contract.Status = newStatus;

			contract.EndingDate = DateTime.Now;
			db.SaveChanges();

			EmailManager.Send(new OnStatusChangeBuilder(freelancerId, contractName, "/Contract/Details/" + id.ToString(), previousStatus, newStatus));
			EmailManager.Send(new OnStatusChangeBuilder(employerId, contractName, "/Contract/Details/" + id.ToString(), previousStatus, newStatus));

			return Redirect("/Contract/Details/" + id.ToString());
		}


		// GET: Contract/Edit/5
		[Authorize(Roles = "Admin")]
		public ActionResult Edit(int? id)
		{
			if (id == null)
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}
			ContractModels contractModels = db.ContractModels.Find(id);
			if (contractModels == null || contractModels.IsHidden)
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
		[Authorize(Roles = "Admin")]
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
		[Authorize(Roles = "Admin")]
		public ActionResult Delete(int? id)
		{
			if (id == null)
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}
			ContractModels contractModels = db.ContractModels.Find(id);
			if (contractModels == null || contractModels.IsHidden)
			{
				return HttpNotFound();
			}
			return View(contractModels);
		}

		// POST: Contract/Delete/5
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		[Authorize(Roles = "Admin")]
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
