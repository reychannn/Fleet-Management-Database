USE master;
GO

IF EXISTS (SELECT name FROM sys.databases WHERE name = 'FleetDB')
BEGIN
    ALTER DATABASE FleetDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE FleetDB;
END
GO

CREATE DATABASE FleetDB;
GO

USE FleetDB;
GO

DROP TABLE IF EXISTS MaintenanceRecords;
DROP TABLE IF EXISTS Trips;
DROP TABLE IF EXISTS Drivers;
DROP TABLE IF EXISTS Vehicles;

IF EXISTS (SELECT * FROM sys.partition_schemes WHERE name = 'ps_TripDate') 
    DROP PARTITION SCHEME ps_TripDate;

IF EXISTS (SELECT * FROM sys.partition_functions WHERE name = 'pf_TripDate') 
    DROP PARTITION FUNCTION pf_TripDate;
GO

CREATE PARTITION FUNCTION pf_TripDate (DATETIME)
AS RANGE RIGHT FOR VALUES 
('2022-01-01', '2023-01-01', '2024-01-01', '2025-01-01');
GO

CREATE PARTITION SCHEME ps_TripDate
AS PARTITION pf_TripDate
ALL TO ([PRIMARY]);
GO

DROP TABLE IF EXISTS Vehicles;
CREATE TABLE Vehicles (
    VehicleID INT IDENTITY(1,1) NOT NULL,
    LicensePlate VARCHAR(20) NOT NULL UNIQUE,
    Make VARCHAR(50) NOT NULL,
    Model VARCHAR(50) NOT NULL,
    Mileage DECIMAL(10,2) DEFAULT 0.00,
    Status VARCHAR(20) CHECK (Status IN ('Available', 'In-Use', 'Maintenance')) DEFAULT 'Available',
    CONSTRAINT PK_Vehicles PRIMARY KEY CLUSTERED (VehicleID)
);
GO

DROP TABLE IF EXISTS Drivers;
CREATE TABLE Drivers (
    DriverID INT IDENTITY(1,1) NOT NULL,
    FirstName VARCHAR(50) NOT NULL,
    LastName VARCHAR(50) NOT NULL,
    CNIC VARCHAR(20) NOT NULL UNIQUE,
    ContactNumber VARCHAR(20),
    LicenseExpiry DATE NOT NULL,
    LicenseValidity VARCHAR(50),
    CONSTRAINT PK_Drivers PRIMARY KEY CLUSTERED (DriverID)
);
GO

DROP TABLE IF EXISTS Trips;
CREATE TABLE Trips (
    TripID INT IDENTITY(1,1) NOT NULL,
    VehicleID INT NOT NULL,
    DriverID INT NOT NULL,
    StartTime DATETIME NOT NULL,
    EndTime DATETIME NOT NULL,
    StartMileage DECIMAL(10,2) NOT NULL,
    EndMileage DECIMAL(10,2) NOT NULL,
    Purpose VARCHAR(100),
    
    CONSTRAINT PK_Trips PRIMARY KEY CLUSTERED (TripID, StartTime), 
    CONSTRAINT CK_TripTime CHECK (EndTime > StartTime)
) ON ps_TripDate(StartTime);
GO

DROP TABLE IF EXISTS MaintenanceRecords;
CREATE TABLE MaintenanceRecords (
    RecordID INT IDENTITY(1,1) NOT NULL,
    VehicleID INT NOT NULL,
    ServiceDate DATETIME DEFAULT GETDATE(),
    ServiceType VARCHAR(100),
    Cost DECIMAL(10,2),
    Description VARCHAR(255),
    
    CONSTRAINT PK_MaintenanceRecords PRIMARY KEY CLUSTERED (RecordID, ServiceDate)
) ON ps_TripDate(ServiceDate);
GO

-- PRINT 'Starting Data Generation...';
-- SET NOCOUNT ON;

