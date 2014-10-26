using SmallDesk.Models;
using System;
using Microsoft.AspNet.Identity.Owin;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SmallDesk.Helpers;
using Microsoft.Owin.Security;
using System.Threading.Tasks;

namespace SmallDesk.Controllers
{
    [Authorize(Roles = "Admin, Support")]
    public class IssueController : Controller
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

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        // GET: Issue
        public ActionResult Index()
        {
            var issues = Database.Issues.ToList();
            return View(issues);
        }

        public ActionResult Create()
        {
            var userSupportList = UserList.UserListByRole(Database, "Support");
            var userList = UserList.UserListByRole(Database, "User");

            userList = userList.Concat(userSupportList.Where(t => t.Text != "Nenhum")).ToList();

            ViewBag.userSupportList = userSupportList;
            ViewBag.userList = userList;
            
            Issue newIssue = new Issue()
            {
                ExpectedAt = DateTime.Now.AddHours(1)
            };

            return View(newIssue);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(Issue issue)
        {
            issue.CreatedAt = DateTime.Now;

            var userThatIncluded = await UserManager.FindByNameAsync(User.Identity.Name);

            if (userThatIncluded != null)
            {
                issue.UserThatIncluded_Id = userThatIncluded.Id;
                issue.IsSolved = false;
            }
            else
            {
                ModelState.AddModelError("", "Erro ao encontrar usuário logado.");
                goto finish;
            }

            ModelState.Remove("UserThatIncluded_Id");

            if (ModelState.IsValid)
            {
                if (issue.ExpectedAt < DateTime.Now)
                {
                    ModelState.AddModelError("", "A data de resolução esperada não pode ser inferior a data corrent.");
                    goto finish;
                }

                Database.Issues.Add(issue);
                await Database.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            finish:
            var userSupportList = UserList.UserListByRole(Database, "Support");
            var userList = UserList.UserListByRole(Database, "User");

            userList = userList.Concat(userSupportList.Where(t => t.Text != "Nenhum")).ToList();

            ViewBag.Op = "Create";
            ViewBag.userSupportList = userSupportList;
            ViewBag.userList = userList;
            return View();
        }

        public async Task<ActionResult> Edit(int id)
        {
            var issue = await Database.Issues.FindAsync(id);

            if (issue == null)
            {
                return View("Error");
            }
            
            IssueEditModel editIssue = new IssueEditModel()
            {
                Id = issue.Id,
                IsSolved = issue.IsSolved,
                ProblemData = issue.ProblemData,
                SolutionData = issue.SolutionData,
                SupportUser_Id = issue.SupportUser_Id,
                UserThatReported_Id = issue.UserThatIncluded_Id,
                ExpectedAt = issue.ExpectedAt
            };

            var userSupportList = UserList.UserListByRole(Database, "Support", editIssue.SupportUser_Id);
            var userList = UserList.UserListByRole(Database, "User", editIssue.UserThatReported_Id);

            userList = userList.Concat(userSupportList.Where(t => t.Text != "Nenhum")).ToList();

            ViewBag.userSupportList = userSupportList;
            ViewBag.userList = userList;

            return View(editIssue);
        }
    }
}