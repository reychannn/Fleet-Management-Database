using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FleetApp.Models
{
    [Table("Vehicles")]
    public class Vehicle
    {
        [Key]
        public int VehicleID { get; set; }
        public string LicensePlate { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public decimal Mileage { get; set; }
        public string Status { get; set; }
    }

    [Table("Drivers")]
    public class Driver
    {
        [Key]
        public int DriverID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string CNIC { get; set; }
        public DateTime LicenseExpiry { get; set; }
        // LicenseValidity is managed by the trg_AutoUpdateDriverLicenseValidity trigger
        public string LicenseValidity { get; set; }
    }

    [Table("Trips")]
    public class Trip
    {
        [Key]
        public int TripID { get; set; }
        public int VehicleID { get; set; }
        public int DriverID { get; set; }
        public DateTime StartTime { get; set; }
        public decimal EndMileage { get; set; }
        // Note: Inserting a Trip invokes the trg_UpdateMileage_AfterTrip trigger
    }

    [Table("MaintenanceRecords")]
    public class MaintenanceRecord
    {
        [Key]
        public int RecordID { get; set; }
        public int VehicleID { get; set; }
        public string ServiceType { get; set; }
        // Note: Cost is validated by the trg_ValidateCost_AfterMaintenance trigger
        public decimal Cost { get; set; }
        public string Description { get; set; }
    }
}