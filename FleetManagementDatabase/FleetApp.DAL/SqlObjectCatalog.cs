using System;
using System.Collections.Generic;

namespace FleetApp.DAL
{
    internal static class SqlObjectCatalog
    {
        private static readonly HashSet<string> AllowedObjects = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Vehicles",
            "Drivers",
            "Trips",
            "MaintenanceRecords",
            "vw_FleetDashboard",
            "vw_HighMaintenanceVehicles",
            "vw_DriverTripStats"
        };

        public static string Normalize(string objectName)
        {
            if (string.IsNullOrWhiteSpace(objectName))
            {
                throw new ArgumentException("Object name must be provided.", nameof(objectName));
            }

            string trimmed = objectName.Trim();
            if (!AllowedObjects.Contains(trimmed))
            {
                throw new ArgumentException($"Object '{objectName}' is not recognized. Allowed values: {string.Join(", ", AllowedObjects)}");
            }

            return trimmed;
        }
    }
}
