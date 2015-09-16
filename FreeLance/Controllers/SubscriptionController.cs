using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net;
using Microsoft.AspNet.Identity;
using FreeLance.Models;

namespace FreeLance.Controllers
{
	public class SubscriptionController : Controller
	{
		private ApplicationDbContext db = new ApplicationDbContext();

		[HttpPost]
		[Authorize(Roles = "Freelancer")]
		public ActionResult Subscribe(int? id)
		{
			if (id == null)
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}
			string userId = User.Identity.GetUserId();
			ApplicationUser freelancer = db.Users.Find(userId);
			ProblemModels problem = db.ProblemModels.Find(id);
			SubscriptionModels[] subscriptions = db.SubscriptionModels.Where(sub => sub.Freelancer.Id == userId
													&& sub.Problem.ProblemId == id).Distinct().ToArray();
			SubscriptionModels subscription = subscriptions.Length > 0 ? subscriptions[0] : null;

			if (subscription == null)
			{
				db.SubscriptionModels.Add(new SubscriptionModels { Freelancer = freelancer, Problem = problem });
				db.SaveChanges();
			}
			return Redirect("/Problem/Details/" + id.ToString());
        }

		[HttpPost]
		[Authorize(Roles = "Freelancer")]
		public ActionResult UnSubscribe(int? id)
		{
			if (id == null)
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}
			string userId = User.Identity.GetUserId();
			ApplicationUser freelancer = db.Users.Find(userId);
			ProblemModels problem = db.ProblemModels.Find(id);
			SubscriptionModels[] subscriptions = db.SubscriptionModels.Where(sub => sub.Freelancer.Id == userId
													&& sub.Problem.ProblemId == id).Distinct().ToArray();
			SubscriptionModels subscription = subscriptions.Length > 0 ? subscriptions[0] : null;

			if (subscription != null)
			{
				db.SubscriptionModels.Remove(subscription);
				db.SaveChanges();
			}
			return Redirect("/Problem/Details/" + id.ToString());
		}
	}
}
