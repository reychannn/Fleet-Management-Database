using FleetApp.BLL;
using System;
using System.Data;
using System.Drawing;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace FleetApp.UI
{
    public partial class Form1 : Form
    {
        private dynamic currentService;
        private readonly string[] previewTargets = new[]
        {
            "Vehicles",
            "Drivers",
            "Trips",
            "MaintenanceRecords",
            "vw_FleetDashboard",
            "vw_HighMaintenanceVehicles",
            "vw_DriverTripStats"
        };

        public Form1()
        {
            InitializeComponent();
            cmbObject.DataSource = previewTargets;
            lblStatus.Text = "Select a table or view and click Load.";
            rbLinq.Checked = true; // default to LINQ/EF implementation
        }

        // Method to instantiate the correct BLL based on radio button selection (Factory Pattern)
        private void SetService()
        {
            try
            {
                if (rbSP.Checked)
                {
                    currentService = ServiceFactory.GetFleetService(ServiceFactory.ServiceType.StoredProcedure);
                    this.Text = "Fleet Management (Mode: Stored Procedures)";
                }
                else
                {
                    currentService = ServiceFactory.GetFleetService(ServiceFactory.ServiceType.LINQ);
                    this.Text = "Fleet Management (Mode: LINQ / EF)";
                }

                LoadSelectedObject();
            }
            catch (Exception ex)
            {
                var root = ex.InnerException ?? ex;
                lblStatus.Text = "Failed to initialize service.";
                MessageBox.Show($"Error initializing service:\n{root.Message}\n\nStack Trace:\n{root.StackTrace}", 
                    "Service Initialization Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void rb_CheckedChanged(object sender, EventArgs e)
        {
            SetService();
        }

        private void cmbObject_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadSelectedObject();
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            LoadSelectedObject();
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            EnsureService();

            if (cmbObject.SelectedItem == null)
            {
                MessageBox.Show("Select a table or view before searching.", "Search", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string searchTerm = txtSearch.Text?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                LoadSelectedObject();
                return;
            }

            string target = cmbObject.SelectedItem.ToString();
            try
            {
                DataTable data = SearchSelectedObject(target, searchTerm);
                gridResults.DataSource = data;
                lblStatus.Text = $"{target} search — {data.Rows.Count} row(s)";
            }
            catch (NotSupportedException ex)
            {
                lblStatus.Text = ex.Message;
                MessageBox.Show(ex.Message, "Search Not Available", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                gridResults.DataSource = null;
                lblStatus.Text = "Search failed.";
                var inner = ex.InnerException != null ? $"\n\nInner Exception:\n{ex.InnerException.Message}" : string.Empty;
                MessageBox.Show($"Error: {ex.Message}{inner}", "Search Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnAddVehicle_Click(object sender, EventArgs e)
        {
            EnsureService();

            using (var dialog = new AddVehicleForm())
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    try
                    {
                        var vehicle = CreateVehicleInstance(dialog.CreatedVehicle);
                        InvokeServiceMethod("AddVehicle", vehicle);
                        lblStatus.Text = "Vehicle added successfully.";

                        if (cmbObject.SelectedItem?.ToString() == "Vehicles")
                        {
                            LoadSelectedObject();
                        }
                    }
                    catch (Exception ex)
                    {
                        var root = (ex as TargetInvocationException)?.InnerException ?? ex;
                        var innerMsg = root.InnerException != null ? $"\n\nInner Exception:\n{root.InnerException.Message}" : "";
                        lblStatus.Text = "Failed to add vehicle.";
                        MessageBox.Show($"Error: {root.Message}{innerMsg}", "Add Vehicle Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void btnAddDriver_Click(object sender, EventArgs e)
        {
            EnsureService();

            using (var dialog = new AddDriverForm())
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    try
                    {
                        var driver = CreateDriverInstance(dialog.CreatedDriver);
                        InvokeServiceMethod("AddDriver", driver);
                        lblStatus.Text = "Driver added successfully.";

                        if (cmbObject.SelectedItem?.ToString() == "Drivers")
                        {
                            LoadSelectedObject();
                        }
                    }
                    catch (Exception ex)
                    {
                        var root = (ex as TargetInvocationException)?.InnerException ?? ex;
                        var innerMsg = root.InnerException != null ? $"\n\nInner Exception:\n{root.InnerException.Message}" : "";
                        lblStatus.Text = "Failed to add driver.";
                        MessageBox.Show($"Error: {root.Message}{innerMsg}", "Add Driver Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void btnAddTrip_Click(object sender, EventArgs e)
        {
            EnsureService();

            using (var dialog = new AddTripForm())
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    try
                    {
                        var trip = CreateTripInstance(dialog.CreatedTrip);
                        InvokeServiceMethod("AddTrip", trip);
                        lblStatus.Text = "Trip added successfully.";

                        if (cmbObject.SelectedItem?.ToString() == "Trips")
                        {
                            LoadSelectedObject();
                        }
                    }
                    catch (Exception ex)
                    {
                        var root = (ex as TargetInvocationException)?.InnerException ?? ex;
                        var innerMsg = root.InnerException != null ? $"\n\nInner Exception:\n{root.InnerException.Message}" : "";
                        lblStatus.Text = "Failed to add trip.";
                        MessageBox.Show($"Error: {root.Message}{innerMsg}", "Add Trip Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void btnAddMaintenance_Click(object sender, EventArgs e)
        {
            EnsureService();

            using (var dialog = new AddMaintenanceForm())
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    try
                    {
                        var record = CreateMaintenanceRecordInstance(dialog.CreatedRecord);
                        InvokeServiceMethod("AddMaintenanceRecord", record);
                        lblStatus.Text = "Maintenance record added successfully.";

                        if (cmbObject.SelectedItem?.ToString() == "MaintenanceRecords" ||
                            cmbObject.SelectedItem?.ToString() == "vw_HighMaintenanceVehicles")
                        {
                            LoadSelectedObject();
                        }
                    }
                    catch (Exception ex)
                    {
                        var root = (ex as TargetInvocationException)?.InnerException ?? ex;
                        var innerMsg = root.InnerException != null ? $"\n\nInner Exception:\n{root.InnerException.Message}" : string.Empty;
                        lblStatus.Text = "Failed to add maintenance record.";
                        MessageBox.Show($"Error: {root.Message}{innerMsg}", "Add Maintenance Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void btnVehicleHistory_Click(object sender, EventArgs e)
        {
            EnsureService();

            if (!string.Equals(cmbObject.SelectedItem?.ToString(), "Vehicles", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("Select a vehicle row in the Vehicles view first.", "Vehicle History", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (!TryGetSelectedId("Vehicles", out int vehicleId, out string error))
            {
                lblStatus.Text = error;
                MessageBox.Show(error, "Vehicle History", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var data = (DataTable)InvokeServiceMethod("GetVehicleHistory", vehicleId);
                if (data == null || data.Rows.Count == 0)
                {
                    MessageBox.Show("No trip history found for this vehicle.", "Vehicle History", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                using (var dialog = new VehicleHistoryForm(vehicleId, data))
                {
                    dialog.ShowDialog(this);
                }

                lblStatus.Text = $"Vehicle #{vehicleId} history displayed.";
            }
            catch (Exception ex)
            {
                string friendly = GetInnermostMessage(ex);
                lblStatus.Text = friendly;
                MessageBox.Show(friendly, "Vehicle History Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnUpdateStatus_Click(object sender, EventArgs e)
        {
            EnsureService();

            if (!string.Equals(cmbObject.SelectedItem?.ToString(), "Vehicles", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("Switch to the Vehicles view and select a vehicle to update its status.", "Update Status", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (gridResults.CurrentRow == null)
            {
                MessageBox.Show("Select a vehicle row first.", "Update Status", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (!TryGetSelectedId("Vehicles", out int vehicleId, out string error))
            {
                lblStatus.Text = error;
                MessageBox.Show(error, "Update Status", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string currentStatus = gridResults.CurrentRow.Cells["Status"]?.Value?.ToString() ?? "Available";

            using (var dialog = new UpdateVehicleStatusForm(currentStatus))
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    try
                    {
                        InvokeServiceMethod("UpdateVehicleStatus", vehicleId, dialog.SelectedStatus);
                        lblStatus.Text = $"Vehicle #{vehicleId} status updated to {dialog.SelectedStatus}.";
                        LoadSelectedObject();
                    }
                    catch (Exception ex)
                    {
                        string friendly = GetInnermostMessage(ex);
                        lblStatus.Text = friendly;
                        MessageBox.Show(friendly, "Update Status Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
        }

        private void btnFuelCost_Click(object sender, EventArgs e)
        {
            EnsureService();

            if (!string.Equals(cmbObject.SelectedItem?.ToString(), "Trips", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("Select a trip row in the Trips view first.", "Fuel Cost", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (!TryGetSelectedId("Trips", out int tripId, out string error))
            {
                lblStatus.Text = error;
                MessageBox.Show(error, "Fuel Cost", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (var dialog = new FuelCostInputForm())
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    try
                    {
                        var result = InvokeServiceMethod("CalculateFuelCost", tripId, dialog.FuelPricePerLiter);
                        decimal cost = result is decimal d ? d : Convert.ToDecimal(result ?? 0);
                        lblStatus.Text = $"Trip #{tripId} fuel cost: {cost:C}";
                        MessageBox.Show($"Estimated fuel cost: {cost:C}", "Fuel Cost", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        string friendly = GetInnermostMessage(ex);
                        lblStatus.Text = friendly;
                        MessageBox.Show(friendly, "Fuel Cost Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
        }

        private void btnUpdateDriver_Click(object sender, EventArgs e)
        {
            EnsureService();

            if (!string.Equals(cmbObject.SelectedItem?.ToString(), "Drivers", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("Select the Drivers table to update records.", "Update Driver", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (!TryGetSelectedId("Drivers", out int driverId, out string error))
            {
                lblStatus.Text = error;
                MessageBox.Show(error, "Update Driver", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Get current values from the selected row
            var row = gridResults.CurrentRow;
            var currentData = new DriverInput
            {
                FirstName = row.Cells["FirstName"].Value?.ToString(),
                LastName = row.Cells["LastName"].Value?.ToString(),
                CNIC = row.Cells["CNIC"].Value?.ToString(),
                ContactNumber = row.Cells["ContactNumber"].Value?.ToString(),
                LicenseExpiry = row.Cells["LicenseExpiry"].Value != DBNull.Value
                                ? Convert.ToDateTime(row.Cells["LicenseExpiry"].Value)
                                : DateTime.Now.AddYears(1)
            };

            using (var dialog = new UpdateDriverForm(currentData))
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    try
                    {
                        var input = dialog.CreatedDriver; // Reuse the input class

                        // Create a driver object manually or via helper
                        var driverType = currentService.GetType().Assembly.GetType("FleetApp.Models.Driver");
                        var driver = Activator.CreateInstance(driverType);

                        driverType.GetProperty("DriverID")?.SetValue(driver, driverId);
                        driverType.GetProperty("FirstName")?.SetValue(driver, input.FirstName);
                        driverType.GetProperty("LastName")?.SetValue(driver, input.LastName);
                        driverType.GetProperty("CNIC")?.SetValue(driver, input.CNIC);
                        driverType.GetProperty("ContactNumber")?.SetValue(driver, input.ContactNumber);
                        driverType.GetProperty("LicenseExpiry")?.SetValue(driver, input.LicenseExpiry);

                        InvokeServiceMethod("UpdateDriver", driver);

                        lblStatus.Text = $"Driver #{driverId} updated successfully.";
                        LoadSelectedObject();
                    }
                    catch (Exception ex)
                    {
                        string friendly = GetInnermostMessage(ex);
                        lblStatus.Text = friendly;
                        MessageBox.Show(friendly, "Update Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void btnDeleteSelected_Click(object sender, EventArgs e)
        {
            EnsureService();

            if (cmbObject.SelectedItem == null)
            {
                MessageBox.Show("Select a table before deleting records.", "Delete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (gridResults.CurrentRow == null)
            {
                MessageBox.Show("Select a row to delete.", "Delete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string target = cmbObject.SelectedItem.ToString();
            try
            {
                if (!TryGetSelectedId(target, out int recordId, out string error))
                {
                    lblStatus.Text = error;
                    MessageBox.Show(error, "Delete", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var confirm = MessageBox.Show($"Are you sure you want to delete {target} record #{recordId}?",
                    "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (confirm != DialogResult.Yes)
                {
                    return;
                }

                DeleteSelectedObject(target, recordId);
                lblStatus.Text = $"{target} record #{recordId} deleted.";
                LoadSelectedObject();
            }
            catch (NotSupportedException ex)
            {
                lblStatus.Text = ex.Message;
                MessageBox.Show(ex.Message, "Delete Not Available", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                string friendly = GetInnermostMessage(ex);
                lblStatus.Text = friendly;
                MessageBox.Show(friendly, "Delete Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void LoadSelectedObject()
        {
            if (currentService == null || cmbObject.SelectedItem == null)
            {
                return;
            }

            try
            {
                string target = cmbObject.SelectedItem.ToString();
                DataTable data = currentService.GetTablePreview(target);
                gridResults.DataSource = data;
                lblStatus.Text = $"{target} — {data.Rows.Count} row(s)";
            }
            catch (Exception ex)
            {
                gridResults.DataSource = null;
                lblStatus.Text = "Failed to load data.";
                var inner = ex.InnerException != null ? $"\n\nInner Exception:\n{ex.InnerException.Message}" : "";
                MessageBox.Show($"Error: {ex.Message}{inner}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private DataTable SearchSelectedObject(string target, string searchTerm)
        {
            switch (target)
            {
                case "Vehicles":
                    return (DataTable)InvokeServiceMethod("SearchVehicles", searchTerm);
                case "Drivers":
                    return (DataTable)InvokeServiceMethod("SearchDrivers", searchTerm);
                case "Trips":
                    return (DataTable)InvokeServiceMethod("SearchTrips", searchTerm);
                case "MaintenanceRecords":
                    return (DataTable)InvokeServiceMethod("SearchMaintenance", searchTerm);
                default:
                    throw new NotSupportedException($"Search is not available for {target}.");
            }
        }

        private void DeleteSelectedObject(string target, int recordId)
        {
            string methodName = GetDeleteMethodName(target);
            InvokeServiceMethod(methodName, recordId);
        }

        private bool TryGetSelectedId(string target, out int recordId, out string errorMessage)
        {
            recordId = 0;
            errorMessage = null;

            string keyColumn = GetKeyColumn(target);
            if (keyColumn == null)
            {
                errorMessage = $"Deletion is not available for {target}.";
                return false;
            }

            var row = gridResults.CurrentRow;
            if (row == null)
            {
                errorMessage = "Select a row to delete.";
                return false;
            }

            var cell = row.Cells[keyColumn];
            if (cell?.Value == null || cell.Value == DBNull.Value || !int.TryParse(cell.Value.ToString(), out recordId))
            {
                errorMessage = $"Unable to determine {keyColumn} for the selected row.";
                return false;
            }

            return true;
        }

        private string GetKeyColumn(string target)
        {
            switch (target)
            {
                case "Vehicles": return "VehicleID";
                case "Drivers": return "DriverID";
                case "Trips": return "TripID";
                case "MaintenanceRecords": return "RecordID";
                default: return null;
            }
        }

        private string GetDeleteMethodName(string target)
        {
            switch (target)
            {
                case "Vehicles": return "DeleteVehicle";
                case "Drivers": return "DeleteDriver";
                case "Trips": return "DeleteTrip";
                case "MaintenanceRecords": return "DeleteMaintenanceRecord";
                default:
                    throw new NotSupportedException($"Deletion is not available for {target}.");
            }
        }

        private static string GetInnermostMessage(Exception ex)
        {
            while (ex.InnerException != null)
            {
                ex = ex.InnerException;
            }

            return ex.Message;
        }

        private void EnsureService()
        {
            if (currentService == null)
            {
                SetService();
            }
        }

        private object InvokeServiceMethod(string methodName, params object[] parameters)
        {
            if (currentService == null)
            {
                throw new InvalidOperationException("Service is not initialized.");
            }

            var method = currentService.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public);
            if (method == null)
            {
                throw new MissingMethodException(currentService.GetType().FullName, methodName);
            }

            try
            {
                return method.Invoke(currentService, parameters);
            }
            catch (TargetInvocationException ex)
            {
                var root = ex.InnerException ?? ex;
                throw new InvalidOperationException(root.Message, root);
            }
        }

        private object CreateVehicleInstance(VehicleInput input)
        {
            if (currentService == null)
            {
                throw new InvalidOperationException("Service is not initialized.");
            }

            var vehicleType = currentService.GetType().Assembly.GetType("FleetApp.Models.Vehicle");
            if (vehicleType == null)
            {
                throw new InvalidOperationException("Vehicle type could not be located in the service assembly.");
            }

            var vehicle = Activator.CreateInstance(vehicleType);
            vehicleType.GetProperty("Make")?.SetValue(vehicle, input.Make);
            vehicleType.GetProperty("Model")?.SetValue(vehicle, input.Model);
            vehicleType.GetProperty("LicensePlate")?.SetValue(vehicle, input.LicensePlate);
            vehicleType.GetProperty("Mileage")?.SetValue(vehicle, input.Mileage);
            vehicleType.GetProperty("Status")?.SetValue(vehicle, input.Status);
            return vehicle;
        }

        private object CreateDriverInstance(DriverInput input)
        {
            if (currentService == null)
            {
                throw new InvalidOperationException("Service is not initialized.");
            }

            var driverType = currentService.GetType().Assembly.GetType("FleetApp.Models.Driver");
            if (driverType == null)
            {
                throw new InvalidOperationException("Driver type could not be located in the service assembly.");
            }

            var driver = Activator.CreateInstance(driverType);
            driverType.GetProperty("FirstName")?.SetValue(driver, input.FirstName);
            driverType.GetProperty("LastName")?.SetValue(driver, input.LastName);
            driverType.GetProperty("CNIC")?.SetValue(driver, input.CNIC);
            driverType.GetProperty("ContactNumber")?.SetValue(driver, input.ContactNumber);
            driverType.GetProperty("LicenseExpiry")?.SetValue(driver, input.LicenseExpiry);
            return driver;
        }

        private object CreateTripInstance(TripInput input)
        {
            if (currentService == null)
            {
                throw new InvalidOperationException("Service is not initialized.");
            }

            var tripType = currentService.GetType().Assembly.GetType("FleetApp.Models.Trip");
            if (tripType == null)
            {
                throw new InvalidOperationException("Trip type could not be located in the service assembly.");
            }

            var trip = Activator.CreateInstance(tripType);
            tripType.GetProperty("VehicleID")?.SetValue(trip, input.VehicleId);
            tripType.GetProperty("DriverID")?.SetValue(trip, input.DriverId);
            tripType.GetProperty("StartTime")?.SetValue(trip, input.StartTime);
            tripType.GetProperty("EndTime")?.SetValue(trip, input.EndTime);
            tripType.GetProperty("StartMileage")?.SetValue(trip, input.StartMileage);
            tripType.GetProperty("EndMileage")?.SetValue(trip, input.EndMileage);
            tripType.GetProperty("Purpose")?.SetValue(trip, input.Purpose);
            return trip;
        }

        private object CreateMaintenanceRecordInstance(MaintenanceInput input)
        {
            if (currentService == null)
            {
                throw new InvalidOperationException("Service is not initialized.");
            }

            var recordType = currentService.GetType().Assembly.GetType("FleetApp.Models.MaintenanceRecord");
            if (recordType == null)
            {
                throw new InvalidOperationException("MaintenanceRecord type could not be located in the service assembly.");
            }

            var record = Activator.CreateInstance(recordType);
            recordType.GetProperty("VehicleID")?.SetValue(record, input.VehicleId);
            recordType.GetProperty("ServiceDate")?.SetValue(record, input.ServiceDate);
            recordType.GetProperty("ServiceType")?.SetValue(record, input.ServiceType);
            recordType.GetProperty("Cost")?.SetValue(record, input.Cost);
            recordType.GetProperty("Description")?.SetValue(record, input.Description);
            return record;
        }
    }

    internal class AddVehicleForm : Form
    {
        private readonly TextBox txtMake = new TextBox();
        private readonly TextBox txtModel = new TextBox();
        private readonly TextBox txtLicensePlate = new TextBox();
        private readonly NumericUpDown numMileage = new NumericUpDown();
        private readonly ComboBox cmbStatus = new ComboBox();
        private readonly Button btnSave = new Button();
        private readonly Button btnCancel = new Button();

        private static readonly string[] AllowedMakes = { "Toyota", "Honda", "Suzuki" };
        private static readonly string[] AllowedModels = { "Corolla", "Civic", "Cultus" };

        public VehicleInput CreatedVehicle { get; private set; }

        public AddVehicleForm()
        {
            Text = "Add Vehicle";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new System.Drawing.Size(360, 300);
            MaximizeBox = false;
            MinimizeBox = false;

            Controls.Add(CreateLabel("Make", 15, 15));
            txtMake.Location = new System.Drawing.Point(140, 12);
            txtMake.Width = 190;
            Controls.Add(txtMake);
            Controls.Add(CreateHintLabel("Format: Toyota / Honda / Suzuki", 140, 35));

            Controls.Add(CreateLabel("Model", 15, 70));
            txtModel.Location = new System.Drawing.Point(140, 67);
            txtModel.Width = 190;
            Controls.Add(txtModel);
            Controls.Add(CreateHintLabel("Format: Corolla / Civic / Cultus", 140, 90));

            Controls.Add(CreateLabel("License Plate", 15, 125));
            txtLicensePlate.Location = new System.Drawing.Point(140, 122);
            txtLicensePlate.Width = 190;
            txtLicensePlate.CharacterCasing = CharacterCasing.Upper;
            Controls.Add(txtLicensePlate);
            Controls.Add(CreateHintLabel("Format: ABC-1234 (3 letters + '-' + digits)", 140, 145));

            Controls.Add(CreateLabel("Mileage", 15, 180));
            numMileage.Location = new System.Drawing.Point(140, 177);
            numMileage.Width = 190;
            numMileage.DecimalPlaces = 2;
            numMileage.Minimum = 0;
            numMileage.Maximum = 1000000;
            Controls.Add(numMileage);
            Controls.Add(CreateHintLabel("Range: 0 to 1,000,000 km", 140, 200));

            Controls.Add(CreateLabel("Status", 15, 215));
            cmbStatus.Location = new System.Drawing.Point(140, 212);
            cmbStatus.Width = 190;
            cmbStatus.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbStatus.Items.AddRange(new object[] { "Available", "In-Use", "Maintenance" });
            cmbStatus.SelectedIndex = 0;
            Controls.Add(cmbStatus);
            Controls.Add(CreateHintLabel("Select operational status", 140, 245));

            btnSave.Text = "Save";
            btnSave.Location = new System.Drawing.Point(140, 265);
            btnSave.Click += btnSave_Click;
            Controls.Add(btnSave);

            btnCancel.Text = "Cancel";
            btnCancel.Location = new System.Drawing.Point(235, 265);
            btnCancel.DialogResult = DialogResult.Cancel;
            Controls.Add(btnCancel);

            AcceptButton = btnSave;
            CancelButton = btnCancel;
        }

        private static Label CreateLabel(string text, int x, int y)
        {
            return new Label
            {
                Text = text,
                Location = new System.Drawing.Point(x, y),
                AutoSize = true
            };
        }

        private static Label CreateHintLabel(string text, int x, int y)
        {
            return new Label
            {
                Text = text,
                Location = new System.Drawing.Point(x, y),
                AutoSize = true,
                ForeColor = SystemColors.GrayText
            };
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (!ValidateForm())
            {
                return;
            }

            CreatedVehicle = new VehicleInput
            {
                Make = txtMake.Text.Trim(),
                Model = txtModel.Text.Trim(),
                LicensePlate = txtLicensePlate.Text.Trim(),
                Mileage = numMileage.Value,
                Status = cmbStatus.SelectedItem?.ToString() ?? "Available"
            };

            DialogResult = DialogResult.OK;
            Close();
        }

        private bool ValidateForm()
        {
            string make = txtMake.Text.Trim();
            string model = txtModel.Text.Trim();
            string licensePlate = txtLicensePlate.Text.Trim().ToUpperInvariant();

            if (Array.IndexOf(AllowedMakes, make) == -1)
            {
                MessageBox.Show("Make must be Toyota, Honda, or Suzuki.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (Array.IndexOf(AllowedModels, model) == -1)
            {
                MessageBox.Show("Model must be Corolla, Civic, or Cultus.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (!Regex.IsMatch(licensePlate, @"^[A-Z]{3}-\d{1,4}$"))
            {
                MessageBox.Show("License Plate must match ABC-1234 pattern.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (numMileage.Value < 0 || numMileage.Value > 1000000)
            {
                MessageBox.Show("Mileage must be between 0 and 1,000,000 km.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            txtMake.Text = make;
            txtModel.Text = model;
            txtLicensePlate.Text = licensePlate;
            return true;
        }
    }

    internal class AddDriverForm : Form
    {
        private readonly TextBox txtFirstName = new TextBox();
        private readonly TextBox txtLastName = new TextBox();
        private readonly TextBox txtCnic = new TextBox();
        private readonly TextBox txtContact = new TextBox();
        private readonly DateTimePicker dtLicenseExpiry = new DateTimePicker();
        private readonly Button btnSave = new Button();
        private readonly Button btnCancel = new Button();

        public DriverInput CreatedDriver { get; private set; }

        public AddDriverForm()
        {
            Text = "Add Driver";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new System.Drawing.Size(360, 300);
            MaximizeBox = false;
            MinimizeBox = false;

            Controls.Add(CreateLabel("First Name", 15, 15));
            txtFirstName.Location = new System.Drawing.Point(140, 12);
            txtFirstName.Width = 190;
            Controls.Add(txtFirstName);
            Controls.Add(CreateHintLabel("Format: Driver#### (e.g., Driver42)", 140, 35));

            Controls.Add(CreateLabel("Last Name", 15, 70));
            txtLastName.Location = new System.Drawing.Point(140, 67);
            txtLastName.Width = 190;
            Controls.Add(txtLastName);
            Controls.Add(CreateHintLabel("Format: User#### (e.g., User42)", 140, 90));

            Controls.Add(CreateLabel("CNIC", 15, 125));
            txtCnic.Location = new System.Drawing.Point(140, 122);
            txtCnic.Width = 190;
            Controls.Add(txtCnic);
            Controls.Add(CreateHintLabel("Format: 42201-0000001 (5 digits-7 digits)", 140, 145));

            Controls.Add(CreateLabel("Contact #", 15, 180));
            txtContact.Location = new System.Drawing.Point(140, 177);
            txtContact.Width = 190;
            Controls.Add(txtContact);
            Controls.Add(CreateHintLabel("Format: 0300-0000001 (4 digits-7 digits)", 140, 200));

            Controls.Add(CreateLabel("License Expiry", 15, 235));
            dtLicenseExpiry.Location = new System.Drawing.Point(140, 232);
            dtLicenseExpiry.Width = 190;
            dtLicenseExpiry.Format = DateTimePickerFormat.Short;
            Controls.Add(dtLicenseExpiry);
            Controls.Add(CreateHintLabel("Enter any valid calendar date", 140, 255));

            btnSave.Text = "Save";
            btnSave.Location = new System.Drawing.Point(140, 265);
            btnSave.Click += btnSave_Click;
            Controls.Add(btnSave);

            btnCancel.Text = "Cancel";
            btnCancel.Location = new System.Drawing.Point(235, 265);
            btnCancel.DialogResult = DialogResult.Cancel;
            Controls.Add(btnCancel);

            AcceptButton = btnSave;
            CancelButton = btnCancel;
        }

        private static Label CreateLabel(string text, int x, int y)
        {
            return new Label
            {
                Text = text,
                Location = new System.Drawing.Point(x, y),
                AutoSize = true
            };
        }

        private static Label CreateHintLabel(string text, int x, int y)
        {
            return new Label
            {
                Text = text,
                Location = new System.Drawing.Point(x, y),
                AutoSize = true,
                ForeColor = SystemColors.GrayText
            };
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (!ValidateForm())
            {
                return;
            }

            CreatedDriver = new DriverInput
            {
                FirstName = txtFirstName.Text.Trim(),
                LastName = txtLastName.Text.Trim(),
                CNIC = txtCnic.Text.Trim(),
                ContactNumber = txtContact.Text.Trim(),
                LicenseExpiry = dtLicenseExpiry.Value
            };

            DialogResult = DialogResult.OK;
            Close();
        }

        private bool ValidateForm()
        {
            string first = txtFirstName.Text.Trim();
            string last = txtLastName.Text.Trim();
            string cnic = txtCnic.Text.Trim();
            string contact = txtContact.Text.Trim();

            if (!Regex.IsMatch(first, @"^Driver\d{1,4}$", RegexOptions.IgnoreCase))
            {
                MessageBox.Show("First Name must follow Driver#### pattern.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (!Regex.IsMatch(last, @"^User\d{1,4}$", RegexOptions.IgnoreCase))
            {
                MessageBox.Show("Last Name must follow User#### pattern.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (!Regex.IsMatch(cnic, @"^\d{5}-\d{7}$"))
            {
                MessageBox.Show("CNIC must match 42201-0000001 pattern.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (!Regex.IsMatch(contact, @"^\d{4}-\d{7}$"))
            {
                MessageBox.Show("Contact must match 0300-0000001 pattern.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }
    }

    internal class AddTripForm : Form
    {
        private readonly NumericUpDown numVehicleId = new NumericUpDown();
        private readonly NumericUpDown numDriverId = new NumericUpDown();
        private readonly DateTimePicker dtStart = new DateTimePicker();
        private readonly DateTimePicker dtEnd = new DateTimePicker();
        private readonly NumericUpDown numStartMileage = new NumericUpDown();
        private readonly NumericUpDown numEndMileage = new NumericUpDown();
        private readonly TextBox txtPurpose = new TextBox();
        private readonly Button btnSave = new Button();
        private readonly Button btnCancel = new Button();

        public TripInput CreatedTrip { get; private set; }

        public AddTripForm()
        {
            Text = "Add Trip";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new System.Drawing.Size(430, 420);
            MaximizeBox = false;
            MinimizeBox = false;

            int labelX = 15;
            int inputX = 160;
            int top = 15;

            Controls.Add(CreateLabel("Vehicle ID", labelX, top));
            ConfigureNumeric(numVehicleId, inputX, top - 3, 1, 1000);
            Controls.Add(numVehicleId);
            Controls.Add(CreateHintLabel("Range: 1 - 1000", inputX, top + 25));
            top += 55;

            Controls.Add(CreateLabel("Driver ID", labelX, top));
            ConfigureNumeric(numDriverId, inputX, top - 3, 1, 1000);
            Controls.Add(numDriverId);
            Controls.Add(CreateHintLabel("Range: 1 - 1000", inputX, top + 25));
            top += 55;

            Controls.Add(CreateLabel("Start Time", labelX, top));
            ConfigureDateTime(dtStart, inputX, top - 5);
            Controls.Add(dtStart);
            Controls.Add(CreateHintLabel("Format: yyyy-MM-dd HH:mm", inputX, top + 25));
            top += 55;

            Controls.Add(CreateLabel("End Time", labelX, top));
            ConfigureDateTime(dtEnd, inputX, top - 5);
            dtEnd.Value = dtStart.Value.AddHours(1);
            Controls.Add(dtEnd);
            Controls.Add(CreateHintLabel("Must be after Start Time", inputX, top + 25));
            top += 55;

            Controls.Add(CreateLabel("Start Mileage", labelX, top));
            ConfigureMileage(numStartMileage, inputX, top - 3, 0, 1000000);
            Controls.Add(numStartMileage);
            Controls.Add(CreateHintLabel("Range: 0 - 1,000,000", inputX, top + 25));
            top += 55;

            Controls.Add(CreateLabel("End Mileage", labelX, top));
            ConfigureMileage(numEndMileage, inputX, top - 3, 1, 1000000);
            numEndMileage.Value = 5;
            Controls.Add(numEndMileage);
            Controls.Add(CreateHintLabel("Must be greater than Start Mileage", inputX, top + 25));
            top += 55;

            Controls.Add(CreateLabel("Purpose", labelX, top));
            txtPurpose.Location = new System.Drawing.Point(inputX, top - 3);
            txtPurpose.Width = 220;
            Controls.Add(txtPurpose);
            Controls.Add(CreateHintLabel("Format: Business Logistics", inputX, top + 25));
            top += 55;

            btnSave.Text = "Save";
            btnSave.Location = new System.Drawing.Point(inputX, top);
            btnSave.Click += btnSave_Click;
            Controls.Add(btnSave);

            btnCancel.Text = "Cancel";
            btnCancel.Location = new System.Drawing.Point(inputX + 95, top);
            btnCancel.DialogResult = DialogResult.Cancel;
            Controls.Add(btnCancel);

            AcceptButton = btnSave;
            CancelButton = btnCancel;
        }

        private static void ConfigureNumeric(NumericUpDown control, int x, int y, int min, int max)
        {
            control.Location = new System.Drawing.Point(x, y);
            control.Width = 120;
            control.Minimum = min;
            control.Maximum = max;
        }

        private static void ConfigureMileage(NumericUpDown control, int x, int y, int min, int max)
        {
            control.Location = new System.Drawing.Point(x, y);
            control.Width = 120;
            control.DecimalPlaces = 2;
            control.Minimum = min;
            control.Maximum = max;
        }

        private static void ConfigureDateTime(DateTimePicker dtp, int x, int y)
        {
            dtp.Location = new System.Drawing.Point(x, y);
            dtp.Width = 220;
            dtp.Format = DateTimePickerFormat.Custom;
            dtp.CustomFormat = "yyyy-MM-dd HH:mm";
            dtp.ShowUpDown = true;
        }

        private static Label CreateLabel(string text, int x, int y)
        {
            return new Label
            {
                Text = text,
                Location = new System.Drawing.Point(x, y),
                AutoSize = true
            };
        }

        private static Label CreateHintLabel(string text, int x, int y)
        {
            return new Label
            {
                Text = text,
                Location = new System.Drawing.Point(x, y),
                AutoSize = true,
                ForeColor = SystemColors.GrayText
            };
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (!ValidateForm())
            {
                return;
            }

            CreatedTrip = new TripInput
            {
                VehicleId = (int)numVehicleId.Value,
                DriverId = (int)numDriverId.Value,
                StartTime = dtStart.Value,
                EndTime = dtEnd.Value,
                StartMileage = numStartMileage.Value,
                EndMileage = numEndMileage.Value,
                Purpose = txtPurpose.Text.Trim()
            };

            DialogResult = DialogResult.OK;
            Close();
        }

        private bool ValidateForm()
        {
            if (numVehicleId.Value < 1 || numVehicleId.Value > 1000)
            {
                MessageBox.Show("Vehicle ID must be between 1 and 1000.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (numDriverId.Value < 1 || numDriverId.Value > 1000)
            {
                MessageBox.Show("Driver ID must be between 1 and 1000.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (dtEnd.Value <= dtStart.Value)
            {
                MessageBox.Show("End Time must be after Start Time.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (numEndMileage.Value <= numStartMileage.Value)
            {
                MessageBox.Show("End Mileage must exceed Start Mileage.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (!string.Equals(txtPurpose.Text.Trim(), "Business Logistics", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("Purpose must match 'Business Logistics'.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            txtPurpose.Text = "Business Logistics";
            return true;
        }
    }

    internal class AddMaintenanceForm : Form
    {
        private readonly NumericUpDown numVehicleId = new NumericUpDown();
        private readonly DateTimePicker dtServiceDate = new DateTimePicker();
        private readonly TextBox txtServiceType = new TextBox();
        private readonly NumericUpDown numCost = new NumericUpDown();
        private readonly TextBox txtDescription = new TextBox();
        private readonly Button btnSave = new Button();
        private readonly Button btnCancel = new Button();

        private static readonly string[] AllowedServiceTypes = { "Oil Change", "Tire Rotation" };

        public MaintenanceInput CreatedRecord { get; private set; }

        public AddMaintenanceForm()
        {
            Text = "Add Maintenance Record";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new System.Drawing.Size(400, 360);
            MaximizeBox = false;
            MinimizeBox = false;

            Controls.Add(CreateLabel("Vehicle ID", 15, 15));
            ConfigureNumeric(numVehicleId, 140, 12, 1, 1000);
            Controls.Add(numVehicleId);
            Controls.Add(CreateHintLabel("Range: 1 - 1000", 140, 35));

            Controls.Add(CreateLabel("Service Date", 15, 70));
            dtServiceDate.Location = new System.Drawing.Point(140, 67);
            dtServiceDate.Width = 200;
            dtServiceDate.Format = DateTimePickerFormat.Short;
            Controls.Add(dtServiceDate);
            Controls.Add(CreateHintLabel("Format: yyyy-MM-dd (within last year)", 140, 90));

            Controls.Add(CreateLabel("Service Type", 15, 125));
            txtServiceType.Location = new System.Drawing.Point(140, 122);
            txtServiceType.Width = 220;
            Controls.Add(txtServiceType);
            Controls.Add(CreateHintLabel("Format: Oil Change / Tire Rotation", 140, 145));

            Controls.Add(CreateLabel("Cost", 15, 180));
            numCost.Location = new System.Drawing.Point(140, 177);
            numCost.Width = 120;
            numCost.DecimalPlaces = 2;
            numCost.Minimum = -1000000;
            numCost.Maximum = 1000000;
            numCost.Increment = 50;
            Controls.Add(numCost);
            Controls.Add(CreateHintLabel("Enter any amount (negatives rejected by DB trigger)", 140, 200));

            Controls.Add(CreateLabel("Description", 15, 235));
            txtDescription.Location = new System.Drawing.Point(140, 232);
            txtDescription.Width = 220;
            txtDescription.Height = 60;
            txtDescription.Multiline = true;
            Controls.Add(txtDescription);
            Controls.Add(CreateHintLabel("Format: Routine Maintenance", 140, 295));

            btnSave.Text = "Save";
            btnSave.Location = new System.Drawing.Point(140, 310);
            btnSave.Click += btnSave_Click;
            Controls.Add(btnSave);

            btnCancel.Text = "Cancel";
            btnCancel.Location = new System.Drawing.Point(235, 310);
            btnCancel.DialogResult = DialogResult.Cancel;
            Controls.Add(btnCancel);

            AcceptButton = btnSave;
            CancelButton = btnCancel;
        }

        private static void ConfigureNumeric(NumericUpDown control, int x, int y, int min, int max)
        {
            control.Location = new System.Drawing.Point(x, y);
            control.Width = 120;
            control.Minimum = min;
            control.Maximum = max;
        }

        private static Label CreateLabel(string text, int x, int y)
        {
            return new Label
            {
                Text = text,
                Location = new System.Drawing.Point(x, y),
                AutoSize = true
            };
        }

        private static Label CreateHintLabel(string text, int x, int y)
        {
            return new Label
            {
                Text = text,
                Location = new System.Drawing.Point(x, y),
                AutoSize = true,
                ForeColor = SystemColors.GrayText
            };
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (!ValidateForm())
            {
                return;
            }

            CreatedRecord = new MaintenanceInput
            {
                VehicleId = (int)numVehicleId.Value,
                ServiceDate = dtServiceDate.Value,
                ServiceType = txtServiceType.Text.Trim(),
                Cost = numCost.Value,
                Description = txtDescription.Text.Trim()
            };

            DialogResult = DialogResult.OK;
            Close();
        }

        private bool ValidateForm()
        {
            if (numVehicleId.Value < 1 || numVehicleId.Value > 1000)
            {
                MessageBox.Show("Vehicle ID must be between 1 and 1000.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (dtServiceDate.Value < DateTime.Now.AddYears(-1) || dtServiceDate.Value > DateTime.Now)
            {
                MessageBox.Show("Service Date must be within the last year.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            string serviceType = txtServiceType.Text.Trim();
            if (Array.IndexOf(AllowedServiceTypes, serviceType) == -1)
            {
                MessageBox.Show("Service Type must be Oil Change or Tire Rotation.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtDescription.Text))
            {
                MessageBox.Show("Description is required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            txtServiceType.Text = serviceType;
            txtDescription.Text = txtDescription.Text.Trim();
            return true;
        }
    }

    internal class UpdateVehicleStatusForm : Form
    {
        private readonly ComboBox cmbStatus = new ComboBox();
        private readonly Button btnSave = new Button();
        private readonly Button btnCancel = new Button();

        public string SelectedStatus { get; private set; }

        public UpdateVehicleStatusForm(string currentStatus)
        {
            Text = "Update Vehicle Status";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new System.Drawing.Size(320, 140);
            MaximizeBox = false;
            MinimizeBox = false;

            Controls.Add(CreateLabel("New Status", 15, 20));
            cmbStatus.Location = new System.Drawing.Point(120, 17);
            cmbStatus.Width = 170;
            cmbStatus.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbStatus.Items.AddRange(new object[] { "Available", "In-Use", "Maintenance" });
            if (!string.IsNullOrWhiteSpace(currentStatus) && cmbStatus.Items.Contains(currentStatus))
            {
                cmbStatus.SelectedItem = currentStatus;
            }
            else
            {
                cmbStatus.SelectedIndex = 0;
            }
            Controls.Add(cmbStatus);

            btnSave.Text = "Save";
            btnSave.Location = new System.Drawing.Point(120, 70);
            btnSave.Click += btnSave_Click;
            Controls.Add(btnSave);

            btnCancel.Text = "Cancel";
            btnCancel.Location = new System.Drawing.Point(210, 70);
            btnCancel.DialogResult = DialogResult.Cancel;
            Controls.Add(btnCancel);

            AcceptButton = btnSave;
            CancelButton = btnCancel;
        }

        private static Label CreateLabel(string text, int x, int y)
        {
            return new Label
            {
                Text = text,
                Location = new System.Drawing.Point(x, y),
                AutoSize = true
            };
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (cmbStatus.SelectedItem == null)
            {
                MessageBox.Show("Select a status before saving.", "Update Status", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SelectedStatus = cmbStatus.SelectedItem.ToString();
            DialogResult = DialogResult.OK;
            Close();
        }
    }

    internal class FuelCostInputForm : Form
    {
        private readonly NumericUpDown numPrice = new NumericUpDown();
        private readonly Button btnSave = new Button();
        private readonly Button btnCancel = new Button();

        public decimal FuelPricePerLiter { get; private set; } = 1;

        public FuelCostInputForm()
        {
            Text = "Fuel Price";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new System.Drawing.Size(300, 130);
            MaximizeBox = false;
            MinimizeBox = false;

            Controls.Add(CreateLabel("Price per Liter", 15, 20));
            numPrice.Location = new System.Drawing.Point(140, 18);
            numPrice.Width = 120;
            numPrice.DecimalPlaces = 2;
            numPrice.Minimum = 1;
            numPrice.Maximum = 1000;
            numPrice.Value = 250;
            Controls.Add(numPrice);

            btnSave.Text = "Calculate";
            btnSave.Location = new System.Drawing.Point(140, 70);
            btnSave.Click += btnSave_Click;
            Controls.Add(btnSave);

            btnCancel.Text = "Cancel";
            btnCancel.Location = new System.Drawing.Point(210, 70);
            btnCancel.DialogResult = DialogResult.Cancel;
            Controls.Add(btnCancel);

            AcceptButton = btnSave;
            CancelButton = btnCancel;
        }

        private static Label CreateLabel(string text, int x, int y)
        {
            return new Label
            {
                Text = text,
                Location = new System.Drawing.Point(x, y),
                AutoSize = true
            };
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            FuelPricePerLiter = numPrice.Value;
            DialogResult = DialogResult.OK;
            Close();
        }
    }

    internal class VehicleHistoryForm : Form
    {
        private readonly DataGridView grid = new DataGridView();

        public VehicleHistoryForm(int vehicleId, DataTable data)
        {
            Text = $"Vehicle #{vehicleId} — Recent Trips";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new System.Drawing.Size(600, 300);
            MaximizeBox = false;
            MinimizeBox = false;

            grid.Dock = DockStyle.Fill;
            grid.ReadOnly = true;
            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            grid.DataSource = data;
            Controls.Add(grid);
        }
    }

    internal class UpdateDriverForm : Form
    {
        private readonly TextBox txtFirstName = new TextBox();
        private readonly TextBox txtLastName = new TextBox();
        private readonly TextBox txtCnic = new TextBox();
        private readonly TextBox txtContact = new TextBox();
        private readonly DateTimePicker dtLicenseExpiry = new DateTimePicker();
        private readonly Button btnSave = new Button();
        private readonly Button btnCancel = new Button();

        public DriverInput CreatedDriver { get; private set; }

        public UpdateDriverForm(DriverInput existing)
        {
            Text = "Update Driver";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new System.Drawing.Size(360, 300);
            MaximizeBox = false;
            MinimizeBox = false;

            Controls.Add(CreateLabel("First Name", 15, 15));
            txtFirstName.Location = new System.Drawing.Point(140, 12);
            txtFirstName.Width = 190;
            txtFirstName.Text = existing.FirstName;
            Controls.Add(txtFirstName);
            Controls.Add(CreateHintLabel("Format: Driver####", 140, 35));

            Controls.Add(CreateLabel("Last Name", 15, 70));
            txtLastName.Location = new System.Drawing.Point(140, 67);
            txtLastName.Width = 190;
            txtLastName.Text = existing.LastName;
            Controls.Add(txtLastName);
            Controls.Add(CreateHintLabel("Format: User####", 140, 90));

            Controls.Add(CreateLabel("CNIC", 15, 125));
            txtCnic.Location = new System.Drawing.Point(140, 122);
            txtCnic.Width = 190;
            txtCnic.Text = existing.CNIC;
            Controls.Add(txtCnic);
            Controls.Add(CreateHintLabel("Format: 42201-0000001", 140, 145));

            Controls.Add(CreateLabel("Contact #", 15, 180));
            txtContact.Location = new System.Drawing.Point(140, 177);
            txtContact.Width = 190;
            txtContact.Text = existing.ContactNumber;
            Controls.Add(txtContact);
            Controls.Add(CreateHintLabel("Format: 0300-0000001", 140, 200));

            Controls.Add(CreateLabel("License Expiry", 15, 235));
            dtLicenseExpiry.Location = new System.Drawing.Point(140, 232);
            dtLicenseExpiry.Width = 190;
            dtLicenseExpiry.Format = DateTimePickerFormat.Short;
            dtLicenseExpiry.Value = existing.LicenseExpiry;
            Controls.Add(dtLicenseExpiry);
            Controls.Add(CreateHintLabel("Enter any valid calendar date", 140, 255));

            btnSave.Text = "Update";
            btnSave.Location = new System.Drawing.Point(140, 265);
            btnSave.Click += btnSave_Click;
            Controls.Add(btnSave);

            btnCancel.Text = "Cancel";
            btnCancel.Location = new System.Drawing.Point(235, 265);
            btnCancel.DialogResult = DialogResult.Cancel;
            Controls.Add(btnCancel);

            AcceptButton = btnSave;
            CancelButton = btnCancel;
        }

        private static Label CreateLabel(string text, int x, int y)
        {
            return new Label
            {
                Text = text,
                Location = new System.Drawing.Point(x, y),
                AutoSize = true
            };
        }

        private static Label CreateHintLabel(string text, int x, int y)
        {
            return new Label
            {
                Text = text,
                Location = new System.Drawing.Point(x, y),
                AutoSize = true,
                ForeColor = SystemColors.GrayText
            };
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (!ValidateForm())
            {
                return;
            }

            CreatedDriver = new DriverInput
            {
                FirstName = txtFirstName.Text.Trim(),
                LastName = txtLastName.Text.Trim(),
                CNIC = txtCnic.Text.Trim(),
                ContactNumber = txtContact.Text.Trim(),
                LicenseExpiry = dtLicenseExpiry.Value
            };

            DialogResult = DialogResult.OK;
            Close();
        }

        private bool ValidateForm()
        {
            if (!Regex.IsMatch(txtFirstName.Text.Trim(), @"^Driver\d{1,4}$", RegexOptions.IgnoreCase))
            {
                MessageBox.Show("First Name must follow Driver#### pattern.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (!Regex.IsMatch(txtLastName.Text.Trim(), @"^User\d{1,4}$", RegexOptions.IgnoreCase))
            {
                MessageBox.Show("Last Name must follow User#### pattern.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (!Regex.IsMatch(txtCnic.Text.Trim(), @"^\d{5}-\d{7}$"))
            {
                MessageBox.Show("CNIC must match 42201-0000001 pattern.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (!Regex.IsMatch(txtContact.Text.Trim(), @"^\d{4}-\d{7}$"))
            {
                MessageBox.Show("Contact must match 0300-0000001 pattern.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }
    }

    internal class VehicleInput
    {
        public string Make { get; set; }
        public string Model { get; set; }
        public string LicensePlate { get; set; }
        public decimal Mileage { get; set; }
        public string Status { get; set; }
    }

    internal class DriverInput
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string CNIC { get; set; }
        public string ContactNumber { get; set; }
        public DateTime LicenseExpiry { get; set; }
    }

    internal class TripInput
    {
        public int VehicleId { get; set; }
        public int DriverId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public decimal StartMileage { get; set; }
        public decimal EndMileage { get; set; }
        public string Purpose { get; set; }
    }

    internal class MaintenanceInput
    {
        public int VehicleId { get; set; }
        public DateTime ServiceDate { get; set; }
        public string ServiceType { get; set; }
        public decimal Cost { get; set; }
        public string Description { get; set; }
    }
}