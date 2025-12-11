using FleetApp.BLL;
using FleetApp.Models;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Data.SqlClient;

namespace FleetApp.DAL
{
    public class FleetServiceLINQ : IFleetService
    {
        private FleetDBContext context = new FleetDBContext();

        // Utility method (reused from SP service) to execute raw SQL for mandatory features (Views, UDFs, SPs)
        private DataTable ExecuteRawSql(string sql, params SqlParameter[] parameters)
        {
            DataTable dt = new DataTable();
            string connectionString = context.Database.Connection.ConnectionString;
            // ... ADO.NET execution logic (as in FleetServiceSP) ...
            return dt;
        }

        // --- LINQ CRUD Implementations ---
        public List<Vehicle> GetAllVehicles() => context.Vehicles.ToList();
        public Vehicle GetVehicleById(int id) => context.Vehicles.Find(id);

        public void AddVehicle(Vehicle vehicle)
        {
            context.Vehicles.Add(vehicle);
            context.SaveChanges();
        }

        public void DeleteVehicle(int vehicleId)
        {
            // EF translates this to SQL DELETE, invoking the INSTEAD OF trigger
            var vehicle = context.Vehicles.Find(vehicleId);
            if (vehicle != null) { context.Vehicles.Remove(vehicle); context.SaveChanges(); }
        }

        public void AddTrip(Trip trip)
        {
            // EF INSERT invokes the trg_UpdateMileage_AfterTrip trigger
            context.Trips.Add(trip);
            context.SaveChanges();
        }

        public void AddMaintenanceRecord(MaintenanceRecord record)
        {
            // EF INSERT invokes the trg_ValidateCost_AfterMaintenance trigger
            context.MaintenanceRecords.Add(record);
            context.SaveChanges();
        }

        // --- Phase 2 Feature Implementations (using raw SQL/EF for compliance) ---

        public void UpdateVehicleStatus(int vehicleId, string status)
        {
            // Pure LINQ update
            var vehicle = context.Vehicles.Find(vehicleId);
            if (vehicle != null) { vehicle.Status = status; context.SaveChanges(); }
        }

        public DataTable GetVehicleSummary(int vehicleId)
        {
            // Executing the Stored Procedure through EF for compliance
            string sql = "EXEC sp_GetVehicleSummary @VehicleID";
            return ExecuteRawSql(sql, new SqlParameter("@VehicleID", vehicleId));
        }

        public DataTable GetVehicleHistory(int vehicleId)
        {
            // Executing the Table-Valued Function
            string sql = "SELECT * FROM fn_GetVehicleHistory(@TargetVehicleID)";
            return ExecuteRawSql(sql, new SqlParameter("@TargetVehicleID", vehicleId));
        }

        public DataTable GetHighMaintenanceVehicles()
        {
            // Executing the View
            string sql = "SELECT * FROM vw_HighMaintenanceVehicles";
            return ExecuteRawSql(sql);
        }
    }
}