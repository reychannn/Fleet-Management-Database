using FleetApp.Models;
using System.Collections.Generic;
using System.Data;

namespace FleetApp.BLL
{
    public interface IFleetService
    {
        // CRUD Operations
        List<Vehicle> GetAllVehicles();
        Vehicle GetVehicleById(int id);
        void AddVehicle(Vehicle vehicle);
        // Deletion is governed by the trg_PreventDirectVehicleDeletion INSTEAD OF trigger
        void DeleteVehicle(int vehicleId);

        // Phase 2 Feature Integration
        void UpdateVehicleStatus(int vehicleId, string status); // Uses sp_UpdateVehicleStatus
        DataTable GetVehicleSummary(int vehicleId); // Uses sp_GetVehicleSummary
        DataTable GetVehicleHistory(int vehicleId); // Uses fn_GetVehicleHistory (TVF)
        DataTable GetHighMaintenanceVehicles(); // Uses vw_HighMaintenanceVehicles (View)
        void AddTrip(Trip trip); // Triggers trg_UpdateMileage_AfterTrip
        void AddMaintenanceRecord(MaintenanceRecord record); // Triggers trg_ValidateCost_AfterMaintenance
    }
}