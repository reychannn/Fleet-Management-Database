using FleetApp.Models;
using System.Data.Entity;

namespace FleetApp.DAL
{
    public class FleetDBContext : DbContext
    {
        // Base("FleetDB") tells EF to use the connection string named "FleetDB" in App.config
        public FleetDBContext() : base("FleetDB")
        {
            // Note: EF handles the complexity of table partitioning and indexing transparently
        }

        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<Driver> Drivers { get; set; }
        public DbSet<Trip> Trips { get; set; }
        public DbSet<MaintenanceRecord> MaintenanceRecords { get; set; }
    }
}