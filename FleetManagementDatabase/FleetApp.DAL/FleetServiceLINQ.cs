using System;
using FleetApp.Models;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Data.SqlClient;
using System.Data.Entity.Infrastructure;

namespace FleetApp.DAL
{
    public class FleetServiceLINQ : IFleetService, IDisposable
    {
        private FleetDBContext context = new FleetDBContext();

        private string GetConnectionString()
        {
            return context.Database.Connection.ConnectionString;
        }

        private void SaveChangesWithConstraintHandling()
        {
            try
            {
                context.SaveChanges();
            }
            catch (DbUpdateException ex)
            {
                var root = ex.InnerException?.InnerException ?? ex.InnerException ?? ex;
                throw new InvalidOperationException(root.Message, root);
            }
        }

        // Utility method (reused from SP service) to execute raw SQL for mandatory features (Views, UDFs, SPs)
        private DataTable ExecuteRawSql(string sql, params SqlParameter[] parameters)
        {
            DataTable dt = new DataTable();
            string connectionString = GetConnectionString();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    bool isStoredProc = !sql.Contains(" ") && !sql.Trim().ToUpper().StartsWith("SELECT");
                    cmd.CommandType = isStoredProc ? CommandType.StoredProcedure : CommandType.Text;

                    if (parameters != null && parameters.Length > 0)
                    {
                        cmd.Parameters.AddRange(parameters);
                    }

                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        adapter.Fill(dt);
                    }
                }
            }

            return dt;
        }

        private int ExecuteNonQuery(string sql, params SqlParameter[] parameters)
        {
            using (SqlConnection conn = new SqlConnection(GetConnectionString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    bool isStoredProc = !sql.Contains(" ") && !sql.Trim().ToUpper().StartsWith("SELECT");
                    cmd.CommandType = isStoredProc ? CommandType.StoredProcedure : CommandType.Text;

                    if (parameters != null && parameters.Length > 0)
                    {
                        cmd.Parameters.AddRange(parameters);
                    }

                    return cmd.ExecuteNonQuery();
                }
            }
        }

        // --- LINQ CRUD Implementations ---
        public List<Vehicle> GetAllVehicles() => context.Vehicles.ToList();
        public Vehicle GetVehicleById(int id) => context.Vehicles.Find(id);

        public void AddVehicle(Vehicle vehicle)
        {
            context.Vehicles.Add(vehicle);
            context.SaveChanges();
        }

        public void AddDriver(Driver driver)
        {
            DateTime expiry = driver.LicenseExpiry == default ? DateTime.Now.AddYears(1) : driver.LicenseExpiry;

            const string sql = @"INSERT INTO Drivers (FirstName, LastName, CNIC, ContactNumber, LicenseExpiry, LicenseValidity)
                                 VALUES (@FirstName, @LastName, @CNIC, @ContactNumber, @LicenseExpiry, @LicenseValidity)";

            ExecuteNonQuery(sql,
                new SqlParameter("@FirstName", driver.FirstName ?? (object)DBNull.Value),
                new SqlParameter("@LastName", driver.LastName ?? (object)DBNull.Value),
                new SqlParameter("@CNIC", driver.CNIC ?? (object)DBNull.Value),
                new SqlParameter("@ContactNumber", driver.ContactNumber ?? (object)DBNull.Value),
                new SqlParameter("@LicenseExpiry", expiry),
                new SqlParameter("@LicenseValidity", driver.LicenseValidity ?? (object)DBNull.Value));
        }

        public void UpdateDriver(Driver driver)
        {
            var existing = context.Drivers.Find(driver.DriverID);
            if (existing != null)
            {
                existing.FirstName = driver.FirstName;
                existing.LastName = driver.LastName;
                existing.CNIC = driver.CNIC;
                existing.ContactNumber = driver.ContactNumber;
                existing.LicenseExpiry = driver.LicenseExpiry;
                // This SaveChanges call will fire the database trigger 'trg_AutoUpdateDriverLicenseValidity'
                context.SaveChanges();
            }
        }

        public void DeleteVehicle(int vehicleId)
        {
            // EF translates this to SQL DELETE, invoking the INSTEAD OF trigger
            var vehicle = context.Vehicles.Find(vehicleId);
            if (vehicle != null)
            {
                context.Vehicles.Remove(vehicle);
                SaveChangesWithConstraintHandling();
            }
        }

        public void DeleteDriver(int driverId)
        {
            var driver = context.Drivers.Find(driverId);
            if (driver != null)
            {
                context.Drivers.Remove(driver);
                SaveChangesWithConstraintHandling();
            }
        }

        public void DeleteTrip(int tripId)
        {
            var trip = context.Trips.Find(tripId);
            if (trip != null)
            {
                context.Trips.Remove(trip);
                SaveChangesWithConstraintHandling();
            }
        }

        public void DeleteMaintenanceRecord(int recordId)
        {
            var record = context.MaintenanceRecords.Find(recordId);
            if (record != null)
            {
                context.MaintenanceRecords.Remove(record);
                SaveChangesWithConstraintHandling();
            }
        }

        public void AddTrip(Trip trip)
        {
            if (trip.StartTime == default)
            {
                trip.StartTime = DateTime.Now;
            }
            if (trip.EndTime <= trip.StartTime)
            {
                trip.EndTime = trip.StartTime.AddMinutes(30);
            }
            if (trip.EndMileage <= trip.StartMileage)
            {
                trip.EndMileage = trip.StartMileage + 1;
            }

            // EF INSERT invokes the trg_UpdateMileage_AfterTrip trigger
            context.Trips.Add(trip);
            context.SaveChanges();
        }

        public void AddMaintenanceRecord(MaintenanceRecord record)
        {
            if (record.ServiceDate == default)
            {
                record.ServiceDate = DateTime.Now;
            }

            const string sql = @"INSERT INTO MaintenanceRecords (VehicleID, ServiceDate, ServiceType, Cost, Description)
                                 VALUES (@VehicleID, @ServiceDate, @ServiceType, @Cost, @Description)";

            ExecuteNonQuery(sql,
                new SqlParameter("@VehicleID", record.VehicleID),
                new SqlParameter("@ServiceDate", record.ServiceDate),
                new SqlParameter("@ServiceType", record.ServiceType ?? (object)DBNull.Value),
                new SqlParameter("@Cost", record.Cost),
                new SqlParameter("@Description", record.Description ?? (object)DBNull.Value));
        }

        // --- Phase 2 Feature Implementations (using raw SQL/EF for compliance) ---

        public void UpdateVehicleStatus(int vehicleId, string status)
        {
            const string sql = "sp_UpdateVehicleStatus";
            ExecuteRawSql(sql,
                new SqlParameter("@VehicleID", vehicleId),
                new SqlParameter("@NewStatus", status ?? string.Empty));
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

        public DataTable GetTablePreview(string objectName, int topRows = 100)
        {
            string safeName = SqlObjectCatalog.Normalize(objectName);
            int limit = topRows > 0 ? topRows : 100;
            string query = $"SELECT TOP {limit} * FROM dbo.[{safeName}]";
            return ExecuteRawSql(query);
        }

        public DataTable SearchVehicles(string searchTerm)
        {
            string trimmed = (searchTerm ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(trimmed))
            {
                return GetTablePreview("Vehicles");
            }

            if (int.TryParse(trimmed, out int vehicleId))
            {
                // Requirement: surface the stored procedure details when an ID is supplied
                return GetVehicleSummary(vehicleId);
            }

            const string sql = @"SELECT * FROM Vehicles
                                   WHERE LicensePlate LIKE @Term
                                      OR Make LIKE @Term
                                      OR Model LIKE @Term
                                      OR Status LIKE @Term";

            return ExecuteRawSql(sql, new SqlParameter("@Term", $"%{trimmed}%"));
        }

        public DataTable SearchDrivers(string searchTerm)
        {
            string trimmed = (searchTerm ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(trimmed))
            {
                return GetTablePreview("Drivers");
            }

            const string sql = @"SELECT * FROM Drivers
                                   WHERE FirstName LIKE @Term
                                      OR LastName LIKE @Term
                                      OR CNIC LIKE @Term
                                      OR ContactNumber LIKE @Term";

            return ExecuteRawSql(sql, new SqlParameter("@Term", $"%{trimmed}%"));
        }

        public DataTable SearchTrips(string searchTerm)
        {
            string trimmed = (searchTerm ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(trimmed))
            {
                return GetTablePreview("Trips");
            }

            if (!int.TryParse(trimmed, out int tripId))
            {
                return GetTablePreview("Trips");
            }

            const string sql = @"SELECT * FROM Trips
                                   WHERE TripID = @TripID";

            return ExecuteRawSql(sql, new SqlParameter("@TripID", tripId));
        }

        public DataTable SearchMaintenance(string searchTerm)
        {
            string trimmed = (searchTerm ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(trimmed))
            {
                return GetTablePreview("MaintenanceRecords");
            }

            const string sql = @"SELECT * FROM MaintenanceRecords
                                   WHERE CAST(RecordID AS NVARCHAR(20)) LIKE @Term
                                      OR CAST(VehicleID AS NVARCHAR(20)) LIKE @Term
                                      OR ServiceType LIKE @Term
                                      OR Description LIKE @Term";

            return ExecuteRawSql(sql, new SqlParameter("@Term", $"%{trimmed}%"));
        }

        public decimal CalculateFuelCost(int tripId, decimal fuelPricePerLiter)
        {
            const string sql = @"SELECT dbo.fn_CalculateFuelCost(
                                      (SELECT CAST(EndMileage - StartMileage AS DECIMAL(10,2))
                                       FROM Trips
                                       WHERE TripID = @TripID),
                                      @FuelPricePerLiter) AS FuelCost";

            var result = ExecuteRawSql(sql,
                new SqlParameter("@TripID", tripId),
                new SqlParameter("@FuelPricePerLiter", fuelPricePerLiter));

            if (result.Rows.Count == 0 || result.Rows[0]["FuelCost"] == DBNull.Value)
            {
                throw new InvalidOperationException("Trip not found.");
            }

            return Convert.ToDecimal(result.Rows[0]["FuelCost"]);
        }

        public void Dispose()
        {
            if (context != null)
            {
                context.Dispose();
                context = null;
            }
        }
    }
}