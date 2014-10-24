using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using SmallDesk.Models;
using SmallDesk.Helpers;
using System.Collections.Generic;

namespace SmallDesk.Controllers
{
    [Authorize]
    public class ManageController : Controller
    {
        public ManageController()
        {
        }

        public ManageController(ApplicationUserManager userManager)
        {
            UserManager = userManager;
        }

        private ApplicationUserManager _userManager;
        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        public ApplicationDbContext Database
        {
            get
            {
                return HttpContext.GetOwinContext().GetUserManager<ApplicationDbContext>();
            }
        }

        //
        // GET: /Manage/Index
        public ActionResult Index(string userId, ManageMessageId? message)
        {
            if (isUserTryingAdmin(userId))
                return View("Error");

            ViewBag.StatusMessage =
                message == ManageMessageId.ChangePasswordSuccess ? "A senha foi alterado com sucesso."
                : message == ManageMessageId.SetPasswordSuccess ? "Seu password foi setado."
                : message == ManageMessageId.RoleChanged ? "Regra alterada com sucesso."
                : message == ManageMessageId.DepartmentChanged ? "Setor alterado com sucesso."
                : message == ManageMessageId.Error ? "Ops, Ocorreu um erro. =("
                : "";

            var model = new IndexViewModel
            {
                UserId = String.IsNullOrEmpty(userId) ? "" : userId
            };

            return View(model);
        }

        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> ChangeRole(string userId)
        {
            var user = await UserManager.FindByIdAsync(userId);
            var model = new ChangeRoleModel();

            if (user == null)
            {
                ModelState.AddModelError("", "Usuário não encontrado");
            } 
            else
            {
                model.Role = user.Roles.FirstOrDefault().RoleId ?? "";
                model.UserId = userId;
            }

            var r = RoleList.CreateSelectableList(Database);
            //foreach (var role in r)
            //{
            //    role.Selected = (role.Text == model.Role);
            //}
            ViewBag.Roles = r;
            
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> ChangeRole(ChangeRoleModel model)
        {
            if (ModelState.IsValid)
            {
                var roles = await UserManager.GetRolesAsync(model.UserId);
                var roleFound = Database.Roles.Find(model.Role);

                foreach (var role in roles)
                {
                    await UserManager.RemoveFromRoleAsync(model.UserId, role);
                }

                await UserManager.AddToRoleAsync(model.UserId, roleFound.Name);

                return RedirectToAction("Index", new { userId = model.UserId, message = ManageMessageId.RoleChanged });
            }

            ViewBag.Roles = RoleList.CreateSelectableList(Database);
            return View(model);
        }

        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> ChangeDepartment(string userId)
        {
            var user = await UserManager.FindByIdAsync(userId);
            var model = new ChangeDepartmentModel();

            if (user == null)
            {
                ModelState.AddModelError("", "Usuário não encontrado");
            }
            else
            {
                model.DepartmentId = user.DepartmentId;
                model.UserId = userId;
            }

            var r = DepartamentList.CreateSelectableList(Database);
            ViewBag.Departments = r;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> ChangeDepartment(ChangeDepartmentModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByIdAsync(model.UserId);

                if (user == null)
                {
                    ModelState.AddModelError("", "Usuário não encontrado");
                }
                else
                {
                    user.DepartmentId = model.DepartmentId;
                    var result = await UserManager.UpdateAsync(user);

                    if (result.Succeeded)
                    {
                        return RedirectToAction("Index", new { userId = model.UserId, message = ManageMessageId.DepartmentChanged });
                    }

                    AddErrors(result);
                }
            }

            ViewBag.Roles = RoleList.CreateSelectableList(Database);
            return View(model);
        }

        //
        // GET: /Manage/ChangePassword
        public ActionResult ChangePassword(string userId)
        {
            if (isUserTryingAdmin(userId))
                return View("Error");

            ViewBag.userId = userId;

            return View();
        }

        //
        // POST: /Manage/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangePassword(string userId, ChangePasswordViewModel model)
        {
            if (isUserTryingAdmin(userId))
                return View("Error");

            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPassword, model.NewPassword);
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInAsync(user, isPersistent: false);
                }
                return RedirectToAction("Index", new { Message = ManageMessageId.ChangePasswordSuccess });
            }
            AddErrors(result);
            return View(model);
        }

        //
        // GET: /Manage/SetPassword
        public ActionResult SetPassword()
        {
            return View();
        }

        //
        // POST: /Manage/SetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SetPassword(SetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await UserManager.AddPasswordAsync(User.Identity.GetUserId(), model.NewPassword);
                if (result.Succeeded)
                {
                    var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                    if (user != null)
                    {
                        await SignInAsync(user, isPersistent: false);
                    }
                    return RedirectToAction("Index", new { Message = ManageMessageId.SetPasswordSuccess });
                }
                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

#region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private bool isUserTryingAdmin(string userId)
        {
            if (!String.IsNullOrEmpty(userId) && !User.IsInRole("Admin"))
                return true;

            return false;
        }

        private async Task SignInAsync(ApplicationUser user, bool isPersistent)
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ExternalCookie, DefaultAuthenticationTypes.TwoFactorCookie);
            AuthenticationManager.SignIn(new AuthenticationProperties { IsPersistent = isPersistent }, await user.GenerateUserIdentityAsync(UserManager));
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private bool HasPassword()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user != null)
            {
                return user.PasswordHash != null;
            }
            return false;
        }

        private bool HasPhoneNumber()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user != null)
            {
                return user.PhoneNumber != null;
            }
            return false;
        }

        public enum ManageMessageId
        {
            AddPhoneSuccess,
            ChangePasswordSuccess,
            SetTwoFactorSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
            RemovePhoneSuccess,
            RoleChanged,
            DepartmentChanged,
            Error
        }

#endregion
    }
}