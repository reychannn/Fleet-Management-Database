# Fleet Management Database Project

This guide explains how to set up the database environment and run the Fleet Management application.

## Prerequisites & Dependencies

Before starting, ensure you have the following installed on your machine:

*   **Docker Desktop**: Required to run the SQL Server container.
*   **Visual Studio 2022** (or newer): Required to build and run the .NET application.
    *   Workload required: *.NET Desktop Development*.
*   **PowerShell**: To run the setup scripts.

---

## Step 1: Start the Database

This project uses a Docker container to host Microsoft SQL Server. You must start the database before running the application.

1.  Open **PowerShell** or your terminal.
2.  Navigate to the **root directory** of the project (where the `scripts` folder is located).
    *   *Example:* `C:\Users\YourName\Desktop\Fleet-Management-Database\FleetManagementDatabase`
3.  Run the startup script:

    ```powershell
    .\scripts\start-sql.ps1
    ```

    **What this does:**
    *   Starts a Docker container named `fleet-sql`.
    *   Sets up the `FleetDB` database.
    *   Creates a user (`app_user`) with the password `AppPassword123`.
    *   Executes the schema script (`Database\group56_p2.sql`) to create tables and seed data.

    > **Note:** The SQL data generation inside `Database\group8_p2.sql` might be turned off (commented out) for testing purposes. If you want to see pre-populated records upon launch, please open that file and uncomment the data generation lines before running the start-sql script.

---

## Step 2: Run the Application

Once the database is running (the script will say "FleetDB is ready"), you can launch the UI.

1.  Open **Visual Studio**.
2.  Select **Open a project or solution**.
3.  Navigate to the **root directory** and select the solution file:
    *   `FleetManagementDatabase.sln` (or `.slnx` if using the preview format).
4.  In the Solution Explorer, ensure **FleetApp.UI** is set as the Startup Project (it should be bold).
5.  Press **F5** or click the **Start** button to build and run the application.