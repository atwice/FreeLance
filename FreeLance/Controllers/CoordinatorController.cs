using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using FreeLance.Models;

namespace FreeLance.Controllers
{
    [Authorize(Roles = "Coordinator")]
    public class CoordinatorController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public class HomeViewModel
        {
            public List<ApplicationUser> Incognitos { get; set; }
        }

        public class FreelancersViewModel
        {
            public List<ApplicationUser> Freelancers { get; set; }
        }

        // GET: Coordinator
        public ActionResult Index()
        {
            return RedirectToAction("Home");
        }

        public ActionResult Home()
        {
            var model = new HomeViewModel();
            model.Incognitos = getApplicationUsersInRole("Incognito").ToList();
            return View(model);
        }

        public ActionResult Freelancers()
        {
            var model = new FreelancersViewModel();
            model.Freelancers = getApplicationUsersInRole("Freelancer").ToList();
            return View(model);
        }

        private IEnumerable<ApplicationUser> getApplicationUsersInRole(string roleName)
        {
            return from role in db.Roles
                   where role.Name == roleName
                   from userRoles in role.Users
                   join user in db.Users
                   on userRoles.UserId equals user.Id
                   select user;
        }

    }
}