using SmallDesk.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace SmallDesk.Helpers
{
    public static class UserList
    {
        public static IEnumerable<SelectListItem> UserListByRole(ApplicationDbContext context, string role, string selectItem = "")
        {
            string sql = @"select Users.Id, Users.UserName, Users.Email, Roles.Name as Role, '' as Deparment from AspNetUsers as Users
                           inner join AspNetUserRoles UserRoles on UserRoles.UserId = Users.Id
                           inner join AspNetRoles Roles on Roles.Id = UserRoles.RoleId
                           where Roles.Name = @role";

            var qry = context.Database.SqlQuery<UserProfileModel>(sql, new SqlParameter("@role", role)).
                ToList<UserProfileModel>();

            List<SelectListItem> list = new List<SelectListItem>();

            list.Add(new SelectListItem()
            {
                Text = "Nenhum",
                Value = "",
                Selected = true
            });

            qry.ForEach((userProfile) =>
            {
                list.Add(new SelectListItem()
                {
                    Text = userProfile.UserName,
                    Value = userProfile.Id
                });
            });

            list.ForEach((item) =>
                {
                    item.Selected = (item.Value == selectItem);
                });

            return list;
        }
    }
}