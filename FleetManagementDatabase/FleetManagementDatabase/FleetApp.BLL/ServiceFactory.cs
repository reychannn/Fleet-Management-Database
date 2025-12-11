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

        public static IFleetService GetFleetService(ServiceType type)
        {
            // [Image of Factory Design Pattern diagram]

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