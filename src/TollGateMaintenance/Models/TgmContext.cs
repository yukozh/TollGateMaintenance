using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace TollGateMaintenance.Models
{
    public class TgmContext : IdentityDbContext<User>
    {
        public TgmContext(DbContextOptions opt) : base(opt) { }

        public DbSet<Report> Reports { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<Report>(e => 
            {
                e.HasIndex(x => x.TollGate);
                e.HasIndex(x => x.Time);
            });
        }
    }
}
