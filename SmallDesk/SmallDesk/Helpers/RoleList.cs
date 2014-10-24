using Microsoft.AspNet.Identity.EntityFramework;
using SmallDesk.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SmallDesk.Helpers
{
    public class RoleList
    {
        public static IEnumerable<SelectListItem> CreateSelectableList(ApplicationDbContext context)
        {
            List<IdentityRole> roles = context.Roles.ToList();
            List<SelectListItem> list = new List<SelectListItem>();

            roles.ForEach((role) =>
            {
                list.Add(new SelectListItem()
                {
                    Text = role.Name,
                    Value = role.Id,
                    Selected = role.Name == "User"
                });
            });

            return list;
        }
    }
}