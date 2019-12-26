using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using LicensingLibrary.General.Security;

namespace LicenseGenerator
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btnCreateLicense_Click(object sender, EventArgs e)
        {
            if (!ValidateInputs())
            {
                MessageBox.Show(this, "Missing Inputs!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            using (var fbd = new FolderBrowserDialog())
            {
                fbd.Description = "Select the folder that contains private key file. If private key file does not exist in selected folder, will be created";

                DialogResult result = fbd.ShowDialog();
                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                    CreateNewLicense(fbd.SelectedPath, dtpStart.Value, dtpEnd.Value, txtProductName.Text, txtCustomerName.Text);
            }

        }


        private bool ValidateInputs()
        {
            if (string.IsNullOrEmpty(txtCustomerName.Text) || string.IsNullOrEmpty(txtProductName.Text))
                return false;
            return true;
        }

        private void CreateNewLicense(string licenseOutputFolder, DateTime startDate, DateTime endDate, string productName, string customerName)
        {
            try
            {
                //create new app license sources if private key file didn't created before
                if (!File.Exists(licenseOutputFolder + "\\privateKey.xml"))
                    LicenseAuthorization.GenerateLicenseResources(licenseOutputFolder);

                //create new license file (*.lic) for customer
                LicenseAuthorization.GenerateUserLicenseFile(licenseOutputFolder, productName, customerName, startDate, endDate);

                MessageBox.Show($"{customerName} License file created in {licenseOutputFolder}");
                Process.Start(licenseOutputFolder);
            }
            catch (Exception e)
            {
                MessageBox.Show(this, e.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

    }
}
