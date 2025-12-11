namespace FleetApp.UI
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.RadioButton rbSP;
        private System.Windows.Forms.RadioButton rbLinq;
        private System.Windows.Forms.Button btnLoad;
        private System.Windows.Forms.ComboBox cmbObject;
        private System.Windows.Forms.Button btnAddVehicle;
        private System.Windows.Forms.Button btnAddDriver;
        private System.Windows.Forms.Button btnAddTrip;
        private System.Windows.Forms.Button btnAddMaintenance;
        private System.Windows.Forms.DataGridView gridResults;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Label lblObject;
        private System.Windows.Forms.Label lblSearch;
        private System.Windows.Forms.TextBox txtSearch;
        private System.Windows.Forms.Button btnSearch;
        private System.Windows.Forms.Button btnDeleteSelected;
        private System.Windows.Forms.Button btnUpdateStatus;
        private System.Windows.Forms.Button btnVehicleHistory;
        private System.Windows.Forms.Button btnFuelCost;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.rbSP = new System.Windows.Forms.RadioButton();
            this.rbLinq = new System.Windows.Forms.RadioButton();
            this.btnLoad = new System.Windows.Forms.Button();
            this.cmbObject = new System.Windows.Forms.ComboBox();
            this.btnAddVehicle = new System.Windows.Forms.Button();
            this.btnAddDriver = new System.Windows.Forms.Button();
            this.btnAddTrip = new System.Windows.Forms.Button();
            this.btnAddMaintenance = new System.Windows.Forms.Button();
            this.gridResults = new System.Windows.Forms.DataGridView();
            this.lblStatus = new System.Windows.Forms.Label();
            this.lblObject = new System.Windows.Forms.Label();
            this.lblSearch = new System.Windows.Forms.Label();
            this.txtSearch = new System.Windows.Forms.TextBox();
            this.btnSearch = new System.Windows.Forms.Button();
            this.btnDeleteSelected = new System.Windows.Forms.Button();
            this.btnUpdateStatus = new System.Windows.Forms.Button();
            this.btnVehicleHistory = new System.Windows.Forms.Button();
            this.btnFuelCost = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.gridResults)).BeginInit();
            this.SuspendLayout();
            // 
            // rbSP
            // 
            this.rbSP.AutoSize = true;
            this.rbSP.Location = new System.Drawing.Point(12, 12);
            this.rbSP.Name = "rbSP";
            this.rbSP.Size = new System.Drawing.Size(110, 17);
            this.rbSP.TabIndex = 0;
            this.rbSP.Text = "Stored Procedures";
            this.rbSP.UseVisualStyleBackColor = true;
            this.rbSP.CheckedChanged += new System.EventHandler(this.rb_CheckedChanged);
            // 
            // rbLinq
            // 
            this.rbLinq.AutoSize = true;
            this.rbLinq.Location = new System.Drawing.Point(12, 35);
            this.rbLinq.Name = "rbLinq";
            this.rbLinq.Size = new System.Drawing.Size(50, 17);
            this.rbLinq.TabIndex = 1;
            this.rbLinq.Text = "LINQ";
            this.rbLinq.UseVisualStyleBackColor = true;
            this.rbLinq.CheckedChanged += new System.EventHandler(this.rb_CheckedChanged);
            // 
            // lblObject
            // 
            this.lblObject.AutoSize = true;
            this.lblObject.Location = new System.Drawing.Point(150, 14);
            this.lblObject.Name = "lblObject";
            this.lblObject.Size = new System.Drawing.Size(124, 13);
            this.lblObject.TabIndex = 2;
            this.lblObject.Text = "Table/View to Visualize";
            // 
            // cmbObject
            // 
            this.cmbObject.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbObject.FormattingEnabled = true;
            this.cmbObject.Location = new System.Drawing.Point(153, 30);
            this.cmbObject.Name = "cmbObject";
            this.cmbObject.Size = new System.Drawing.Size(280, 21);
            this.cmbObject.TabIndex = 3;
            this.cmbObject.SelectedIndexChanged += new System.EventHandler(this.cmbObject_SelectedIndexChanged);
            // 
            // btnLoad
            // 
            this.btnLoad.Location = new System.Drawing.Point(450, 29);
            this.btnLoad.Name = "btnLoad";
            this.btnLoad.Size = new System.Drawing.Size(120, 23);
            this.btnLoad.TabIndex = 4;
            this.btnLoad.Text = "Load Selection";
            this.btnLoad.UseVisualStyleBackColor = true;
            this.btnLoad.Click += new System.EventHandler(this.btnLoad_Click);
            // 
            // btnAddVehicle
            // 
            this.btnAddVehicle.Location = new System.Drawing.Point(580, 29);
            this.btnAddVehicle.Name = "btnAddVehicle";
            this.btnAddVehicle.Size = new System.Drawing.Size(110, 23);
            this.btnAddVehicle.TabIndex = 5;
            this.btnAddVehicle.Text = "Add Vehicle";
            this.btnAddVehicle.UseVisualStyleBackColor = true;
            this.btnAddVehicle.Click += new System.EventHandler(this.btnAddVehicle_Click);
            // 
            // btnAddDriver
            // 
            this.btnAddDriver.Location = new System.Drawing.Point(700, 29);
            this.btnAddDriver.Name = "btnAddDriver";
            this.btnAddDriver.Size = new System.Drawing.Size(100, 23);
            this.btnAddDriver.TabIndex = 6;
            this.btnAddDriver.Text = "Add Driver";
            this.btnAddDriver.UseVisualStyleBackColor = true;
            this.btnAddDriver.Click += new System.EventHandler(this.btnAddDriver_Click);
            // 
            // btnVehicleHistory
            // 
            this.btnVehicleHistory.Location = new System.Drawing.Point(700, 58);
            this.btnVehicleHistory.Name = "btnVehicleHistory";
            this.btnVehicleHistory.Size = new System.Drawing.Size(100, 23);
            this.btnVehicleHistory.TabIndex = 15;
            this.btnVehicleHistory.Text = "Vehicle History";
            this.btnVehicleHistory.UseVisualStyleBackColor = true;
            this.btnVehicleHistory.Click += new System.EventHandler(this.btnVehicleHistory_Click);
            // 
            // btnAddTrip
            // 
            this.btnAddTrip.Location = new System.Drawing.Point(450, 58);
            this.btnAddTrip.Name = "btnAddTrip";
            this.btnAddTrip.Size = new System.Drawing.Size(120, 23);
            this.btnAddTrip.TabIndex = 7;
            this.btnAddTrip.Text = "Add Trip";
            this.btnAddTrip.UseVisualStyleBackColor = true;
            this.btnAddTrip.Click += new System.EventHandler(this.btnAddTrip_Click);
            // 
            // btnAddMaintenance
            // 
            this.btnAddMaintenance.Location = new System.Drawing.Point(580, 58);
            this.btnAddMaintenance.Name = "btnAddMaintenance";
            this.btnAddMaintenance.Size = new System.Drawing.Size(120, 23);
            this.btnAddMaintenance.TabIndex = 8;
            this.btnAddMaintenance.Text = "Add Maintenance";
            this.btnAddMaintenance.UseVisualStyleBackColor = true;
            this.btnAddMaintenance.Click += new System.EventHandler(this.btnAddMaintenance_Click);
            // 
            // btnUpdateStatus
            // 
            this.btnUpdateStatus.Location = new System.Drawing.Point(700, 88);
            this.btnUpdateStatus.Name = "btnUpdateStatus";
            this.btnUpdateStatus.Size = new System.Drawing.Size(100, 23);
            this.btnUpdateStatus.TabIndex = 16;
            this.btnUpdateStatus.Text = "Update Status";
            this.btnUpdateStatus.UseVisualStyleBackColor = true;
            this.btnUpdateStatus.Click += new System.EventHandler(this.btnUpdateStatus_Click);
            // 
            // gridResults
            // 
            this.gridResults.AllowUserToAddRows = false;
            this.gridResults.AllowUserToDeleteRows = false;
            this.gridResults.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gridResults.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.gridResults.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gridResults.Location = new System.Drawing.Point(12, 120);
            this.gridResults.MultiSelect = false;
            this.gridResults.Name = "gridResults";
            this.gridResults.ReadOnly = true;
            this.gridResults.Size = new System.Drawing.Size(776, 290);
            this.gridResults.TabIndex = 9;
            // 
            // lblStatus
            // 
            this.lblStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblStatus.AutoSize = true;
            this.lblStatus.Location = new System.Drawing.Point(12, 420);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(196, 13);
            this.lblStatus.TabIndex = 10;
            this.lblStatus.Text = "Select a table or view and click Load.";
            // 
            // lblSearch
            // 
            this.lblSearch.AutoSize = true;
            this.lblSearch.Location = new System.Drawing.Point(150, 74);
            this.lblSearch.Name = "lblSearch";
            this.lblSearch.Size = new System.Drawing.Size(41, 13);
            this.lblSearch.TabIndex = 11;
            this.lblSearch.Text = "Search";
            // 
            // txtSearch
            // 
            this.txtSearch.Location = new System.Drawing.Point(153, 90);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.Size = new System.Drawing.Size(280, 20);
            this.txtSearch.TabIndex = 12;
            // 
            // btnSearch
            // 
            this.btnSearch.Location = new System.Drawing.Point(450, 88);
            this.btnSearch.Name = "btnSearch";
            this.btnSearch.Size = new System.Drawing.Size(120, 23);
            this.btnSearch.TabIndex = 13;
            this.btnSearch.Text = "Search";
            this.btnSearch.UseVisualStyleBackColor = true;
            this.btnSearch.Click += new System.EventHandler(this.btnSearch_Click);
            // 
            // btnDeleteSelected
            // 
            this.btnDeleteSelected.Location = new System.Drawing.Point(580, 88);
            this.btnDeleteSelected.Name = "btnDeleteSelected";
            this.btnDeleteSelected.Size = new System.Drawing.Size(120, 23);
            this.btnDeleteSelected.TabIndex = 14;
            this.btnDeleteSelected.Text = "Delete Selected";
            this.btnDeleteSelected.UseVisualStyleBackColor = true;
            this.btnDeleteSelected.Click += new System.EventHandler(this.btnDeleteSelected_Click);
            // 
            // btnFuelCost
            // 
            this.btnFuelCost.Location = new System.Drawing.Point(700, 117);
            this.btnFuelCost.Name = "btnFuelCost";
            this.btnFuelCost.Size = new System.Drawing.Size(100, 23);
            this.btnFuelCost.TabIndex = 17;
            this.btnFuelCost.Text = "Fuel Cost";
            this.btnFuelCost.UseVisualStyleBackColor = true;
            this.btnFuelCost.Click += new System.EventHandler(this.btnFuelCost_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(820, 450);
            this.Controls.Add(this.btnFuelCost);
            this.Controls.Add(this.btnVehicleHistory);
            this.Controls.Add(this.btnUpdateStatus);
            this.Controls.Add(this.btnDeleteSelected);
            this.Controls.Add(this.btnSearch);
            this.Controls.Add(this.txtSearch);
            this.Controls.Add(this.lblSearch);
            this.Controls.Add(this.btnAddMaintenance);
            this.Controls.Add(this.btnAddTrip);
            this.Controls.Add(this.btnAddDriver);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.gridResults);
            this.Controls.Add(this.btnAddVehicle);
            this.Controls.Add(this.btnLoad);
            this.Controls.Add(this.cmbObject);
            this.Controls.Add(this.lblObject);
            this.Controls.Add(this.rbLinq);
            this.Controls.Add(this.rbSP);
            this.Name = "Form1";
            this.Text = "Fleet Management";
            ((System.ComponentModel.ISupportInitialize)(this.gridResults)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
    }
}

