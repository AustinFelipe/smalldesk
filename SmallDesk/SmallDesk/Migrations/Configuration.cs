namespace SmallDesk.Migrations
{
    using Microsoft.AspNet.Identity.EntityFramework;
    using SmallDesk.Models;
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<SmallDesk.Models.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            ContextKey = "SmallDesk.Models.ApplicationDbContext";
        }

        protected override void Seed(SmallDesk.Models.ApplicationDbContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data. E.g.
            //
            //    context.People.AddOrUpdate(
            //      p => p.FullName,
            //      new Person { FullName = "Andrew Peters" },
            //      new Person { FullName = "Brice Lambson" },
            //      new Person { FullName = "Rowan Miller" }
            //    );
            //

            context.Roles.AddOrUpdate(
                p => p.Name,
                new IdentityRole("Admin"),
                new IdentityRole("Support"),
                new IdentityRole("User")
            );

            // Cria departamentos comuns
            context.Departments.AddOrUpdate(
                p => p.Description,
                new Department { Description = "Compras" },
                new Department { Description = "Faturamento" },
                new Department { Description = "Geral" },
                new Department { Description = "Vendas" },
                new Department { Description = "Administração" },
                new Department { Description = "TI" }
            );

            IdentityRole role = context.Roles.Where(t => t.Name == "Admin").FirstOrDefault();

            // Cria usuário admin
            // CUIDADO!!! Não trocar o DepartmentId, só se tiver certeza de qual é o TI
            context.Users.AddOrUpdate(
                p => p.Id,
                new ApplicationUser { 
                    Id = "e40cea5e-394b-4fd9-a160-254d50387d86",
                    Email = "sistema@thermoglass.com.br",
                    EmailConfirmed = false,
                    PasswordHash = "AI/xkn6gPIxLG18uE8cWZKMcApCYuwYCEEYbZTiYGJV6c5XAj7Pa9/IeJl4aHItSug==",
                    SecurityStamp = "e4bb309f-754d-40ca-9640-dd82e36046c1",
                    UserName = "admin",
                    DepartmentId = 6 // Ti
                }
            );

            ApplicationUser admin = context.Users.Where(t => t.UserName == "admin").FirstOrDefault();

            if (admin.Roles.FirstOrDefault(t => t.RoleId == role.Id) == null)
                context.Database.ExecuteSqlCommand(
                    "INSERT INTO AspNetUserRoles (UserId, RoleId) VALUES ('e40cea5e-394b-4fd9-a160-254d50387d86', '" + 
                    role.Id + "');");

            context.Companies.AddOrUpdate(
                p => p.Id,
                new Company() { Description = "Thermo Matriz" },
                new Company() { Description = "Thermo Yaya" },
                new Company() { Description = "Twin Matriz" },
                new Company() { Description = "Twin Filial" },
                new Company() { Description = "Primi Vidros" }
            );
        }
    }
}