-- -- A. Generate 1,000 Vehicles
-- DECLARE @i INT = 1;
-- WHILE @i <= 1000
-- BEGIN
--     INSERT INTO Vehicles (LicensePlate, Make, Model, Mileage, Status)
--     VALUES (
--         'ABC-' + CAST(@i AS VARCHAR), 
--         CHOOSE(@i % 3 + 1, 'Toyota', 'Honda', 'Suzuki'),
--         CHOOSE(@i % 3 + 1, 'Corolla', 'Civic', 'Cultus'),
--         0, 'Available'
--     );
--     SET @i = @i + 1;
-- END
-- PRINT '1,000 Vehicles Generated';

-- --1000 drivers, 1000 vehicles, 10,000 trips, 2,000 maintenance records

-- SET @i = 1;
-- WHILE @i <= 1000
-- BEGIN
--     INSERT INTO Drivers (FirstName, LastName, CNIC, ContactNumber, LicenseExpiry, LicenseValidity)
--     VALUES (
--         'Driver' + CAST(@i AS VARCHAR), 
--         'User' + CAST(@i AS VARCHAR),
--         '42201-' + RIGHT('0000000' + CAST(@i AS VARCHAR), 7),
--         '0300-' + RIGHT('0000000' + CAST(@i AS VARCHAR), 7),
--         DATEADD(YEAR, 2, GETDATE()), 'Valid'
--     );
--     SET @i = @i + 1;
-- END
-- PRINT '1,000 Drivers Generated';

-- SET @i = 1;
-- DECLARE @BatchSize INT = 10000;
-- DECLARE @TotalRows INT = 1000000; 

-- WHILE @i <= @TotalRows
-- BEGIN
--     INSERT INTO Trips (VehicleID, DriverID, StartTime, EndTime, StartMileage, EndMileage, Purpose)
--     SELECT TOP (@BatchSize)
--         ((ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) + @i - 1) % 1000) + 1,
--         ((ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) + @i - 1) % 1000) + 1,
--         DATEADD(DAY, -((ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) + @i - 1) % 1400), GETDATE()),
--         DATEADD(MINUTE, ((ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) + @i - 1) % 105) + 15, 
--                 DATEADD(DAY, -((ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) + @i - 1) % 1400), GETDATE())),
--         0,
--         ((ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) + @i - 1) % 95) + 5,
--         'Business Logistics'
--     FROM sys.all_columns c1
--     CROSS JOIN sys.all_columns c2;
    
--     SET @i = @i + @BatchSize;
    
--     IF @i % 100000 = 0
--         PRINT CAST(@i AS VARCHAR) + ' Trips Generated...';
-- END
-- PRINT CAST(@TotalRows AS VARCHAR) + ' Trips Generated';

-- SET @i = 1;
-- WHILE @i <= 2000
-- BEGIN
--     INSERT INTO MaintenanceRecords (VehicleID, ServiceDate, ServiceType, Cost, Description)
--     VALUES (
--         (@i % 1000) + 1,
--         DATEADD(DAY, -(@i % 365), GETDATE()),
--         CHOOSE(@i % 2 + 1, 'Oil Change', 'Tire Rotation'),
--         (@i % 3000) + 2000,
--         'Routine Maintenance'
--     );
--     SET @i = @i + 1;
-- END
-- PRINT '2,000 Maintenance Records Generated';
-- GO

-- PRINT 'Updating Vehicle Mileage...';

-- UPDATE V
-- SET V.Mileage = ISNULL(T.TotalDistance, 0)
-- FROM Vehicles V
-- LEFT JOIN (
--     SELECT VehicleID, SUM(EndMileage - StartMileage) AS TotalDistance
--     FROM Trips
--     GROUP BY VehicleID
-- ) T ON V.VehicleID = T.VehicleID;

-- PRINT 'Vehicle Mileage Updated';
-- GO

-- SET NOCOUNT OFF;

ALTER TABLE Trips WITH CHECK ADD CONSTRAINT FK_Trips_Vehicles FOREIGN KEY(VehicleID) REFERENCES Vehicles (VehicleID);
ALTER TABLE Trips WITH CHECK ADD CONSTRAINT FK_Trips_Drivers FOREIGN KEY(DriverID) REFERENCES Drivers (DriverID);
ALTER TABLE MaintenanceRecords WITH CHECK ADD CONSTRAINT FK_Maint_Vehicles FOREIGN KEY(VehicleID) REFERENCES Vehicles (VehicleID);

