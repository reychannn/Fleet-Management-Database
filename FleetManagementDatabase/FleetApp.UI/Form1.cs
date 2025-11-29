using FleetApp.BLL;
using System;
using System.Data;
using System.Windows.Forms;

namespace FleetApp.UI
{
    // Ensure you add two Radio Buttons (rbLinq, rbSP) and one Button (btnLoad) in the designer!
    public partial class Form1 : Form
    {
        private IFleetService currentService;

        public Form1()
        {
            InitializeComponent();
            rbSP.Checked = true;
            SetService();
        }

        // Method to instantiate the correct BLL based on radio button selection (Factory Pattern)
        private void SetService()
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
        }

        private void rb_CheckedChanged(object sender, EventArgs e)
        {
            SetService();
        }

        // Test method demonstrating feature usage
        private void btnLoad_Click(object sender, EventArgs e)
        {
            try
            {
                // This call uses the BLL instance selected by the Factory at runtime!
                DataTable highMaint = currentService.GetHighMaintenanceVehicles();
                MessageBox.Show($"Loaded {highMaint.Rows.Count} high-maintenance vehicles using the {this.Text} BLL.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                // Hint: Use a DataGridView to display the results!
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}