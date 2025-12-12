using FleetApp.Models;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System;

namespace FleetApp.DAL
{
    public class FleetServiceSP : IFleetService
    {
        // Use EF DbContext to obtain connection string to avoid needing System.Configuration in project references
        private readonly string connectionString = new FleetDBContext().Database.Connection.ConnectionString;

        // Utility method to execute raw SQL and return a DataTable
        private DataTable ExecuteRawSql(string sql, params SqlParameter[] parameters)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(sql, conn);
                // Simple check: if it doesn't have spaces and doesn't start with SELECT/INSERT/DELETE/UPDATE, assume it's an SP name
                bool isStoredProc = !sql.Contains(" ") && !sql.Trim().ToUpper().StartsWith("SELECT");
                cmd.CommandType = isStoredProc ? CommandType.StoredProcedure : CommandType.Text;
                
                if (parameters != null && parameters.Length > 0)
                {
                    cmd.Parameters.AddRange(parameters);
                }

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
            }
            return dt;
        }

        // --- Complete IFleetService Implementations ---

        public List<Vehicle> GetAllVehicles()
        {
            DataTable dt = ExecuteRawSql("SELECT * FROM Vehicles");
            List<Vehicle> vehicles = new List<Vehicle>();

            foreach (DataRow row in dt.Rows)
            {
                vehicles.Add(new Vehicle
                {
                    VehicleID = Convert.ToInt32(row["VehicleID"]),
                    Make = row["Make"].ToString(),
                    Model = row["Model"].ToString(),
                    Mileage = Convert.ToDecimal(row["Mileage"]),
                    LicensePlate = row["LicensePlate"].ToString(),
                    Status = row["Status"].ToString()
                });
            }
            return vehicles;
        }

        public Vehicle GetVehicleById(int id)
        {
            DataTable dt = ExecuteRawSql("SELECT * FROM Vehicles WHERE VehicleID = @id", new SqlParameter("@id", id));
            
            if (dt.Rows.Count == 0) return null;

            DataRow row = dt.Rows[0];
            return new Vehicle
            {
                VehicleID = Convert.ToInt32(row["VehicleID"]),
                Make = row["Make"].ToString(),
                Model = row["Model"].ToString(),
                Mileage = Convert.ToDecimal(row["Mileage"]),
                LicensePlate = row["LicensePlate"].ToString(),
                Status = row["Status"].ToString()
            };
        }

        public void AddVehicle(Vehicle vehicle)
        {
            string sql = @"INSERT INTO Vehicles (Make, Model, Mileage, LicensePlate, Status) 
                           VALUES (@Make, @Model, @Mileage, @LicensePlate, @Status)";

            ExecuteRawSql(sql,
                new SqlParameter("@Make", vehicle.Make),
                new SqlParameter("@Model", vehicle.Model),
                new SqlParameter("@Mileage", vehicle.Mileage),
                new SqlParameter("@LicensePlate", vehicle.LicensePlate),
                new SqlParameter("@Status", vehicle.Status)
            );
        }

        public void AddDriver(Driver driver)
        {
            DateTime licenseExpiry = driver.LicenseExpiry == default ? DateTime.Now.AddYears(1) : driver.LicenseExpiry;

            string sql = @"INSERT INTO Drivers (FirstName, LastName, CNIC, ContactNumber, LicenseExpiry, LicenseValidity) 
                           VALUES (@FirstName, @LastName, @CNIC, @ContactNumber, @LicenseExpiry, @LicenseValidity)";

            ExecuteRawSql(sql,
                new SqlParameter("@FirstName", driver.FirstName),
                new SqlParameter("@LastName", driver.LastName),
                new SqlParameter("@CNIC", driver.CNIC),
                new SqlParameter("@ContactNumber", driver.ContactNumber),
                new SqlParameter("@LicenseExpiry", licenseExpiry),
                new SqlParameter("@LicenseValidity", (object)driver.LicenseValidity ?? DBNull.Value)
            );
        }

        public void UpdateDriver(Driver driver)
        {
            string sql = @"UPDATE Drivers 
                   SET FirstName = @fn, LastName = @ln, CNIC = @cnic, 
                       ContactNumber = @contact, LicenseExpiry = @expiry
                   WHERE DriverID = @id";

            ExecuteRawSql(sql,
                new SqlParameter("@id", driver.DriverID),
                new SqlParameter("@fn", driver.FirstName),
                new SqlParameter("@ln", driver.LastName),
                new SqlParameter("@cnic", driver.CNIC),
                new SqlParameter("@contact", driver.ContactNumber),
                new SqlParameter("@expiry", driver.LicenseExpiry)
            );
        }

        public void DeleteVehicle(int vehicleId)
        {
            try
            {
                // The INSTEAD OF trigger trg_PreventDirectVehicleDeletion handles the actual logic
                ExecuteRawSql("DELETE FROM Vehicles WHERE VehicleID = @id", new SqlParameter("@id", vehicleId));
            }
            catch (SqlException ex)
            {
                throw new InvalidOperationException(ex.Message, ex);
            }
        }

        public void DeleteDriver(int driverId)
        {
            ExecuteRawSql("DELETE FROM Drivers WHERE DriverID = @id", new SqlParameter("@id", driverId));
        }

        public void DeleteTrip(int tripId)
        {
            ExecuteRawSql("DELETE FROM Trips WHERE TripID = @id", new SqlParameter("@id", tripId));
        }

        public void DeleteMaintenanceRecord(int recordId)
        {
            ExecuteRawSql("DELETE FROM MaintenanceRecords WHERE RecordID = @id", new SqlParameter("@id", recordId));
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

        public DataTable GetTablePreview(string objectName, int topRows = 100)
        {
            string safeName = SqlObjectCatalog.Normalize(objectName);
            int limit = topRows > 0 ? topRows : 100;
            string query = $"SELECT TOP {limit} * FROM dbo.[{safeName}]";
            return ExecuteRawSql(query);
        }

        public void AddTrip(Trip trip)
        {
            DateTime startTime = trip.StartTime == default ? DateTime.Now : trip.StartTime;
            DateTime endTime = trip.EndTime <= startTime ? startTime.AddMinutes(30) : trip.EndTime;
            decimal startMileage = trip.StartMileage;
            decimal endMileage = trip.EndMileage <= startMileage ? startMileage + 1 : trip.EndMileage;

            string sql = @"INSERT INTO Trips (VehicleID, DriverID, StartTime, EndTime, StartMileage, EndMileage, Purpose) 
                           VALUES (@VehicleID, @DriverID, @StartTime, @EndTime, @StartMileage, @EndMileage, @Purpose)";

            ExecuteRawSql(sql,
                new SqlParameter("@VehicleID", trip.VehicleID),
                new SqlParameter("@DriverID", trip.DriverID),
                new SqlParameter("@StartTime", startTime),
                new SqlParameter("@EndTime", endTime),
                new SqlParameter("@StartMileage", startMileage),
                new SqlParameter("@EndMileage", endMileage),
                new SqlParameter("@Purpose", (object)trip.Purpose ?? DBNull.Value)
            );
        }

        public void AddMaintenanceRecord(MaintenanceRecord record)
        {
            DateTime serviceDate = record.ServiceDate == default ? DateTime.Now : record.ServiceDate;

            string sql = @"INSERT INTO MaintenanceRecords (VehicleID, ServiceDate, ServiceType, Cost, Description) 
                           VALUES (@VehicleID, @ServiceDate, @ServiceType, @Cost, @Description)";

            ExecuteRawSql(sql,
                new SqlParameter("@VehicleID", record.VehicleID),
                new SqlParameter("@ServiceDate", serviceDate),
                new SqlParameter("@ServiceType", record.ServiceType),
                new SqlParameter("@Cost", record.Cost),
                new SqlParameter("@Description", (object)record.Description ?? DBNull.Value)
            );
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

        public DataTable SearchVehicles(string searchTerm)
        {
            string trimmed = (searchTerm ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(trimmed))
            {
                return GetTablePreview("Vehicles");
            }

            if (int.TryParse(trimmed, out int vehicleId))
            {
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
    }
}