using FleetApp.Models;
using System.Collections.Generic;
using System.Data;

namespace FleetApp.DAL
{
    public interface IFleetService
    {
        // CRUD Operations
        List<Vehicle> GetAllVehicles();
        Vehicle GetVehicleById(int id);
        void AddVehicle(Vehicle vehicle);
        void AddDriver(Driver driver);
        // Deletion is governed by the trg_PreventDirectVehicleDeletion INSTEAD OF trigger
        void DeleteVehicle(int vehicleId);
        void DeleteDriver(int driverId);
        void DeleteTrip(int tripId);
        void DeleteMaintenanceRecord(int recordId);

        // Phase 2 Feature Integration
        void UpdateVehicleStatus(int vehicleId, string status); // Uses sp_UpdateVehicleStatus
        DataTable GetVehicleSummary(int vehicleId); // Uses sp_GetVehicleSummary
        DataTable GetVehicleHistory(int vehicleId); // Uses fn_GetVehicleHistory (TVF)
        DataTable GetHighMaintenanceVehicles(); // Uses vw_HighMaintenanceVehicles (View)
        DataTable GetTablePreview(string objectName, int topRows = 100); // Direct table/view preview
        void AddTrip(Trip trip); // Triggers trg_UpdateMileage_AfterTrip
        void AddMaintenanceRecord(MaintenanceRecord record); // Triggers trg_ValidateCost_AfterMaintenance
        decimal CalculateFuelCost(int tripId, decimal fuelPricePerLiter);
        DataTable SearchVehicles(string searchTerm); // Vehicles search leverages sp_GetVehicleSummary when possible
        DataTable SearchDrivers(string searchTerm);
        DataTable SearchTrips(string searchTerm);
        DataTable SearchMaintenance(string searchTerm);
    }
}