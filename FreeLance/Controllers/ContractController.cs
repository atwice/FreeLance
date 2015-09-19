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
		public ActionResult Create()
		{
			return View();
		}

		// POST: Contract/Create
		// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
		// more details see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Create([Bind(Include = "ContractId,Details,Status")] ContractModels contractModels)
		{
			if (ModelState.IsValid)
			{
				db.ContractModels.Add(contractModels);
				db.SaveChanges();
				return RedirectToAction("Index");
			}

			return View(contractModels);
		}

		[HttpPost]
		[Authorize(Roles = "Freelancer")]
		public ActionResult ChangeStatus(int id, ContractStatus status, string redirect)
		{
			string userId = User.Identity.GetUserId();
			ContractModels contract = db.ContractModels.Find(id);
			if (contract == null || contract.Freelancer == null || contract.Freelancer.Id != userId)
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
