using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace TestMVCApplication.Models
{
    public class PostgresContext : DbContext
    {
        public PostgresContext()
            : base("PostgresConnection")
            {
            }

        public DbSet<CACensusData> CACensusData { get; set; }
        public DbSet<CoffeeOutlets> Starbucks { get; set; }    
    }
}