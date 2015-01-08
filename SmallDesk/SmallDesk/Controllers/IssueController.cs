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
using System.Data.Entity;
using PagedList;

namespace SmallDesk.Controllers
{
    [Authorize]
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
        [Authorize(Roles = "Admin, Support")]
        public ActionResult Index(string sortOrder, string problem, 
            string supportUser, string userThatReport, bool? closed = true,
            int? page = 1, bool? returnPdf = false)
        {
            var query = Database.Issues.AsQueryable();

            ViewBag.problem = problem;
            ViewBag.sortOrder = sortOrder ?? "create_at";
            ViewBag.supportUser = supportUser;
            ViewBag.userThatReport = userThatReport;
            ViewBag.hasFilter = false;
            ViewBag.orderBy = String.Empty;
            ViewBag.closed = closed;

            if (!String.IsNullOrEmpty(sortOrder))
            {
                switch (sortOrder)
                {
                    case "create_at_desc":
                        query = query.OrderByDescending(t => t.CreatedAt);
                        ViewBag.orderBy = "Data Inclusão (Desc)";
                        break;
                    case "support_user":
                        query = query.OrderBy(t => t.SupportUser.UserName);
                        ViewBag.orderBy = "Técnico (Asc)";
                        break;
                    case "support_user_desc":
                        query = query.OrderByDescending(t => t.SupportUser.UserName);
                        ViewBag.orderBy = "Técnico (Desc)";
                        break;
                    case "user_that_report":
                        query = query.OrderBy(t => t.UserThatReported.UserName);
                        ViewBag.orderBy = "Usuário Reportador (Asc)";
                        break;
                    case "user_that_report_desc":
                        query = query.OrderByDescending(t => t.UserThatReported.UserName);
                        ViewBag.orderBy = "Usuário Reportador (Desc)";
                        break;
                    default:
                        ViewBag.sortOrder = "create_at";
                        query = query.OrderBy(t => t.CreatedAt);
                        break;
                }
            }

            if (!String.IsNullOrEmpty(problem))
            {
                ViewBag.hasFilter = true;
                query = query.Where(t => t.ProblemData.Contains(problem));
            }

            if (!String.IsNullOrEmpty(supportUser))
            {
                ViewBag.hasFilter = true;
                query = query.Where(t => t.SupportUser.UserName.Contains(supportUser));
            }

            if (!String.IsNullOrEmpty(userThatReport))
            {
                ViewBag.hasFilter = true;
                query = query.Where(t => t.UserThatReported.UserName.Contains(userThatReport));
            }

            if (closed == false)
            {
                ViewBag.hasFilter = true;
                query = query.Where(t => !t.IsSolved);
            }

            int pageSize = 10;
            int pageNumber = (page ?? 1);

            var issues = query.ToList().ToPagedList(pageNumber, pageSize);

            if (returnPdf == true)
                return new RazorPDF.PdfResult(issues, "ExportPDF");
            else
                return View(issues);
        }

        [Authorize(Roles = "Admin, Support")]
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
        [Authorize(Roles = "Admin, Support")]
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

            ViewBag.userSupportList = userSupportList;
            ViewBag.userList = userList;
            return View();
        }

        [Authorize(Roles = "Admin, Support")]
        public async Task<ActionResult> Edit(int id)
        {
            var issue = await Database.Issues.FindAsync(id);

            if (issue == null)
            {
                return View("Error");
            }

            if (issue.IsSolved)
            {
                var error = new Exception("Não é possível editar chamados já resolvidos.");
                return View("Error", new HandleErrorInfo(error, "IssueController", "Edit"));
            }
            
            IssueEditModel editIssue = new IssueEditModel()
            {
                Id = issue.Id,
                IsSolved = issue.IsSolved,
                ProblemData = issue.ProblemData,
                SolutionData = issue.SolutionData,
                SupportUser_Id = issue.SupportUser_Id,
                UserThatReported_Id = issue.UserThatReported_Id,
                ExpectedAt = issue.ExpectedAt
            };

            var userSupportList = UserList.UserListByRole(Database, "Support", editIssue.SupportUser_Id);
            var userList = UserList.UserListByRole(Database, "User", editIssue.UserThatReported_Id);

            userList = userList.Concat(userSupportList.Where(t => t.Text != "Nenhum")).ToList();

            ViewBag.userSupportList = userSupportList;
            ViewBag.userList = userList;

            return View(editIssue);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, Support")]
        public async Task<ActionResult> Edit(IssueEditModel issue)
        {
            if (ModelState.IsValid)
            {
                var issueFromDb = await Database.Issues.FindAsync(issue.Id);

                if (issueFromDb != null)
                {
                    if (!issueFromDb.IsSolved)
                    {
                        if (issue.IsSolved && 
                            (String.IsNullOrEmpty(issue.SolutionData) || String.IsNullOrEmpty(issue.SupportUser_Id)))
                        {
                            ModelState.AddModelError("", "Para encerrar o chamado, é necessário colocar a solução do problema e o técnico que resolveu.");
                        }
                        else
                        {
                            issueFromDb.IsSolved = issue.IsSolved;
                            issueFromDb.ProblemData = issue.ProblemData;
                            issueFromDb.SolutionData = issue.SolutionData;
                            issueFromDb.ExpectedAt = issue.ExpectedAt;
                            issueFromDb.SupportUser_Id = issue.SupportUser_Id;
                            issueFromDb.UserThatReported_Id = issue.UserThatReported_Id;

                            if (issue.IsSolved)
                                issueFromDb.ResolvedAt = DateTime.Now;

                            Database.Entry(issueFromDb).State = EntityState.Modified;
                            await Database.SaveChangesAsync();

                            return RedirectToAction("Index");
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", "Não é possível editar chamados fechados.");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Não foi encontrado o chamado desejado.");
                }
            }

            var userSupportList = UserList.UserListByRole(Database, "Support", issue.SupportUser_Id);
            var userList = UserList.UserListByRole(Database, "User", issue.UserThatReported_Id);

            userList = userList.Concat(userSupportList.Where(t => t.Text != "Nenhum")).ToList();

            ViewBag.userSupportList = userSupportList;
            ViewBag.userList = userList;

            return View(issue);
        }

        public async Task<ActionResult> Details(int id)
        {
            var issue = await Database.Issues.FindAsync(id);
            var user = await UserManager.FindByNameAsync(User.Identity.Name);

            if (User.IsInRole("User") && (user.Id != issue.UserThatReported_Id))
            {
                var error = new Exception("Não é possível visualizar chamados que não foram abertos para você.");
                return View("Error", new HandleErrorInfo(error, "IssueController", "Detail"));
            }

            return View(issue);
        }
    }
}