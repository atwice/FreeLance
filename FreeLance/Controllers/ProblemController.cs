using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using FreeLance.Models;

namespace FreeLance.Controllers
{
	public class ProblemController : Controller
	{
		private ApplicationDbContext db = new ApplicationDbContext();

		// GET: Problem
		public ActionResult Index()
		{
			return View(db.ProblemModels.ToList());
		}

		// GET: Problem/Details/5
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
			return View(problemModels);
		}

		// GET: Problem/Create
		public ActionResult Create()
		{
			return View();
		}

		// POST: Problem/Create
		// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
		// more details see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Create([Bind(Include = "ProblemId,Name,Description,Type")] ProblemModels problemModels)
		{
			if (ModelState.IsValid)
			{
				db.ProblemModels.Add(problemModels);
				db.SaveChanges();
				return RedirectToAction("Index");
			}

			return View(problemModels);
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
		public ActionResult Edit([Bind(Include = "ProblemId,Name,Description,Type")] ProblemModels problemModels)
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
