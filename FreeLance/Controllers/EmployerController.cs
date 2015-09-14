﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using System.Web.Mvc;
using FreeLance.Models;

namespace FreeLance.Controllers
{
	public class EmployerController : Controller
	{
        private ApplicationDbContext db = new ApplicationDbContext();

        public class HomeView
        {
            public List<ContractModels> Contracts { get; set; }
            public List<ProblemModels> Problems { get; set; }
        }

        // GET: Employer
        public ActionResult Home()
		{
			return View();
		}

		public ActionResult Archive()
		{
            return View();
		}

        public ActionResult Problem(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ProblemModels problemModels = db.ProblemModels.Find(id);
            // check if employer is this problem's owner?
            if (problemModels == null)
            {
                return HttpNotFound();
            }
            return View(problemModels);
        }

        public ActionResult Freelancers()
		{
			return View();
		}

        public ActionResult NewProblem()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult NewProblem([Bind(Include = "ProblemId,Name,Description,Status")] ProblemModels problem)
        {
            if (ModelState.IsValid)
            {
                db.ProblemModels.Add(problem);
                db.SaveChanges();
                return RedirectToAction("Problem", new { id = problem.ProblemId });
            }
            
            return View(problem);
        }

    }
}