using FleetApp.BLL;
using FleetApp.Models;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Configuration; // Required reference

namespace FleetApp.DAL
{
    public class FleetServiceSP : IFleetService
    {
        // ! IMPORTANT: CONFIGURATION REQUIRED
        // Your teammate must ensure System.Configuration is referenced in FleetApp.DAL
        private readonly string connectionString =
            ConfigurationManager.ConnectionStrings["FleetDB"].ConnectionString;

        // Utility method to execute raw SQL and return a DataTable
        private DataTable ExecuteRawSql(string sql, params SqlParameter[] parameters)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.CommandType = sql.Trim().StartsWith("EXEC") ? CommandType.StoredProcedure : CommandType.Text;
                cmd.Parameters.AddRange(parameters);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
            }
            return dt;
        }

        // --- Complete IFleetService Implementations ---

        public List<Vehicle> GetAllVehicles() { /* Implement GetAllVehicles */ }
        public Vehicle GetVehicleById(int id) { /* Implement GetVehicleById */ }
        public void AddVehicle(Vehicle vehicle) { /* Implement AddVehicle */ }

        public void DeleteVehicle(int vehicleId)
        {
            // The INSTEAD OF trigger trg_PreventDirectVehicleDeletion handles the actual logic
            ExecuteRawSql("DELETE FROM Vehicles WHERE VehicleID = @id", new SqlParameter("@id", vehicleId));
        }

        public void UpdateVehicleStatus(int vehicleId, string status)
        {
            // Calls Phase 2 Stored Procedure sp_UpdateVehicleStatus
            ExecuteRawSql("sp_UpdateVehicleStatus",
                new SqlParameter("@VehicleID", vehicleId),
                new SqlParameter("@NewStatus", status));
        }

        public DataTable GetVehicleSummary(int vehicleId)
        {
            // Calls Phase 2 Stored Procedure sp_GetVehicleSummary
            return ExecuteRawSql("sp_GetVehicleSummary", new SqlParameter("@VehicleID", vehicleId));
        }

        public DataTable GetVehicleHistory(int vehicleId)
        {
            // Calls Phase 2 Table-Valued Function (TVF) fn_GetVehicleHistory
            string query = $"SELECT * FROM fn_GetVehicleHistory(@TargetVehicleID)";
            return ExecuteRawSql(query, new SqlParameter("@TargetVehicleID", vehicleId));
        }

        public DataTable GetHighMaintenanceVehicles()
        {
            // Uses Phase 2 View vw_HighMaintenanceVehicles
            return ExecuteRawSql("SELECT * FROM vw_HighMaintenanceVehicles");
        }

        public void AddTrip(Trip trip) { /* Implement AddTrip (triggers mileage update) */ }
        public void AddMaintenanceRecord(MaintenanceRecord record) { /* Implement AddMaintenanceRecord (triggers cost validation) */ }
    }
}