DROP INDEX IF EXISTS IX_Vehicles_LicensePlate ON Vehicles;
CREATE NONCLUSTERED INDEX IX_Vehicles_LicensePlate ON Vehicles(LicensePlate);

DROP INDEX IF EXISTS IX_Trips_DriverID ON Trips;
CREATE NONCLUSTERED INDEX IX_Trips_DriverID ON Trips(DriverID) INCLUDE (StartTime, EndTime) ON ps_TripDate(StartTime);

PRINT 'Foreign Keys and Indexes have been successfully applied.';

GO
CREATE OR ALTER VIEW vw_FleetDashboard AS
SELECT v.LicensePlate, v.Make, v.Model, v.Status, v.Mileage
FROM Vehicles v;
GO

GO
CREATE OR ALTER VIEW vw_HighMaintenanceVehicles AS
WITH MaintenanceStats AS (
    SELECT VehicleID, SUM(Cost) as TotalSpent 
    FROM MaintenanceRecords 
    GROUP BY VehicleID
)
SELECT v.LicensePlate, v.Make, v.Model, ms.TotalSpent
FROM Vehicles v
INNER JOIN MaintenanceStats ms ON v.VehicleID = ms.VehicleID
WHERE ms.TotalSpent > 3000;
GO

GO
CREATE OR ALTER VIEW vw_DriverTripStats AS
WITH DriverCounts AS (
    SELECT DriverID, COUNT(*) as TripCount, SUM(EndMileage - StartMileage) as TotalDistance
    FROM Trips
    GROUP BY DriverID
)
SELECT d.FirstName, d.LastName, dc.TripCount, dc.TotalDistance
FROM Drivers d
JOIN DriverCounts dc ON d.DriverID = dc.DriverID;
GO

GO
CREATE OR ALTER FUNCTION fn_CalculateFuelCost (@Distance DECIMAL(10,2), @FuelPricePerLiter DECIMAL(10,2))
RETURNS DECIMAL(10,2)
AS
BEGIN
    RETURN (@Distance / 12.0) * @FuelPricePerLiter;
END
GO

GO
CREATE OR ALTER FUNCTION fn_GetVehicleHistory (@TargetVehicleID INT)
RETURNS TABLE
AS
RETURN (
    SELECT TOP 20 StartTime, EndTime, (EndMileage - StartMileage) AS TripDistance, Purpose
    FROM Trips
    WHERE VehicleID = @TargetVehicleID
    ORDER BY StartTime DESC
);
GO

GO
CREATE OR ALTER PROCEDURE sp_UpdateVehicleStatus
    @VehicleID INT, 
    @NewStatus VARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1 FROM Vehicles WHERE VehicleID = @VehicleID)
    BEGIN
        UPDATE Vehicles SET Status = @NewStatus WHERE VehicleID = @VehicleID;
        PRINT 'Success: Vehicle status updated.';
    END
    ELSE PRINT 'Error: Vehicle not found.';
END
GO

GO
CREATE OR ALTER PROCEDURE sp_GetVehicleSummary
    @VehicleID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT 
        v.VehicleID,
        v.LicensePlate,
        v.Make,
        v.Model,
        v.Mileage,
        v.Status,
        COUNT(t.TripID) AS TotalTrips
    FROM Vehicles v
    LEFT JOIN Trips t ON v.VehicleID = t.VehicleID
    WHERE v.VehicleID = @VehicleID
    GROUP BY v.VehicleID, v.LicensePlate, v.Make, v.Model, v.Mileage, v.Status;
END
GO

GO
CREATE OR ALTER TRIGGER trg_UpdateMileage_AfterTrip
ON Trips
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE V
    SET V.Mileage = V.Mileage + (I.EndMileage - I.StartMileage)
    FROM Vehicles V
    INNER JOIN Inserted I ON V.VehicleID = I.VehicleID;
END
GO

