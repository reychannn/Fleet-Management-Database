using FleetApp.DAL;
using System;

namespace FleetApp.BLL
{
    public static class ServiceFactory
    {
        public enum ServiceType
        {
            LINQ,
            StoredProcedure
        }

        // Return as object to avoid forcing downstream projects to reference DAL directly
        public static object GetFleetService(ServiceType type)
        {
            switch (type)
            {
                case ServiceType.StoredProcedure:
                    return new FleetServiceSP();
                case ServiceType.LINQ:
                    return new FleetServiceLINQ();
                default:
                    throw new ArgumentException("Invalid service type selected.");
            }
        }
    }
}