using SmallDesk.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SmallDesk.Helpers
{
    public static class DepartamentList
    {
        public static IEnumerable<SelectListItem> CreateSelectableList(ApplicationDbContext context)
        {
            List<Department> departs = context.Departments.ToList();
            List<SelectListItem> list = new List<SelectListItem>();

            departs.ForEach((depart) =>
            {
                list.Add(new SelectListItem()
                {
                    Text = depart.Description,
                    Value = depart.Id.ToString(),
                    Selected = depart.Description == "Geral"
                });
            });
            
            return list;
        }
    }
}