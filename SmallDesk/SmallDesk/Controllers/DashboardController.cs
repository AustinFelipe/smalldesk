using SmallDesk.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity.Owin;

namespace SmallDesk.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        public ApplicationDbContext Database
        {
            get
            {
                return HttpContext.GetOwinContext().Get<ApplicationDbContext>();
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
        }

        // GET: Dashboard
        public async Task<ActionResult> Index()
        {
            var user = await UserManager.FindByNameAsync(User.Identity.Name);

            if (user == null)
            {
                return View("Error");
            }

            var userIssues = User.IsInRole("Admin") ? 
                Database.Issues.ToList() :
                Database.Issues.Where(t => t.UserThatIncluded_Id == user.Id || t.UserThatReported_Id == user.Id).ToList();

            return View(userIssues);
        }
    }
}