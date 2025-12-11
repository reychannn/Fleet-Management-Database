using FleetApp.Models;
using System.Data.Entity;
using System.Diagnostics;
using System;

namespace FleetApp.DAL
{
    public class FleetDBContext : DbContext
    {
        // Base("FleetDB") tells EF to use the connection string named "FleetDB" in App.config
        public FleetDBContext() : base("FleetDB")
        {
            // Disable database initialization - we're using an existing database
            Database.SetInitializer<FleetDBContext>(null);
            
            try
            {
                // Enable SQL logging to debug window
                Database.Log = s => Debug.WriteLine("EF SQL: " + s);
                
                // Debug: Output connection string to help diagnose issues
                Debug.WriteLine($"FleetDBContext initialized. Connection String: {Database.Connection.ConnectionString}");
                
                // Test the connection immediately
                var canConnect = Database.Exists();
                Debug.WriteLine($"Database exists check: {canConnect}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("ERROR in FleetDBContext constructor: " + ex.Message);
                Debug.WriteLine("Stack Trace: " + ex.StackTrace);
                if (ex.InnerException != null)
                {
                    Debug.WriteLine("Inner Exception: " + ex.InnerException.Message);
                }
                throw;
            }
            // Note: EF handles the complexity of table partitioning and indexing transparently
        }

        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<Driver> Drivers { get; set; }
        public DbSet<Trip> Trips { get; set; }
        public DbSet<MaintenanceRecord> MaintenanceRecords { get; set; }
    }
}