GO
CREATE OR ALTER TRIGGER trg_ValidateCost_AfterMaintenance
ON MaintenanceRecords
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1 FROM Inserted WHERE Cost < 0)
    BEGIN
        RAISERROR ('Error: Maintenance cost cannot be negative.', 16, 1);
        ROLLBACK TRANSACTION;
    END
END
GO

GO
CREATE OR ALTER TRIGGER trg_PreventDirectVehicleDeletion
ON Vehicles
INSTEAD OF DELETE
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Check if vehicle has trips or maintenance records
    IF EXISTS (
        SELECT 1 FROM Deleted d
        INNER JOIN Trips t ON d.VehicleID = t.VehicleID
    )
    BEGIN
        RAISERROR ('Cannot delete vehicle with existing trips. Archive trips first.', 16, 1);
        RETURN;
    END
    
    IF EXISTS (
        SELECT 1 FROM Deleted d
        INNER JOIN MaintenanceRecords m ON d.VehicleID = m.VehicleID
    )
    BEGIN
        RAISERROR ('Cannot delete vehicle with maintenance records. Archive records first.', 16, 1);
        RETURN;
    END
    
    DELETE FROM Vehicles WHERE VehicleID IN (SELECT VehicleID FROM Deleted);
    PRINT 'Vehicle(s) deleted successfully.';
END
GO

GO
CREATE OR ALTER TRIGGER trg_AutoUpdateDriverLicenseValidity
ON Drivers
INSTEAD OF INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    
    -- For INSERT
    IF NOT EXISTS (SELECT 1 FROM Deleted)
    BEGIN
        INSERT INTO Drivers (FirstName, LastName, CNIC, ContactNumber, LicenseExpiry, LicenseValidity)
        SELECT 
            FirstName, 
            LastName, 
            CNIC, 
            ContactNumber, 
            LicenseExpiry,
            CASE 
                WHEN LicenseExpiry < GETDATE() THEN 'Expired'
                WHEN LicenseExpiry < DATEADD(MONTH, 1, GETDATE()) THEN 'Expiring Soon'
                ELSE 'Valid'
            END AS LicenseValidity
        FROM Inserted;
        
        PRINT 'Driver(s) inserted with auto-calculated license validity.';
    END
    -- For UPDATE
    ELSE
    BEGIN
        UPDATE d
        SET 
            d.FirstName = i.FirstName,
            d.LastName = i.LastName,
            d.CNIC = i.CNIC,
            d.ContactNumber = i.ContactNumber,
            d.LicenseExpiry = i.LicenseExpiry,
            d.LicenseValidity = CASE 
                WHEN i.LicenseExpiry < GETDATE() THEN 'Expired'
                WHEN i.LicenseExpiry < DATEADD(MONTH, 1, GETDATE()) THEN 'Expiring Soon'
                ELSE 'Valid'
            END
        FROM Drivers d
        INNER JOIN Inserted i ON d.DriverID = i.DriverID;
        
        PRINT 'Driver(s) updated with auto-calculated license validity.';
    END
END
GO


PRINT '=== VEHICLES TABLE (First 10 rows) ===';
SELECT TOP 10 * FROM Vehicles ORDER BY VehicleID;
PRINT '';

PRINT '=== DRIVERS TABLE (First 10 rows) ===';
SELECT TOP 10 * FROM Drivers ORDER BY DriverID;
PRINT '';

PRINT '=== TRIPS TABLE (First 10 rows) ===';
SELECT TOP 10 * FROM Trips ORDER BY TripID, StartTime;
PRINT '';

PRINT '=== MAINTENANCE RECORDS TABLE (First 10 rows) ===';
SELECT TOP 10 * FROM MaintenanceRecords ORDER BY RecordID, ServiceDate;
PRINT '';



PRINT '=== Table Record Counts ===';
SELECT 'Vehicles' AS TableName, COUNT(*) AS RecordCount FROM Vehicles
UNION ALL
SELECT 'Drivers', COUNT(*) FROM Drivers
UNION ALL
SELECT 'Trips', COUNT(*) FROM Trips
UNION ALL
SELECT 'MaintenanceRecords', COUNT(*) FROM MaintenanceRecords;
PRINT '';