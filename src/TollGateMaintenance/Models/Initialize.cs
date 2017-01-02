using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

namespace TollGateMaintenance.Models
{
    public static class Initialize
    {
        public static async Task InitDatabaseAsync(this IApplicationBuilder app)
        {
            var DB = app.ApplicationServices.GetRequiredService<TgmContext>();
            DB.Database.EnsureCreated();
            var UserManager = app.ApplicationServices.GetRequiredService<UserManager<User>>();
            if (await UserManager.FindByNameAsync("admin") == null)
            {
                var User = new User { UserName = "admin", Email = "1@1234.sh" };
                var result = await UserManager.CreateAsync(User, "123456");
            }
        }
    }
}
