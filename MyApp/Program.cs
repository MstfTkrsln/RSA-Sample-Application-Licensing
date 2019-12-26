using System;
using System.IO;
using System.Security;
using System.Security.Cryptography;
using System.Windows.Forms;
using LicensingLibrary.General.Security;

namespace MyApp
{
    static class Program
    {
        private const string ProductName = "MyApp";

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // run the license validation routine before running the main form:
            if (validateLicenseFile())
                Application.Run(new Form1());
        }

        /// <summary>
        /// locate/request the license file, and validate the signature and terms.
        /// </summary>
        /// <returns></returns>
        internal static bool validateLicenseFile()
        {
            try
            {
                // reserve a license object:
                License license = null;

                // get the public key from resources
                String publicKey = Properties.Resources.publicKey;

                // work out the expected license file-name:
                String licenseFile = Application.LocalUserAppDataPath + "\\license.lic";

                // does the license file exist?
                if (File.Exists(licenseFile))
                {
                    // load the license:
                    license = License.Load(licenseFile);
                }
                else
                {
                    // prompt the user for a license file:
                    OpenFileDialog dlg = new OpenFileDialog();

                    // setup a dialog;
                    dlg.Filter = "User License Files (*.lic)|*.lic";
                    dlg.Title = "Select License File";

                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        try
                        {
                            // copy the license file into the local app data directory:
                            File.Copy(dlg.FileName, licenseFile);

                            // if it made it here, load it:
                            if (File.Exists(licenseFile))
                            {
                                license = License.Load(licenseFile);
                            }
                        }
                        catch
                        {
                            // can't copy the file?.. load the original:
                            license = License.Load(dlg.FileName);
                        }
                    }
                }
                if (license != null)
                {
                    // validate the signature on the license with the message contents, and the public key:
                    LicenseAuthorization.ValidateLicense(license, publicKey, ProductName);

                    // if we get here, the license is valid;
                    return true;
                }
                else
                {
                    // no license file...
                    MessageBox.Show("License File Not Supplied!", "License Check");
                    return false;
                }
            }
            catch (SecurityException se)
            {
                // display the reason for the license check failure:
                MessageBox.Show(se.Message, "License Check");
            }

            // return false...invalid license.
            return false;
        }
    }
}
