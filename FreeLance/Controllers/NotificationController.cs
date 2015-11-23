using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FreeLance.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace FreeLance.Controllers
{
	[Authorize]
	public class NotificationController : Controller
	{

		public class NotificationContext
		{
			public ApplicationUser User { get; set; }
			public string Action { get; set; }
			public string Controller { get; set; }
		}

		public interface Notification
		{
			bool WillBeShown(NotificationContext context);
			string GetPartialViewName();
			Object GetPartialViewModel();
		}

		public class ErrorNotification : Notification
		{
			private Func<NotificationContext, bool> willBeShownFunc;
			string text;
			public ErrorNotification(Func<NotificationContext, bool> willBeShown, string text)
			{
				willBeShownFunc = willBeShown;
				this.text = text;
			}
			public bool WillBeShown(NotificationContext context)
			{
				return willBeShownFunc(context);
			}
			public string GetPartialViewName() { return "_ErrorNotification"; }
			public Object GetPartialViewModel() { return text; }
		}


		public List<Notification> NotificationList = new List<Notification> {
			new ErrorNotification(context => checkUserIsInRole(context.User, "Employer") && context.User.IsApprovedByCoordinator != true,
				"Ваш профиль не подтвержден координатором. Вы не можете создавать задачи.")
		};

		[HttpPost]
		public ActionResult GetPageNotifications(string action, string controller)
		{
			NotificationContext context = new NotificationContext
			{
				User = db.Users.Find(User.Identity.GetUserId()),
				Action = action,
				Controller = controller
			};
			List<Notification> selectedNotifications = NotificationList.Where(not => not.WillBeShown(context)).ToList();
			return PartialView(selectedNotifications);
		}

		public ActionResult GetViewBagNotifications(dynamic ViewBag) 
		{
			List<Notification> selectedNotifications = new List<Notification>();
			if (ViewBag.ErrorMessage != null) {
				selectedNotifications.Add(new ErrorNotification(x => true, (string) ViewBag.ErrorMessage));
			}
			return PartialView("GetPageNotifications", selectedNotifications);
		}

		public string Index()
		{
			return "";
		}


		private static ApplicationDbContext db = new ApplicationDbContext();
		private static bool checkUserIsInRole(ApplicationUser user, string roleName)
		{
			IdentityRole role = db.Roles.Where(r => r.Name == roleName).Single();
			return user.Roles.Where(r => r.RoleId == role.Id).Any();
		}
	}
}