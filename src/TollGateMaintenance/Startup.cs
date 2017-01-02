using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using TollGateMaintenance.Models;

namespace TollGateMaintenance
{
    public class Startup
    {
        public static IConfiguration Config;

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddConfiguration(out Config);
            services.AddEntityFrameworkMySql()
                .AddDbContext<TgmContext>(x => x.UseMySql(Config["ConnectionStrings:DefaultConnection"]));
            services.AddIdentity<User, IdentityRole>(x =>
            {
                x.Password.RequireDigit = false;
                x.Password.RequiredLength = 0;
                x.Password.RequireLowercase = false;
                x.Password.RequireNonAlphanumeric = false;
                x.Password.RequireUppercase = false;
                x.User.AllowedUserNameCharacters = null;
            })
                .AddDefaultTokenProviders()
                .AddEntityFrameworkStores<TgmContext>();
            services.AddSmartUser<User, string>();
            services.AddWebPages();
            services.AddSignalR();
        }

        public async void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole();
            app.UseSignalR();
            app.UseIdentity();
            app.UseDeveloperExceptionPage();
            app.UseWebPages();
            app.UseStaticFiles();
            await app.InitDatabaseAsync();
        }
    }
}
