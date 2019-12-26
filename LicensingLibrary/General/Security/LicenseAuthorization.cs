using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security;
using System.Security.Cryptography;
using LicensingLibrary.General.IO;

namespace LicensingLibrary.General.Security
{
    /// <summary>
    /// handles license authorization.
    /// </summary>
    public class LicenseAuthorization
    {
        /// <summary>
        /// terms of the license agreement: it's not encrypted (but is obscured)
        /// </summary>
        [Serializable]
        internal class LicenseTerms
        {
            /// <summary>
            /// start date of the license agreement.
            /// </summary>
            public DateTime StartDate { get; set; }

            /// <summary>
            /// registered user name for the license agreement.
            /// </summary>
            public String UserName { get; set; }

            /// <summary>
            /// the assembly name of the product that is licensed.
            /// </summary>
            public String ProductName { get; set; }

            /// <summary>
            /// the last date on which the software can be used on this license.
            /// </summary>
            public DateTime EndDate { get; set; }

            /// <summary>
            /// returns the license terms as an obscure (not human readable) string.
            /// </summary>
            /// <returns></returns>
            public String GetLicenseString()
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    // create a binary formatter:
                    BinaryFormatter bnfmt = new BinaryFormatter();

                    // serialize the data to the memory-steam;
                    bnfmt.Serialize(ms, this);

                    // return a base64 string representation of the binary data:
                    return Convert.ToBase64String(ms.GetBuffer());

                }
            }

            /// <summary>
            /// returns a binary representation of the license terms.
            /// </summary>
            /// <returns></returns>
            public byte[] GetLicenseData()
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    // create a binary formatter:
                    BinaryFormatter bnfmt = new BinaryFormatter();

                    // serialize the data to the memory-steam;
                    bnfmt.Serialize(ms, this);

                    // return a base64 string representation of the binary data:
                    return ms.GetBuffer();

                }
            }

            /// <summary>
            /// create a new license-terms object from a string-representation of the binary
            /// serialization of the licence-terms.
            /// </summary>
            /// <param name="licenseTerms"></param>
            /// <returns></returns>
            internal static LicenseTerms FromString(String licenseTerms)
            {

                using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(licenseTerms)))
                {
                    // create a binary formatter:
                    BinaryFormatter bnfmt = new BinaryFormatter();

                    // serialize the data to the memory-steam;
                    object value = bnfmt.Deserialize(ms);

                    if (value is LicenseTerms)
                        return (LicenseTerms)value;
                    else
                        throw new ApplicationException("Invalid Type!");

                }
            }

        }

        ///// <summary>
        ///// builds a user-license pack. This includes the public-key that must be embedded in the application,
        ///// and the private key (which must be kept secure) and a license-file for each user, specific to the 
        ///// currently executing assembly, with the specified end date. Start date for the user-license file is
        ///// current date.
        ///// </summary>
        ///// <param name="outputFolder"></param>
        ///// <param name="userNames"></param>
        ///// <param name="endDates"></param>
        //public static void GenerateLicensePack(String outputFolder, String[] userNames, DateTime[] endDates)
        //{
        //    // if the key files don't exist..create them:
        //    if (!File.Exists(outputFolder + "\\privateKey.xml"))
        //        GenerateLicenseResources(outputFolder);

        //    // generate each user-license for the current assembly:
        //    int i = 0;
        //    foreach (String userName in userNames)
        //    {
        //        // generate each license file:
        //        GenerateUserLicenseFile(outputFolder, userName, endDates[i++]);
        //    }
        //}

        /// <summary>
        /// generate the public and private key files in the specified folder.
        /// </summary>
        /// <param name="outputFolder"></param>
        public static void GenerateLicenseResources(String outputFolder)
        {
            // create the directory if it doesn't exist:
            if (!Directory.Exists(outputFolder))
            {
                Directory.CreateDirectory(outputFolder);
            }

            // generate the required key files:
            String publicKeyFile = outputFolder + "\\publicKey.xml";
            String privateKeyFile = outputFolder + "\\privateKey.xml";

            // create a new private key:
            String privateKey = GeneratePrivateKey();

            // extract the public part of the key:
            String publicKey = GetPublicKey(privateKey);

            // save them:
            File.WriteAllText(publicKeyFile, publicKey);
            File.WriteAllText(privateKeyFile, privateKey);

        }

        /// <summary>
        /// generate a user-license file.
        /// </summary>
        /// <param name="licenseResourceFolder"></param>
        /// <param name="userName"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        public static bool GenerateUserLicenseFile(String licenseResourceFolder, String productName, String userName, DateTime startDate, DateTime endDate)
        {

            // find and load the private key:
            String privateKeyFile = licenseResourceFolder + "\\privateKey.xml";

            // check the key file exists:
            if (File.Exists(privateKeyFile))
            {
                // load the private key:
                String privateKey = File.ReadAllText(privateKeyFile);

                // generate the license file;
                License license = CreateLicense(
                    startDate,
                    endDate,
                    productName,
                    userName,
                    privateKey
                    );

                // save the license file:
                license.Save(licenseResourceFolder + "\\" + userName + ".lic");

                // success - *.lic file created
                return true;
            }
            else
            {
                // can't find the key-file:
                throw new FileNotFoundException("Can't find private-key file: " + privateKeyFile);
            }
        }

        
        /// <summary>
        /// generate a new, private key. this will be the master key for generating license files.
        /// </summary>
        /// <returns></returns>
        internal static String GeneratePrivateKey()
        {
            DSACryptoServiceProvider dsa = new DSACryptoServiceProvider();
            return dsa.ToXmlString(true);
        }

        /// <summary>
        /// get the public key from a private key. this key must be distributed with the application.
        /// </summary>
        /// <param name="privateKey"></param>
        /// <returns></returns>
        internal static String GetPublicKey(String privateKey)
        {
            DSACryptoServiceProvider dsa = new DSACryptoServiceProvider();
            dsa.FromXmlString(privateKey);
            return dsa.ToXmlString(false);
        }

        /// <summary>
        /// use a private key to generate a secure license file. the private key must match the public key accessible to
        /// the system validating the license.
        /// </summary>
        /// <param name="start">applicable start date for the license file.</param>
        /// <param name="end">applicable end date for the license file</param>
        /// <param name="productName">applicable product name</param>
        /// <param name="userName">user-name</param>
        /// <param name="privateKey">the private key (in XML form)</param>
        /// <returns>secure, public license, validated with the public part of the key</returns>
        internal static License CreateLicense(DateTime start, DateTime end, String productName, String userName, String privateKey)
        {
            // create the licence terms:
            LicenseTerms terms = new LicenseTerms()
            {
                StartDate = start,
                EndDate = end,
                ProductName = productName,
                UserName = userName
            };

            // create the crypto-service provider:
            DSACryptoServiceProvider dsa = new DSACryptoServiceProvider();

            // setup the dsa from the private key:
            dsa.FromXmlString(privateKey);

            // get the byte-array of the licence terms:
            byte[] license = terms.GetLicenseData();

            // get the signature:
            byte[] signature = dsa.SignData(license);

            // now create the license object:
            return new License()
            {
                LicenseTerms = Convert.ToBase64String(license),
                Signature = Convert.ToBase64String(signature)
            };
        }

        /// <summary>
        /// validates the license and if the app should run; if the license is valid the 
        /// method will complete, if not it will throw a security exception.
        /// </summary>
        /// <param name="license">
        /// the license object.
        /// </param>
        /// <exception cref="SecurityException">thrown if the license is invalid or expired</exception>
        /// <returns></returns>
        public static void ValidateLicense(License license, String publicKey, String productName)
        {
            // get the valid terms for the license: (this checks the digital signature on the license file)
            LicenseTerms terms = GetValidTerms(license, publicKey);

            // ensure a valid license-terms object was returned:
            if (terms != null)
            {
                // validate the date-range of the license terms:
                if (DateTime.Now.CompareTo(terms.EndDate) <= 0)
                {
                    if (DateTime.Now.CompareTo(terms.StartDate) >= 0)
                    {
                        // date range is valid... check the product name against the current assembly
                        if (productName == terms.ProductName)
                        {
                            //OK
                            return;
                        }
                        else
                        {
                            // product name doesn't match.
                            throw new SecurityException("Invalid Product Name: " + terms.ProductName);
                        }
                    }
                    else
                    {
                        // license terms not valid yet.
                        throw new SecurityException("License Terms Not Valid Until: " + terms.StartDate.ToShortDateString());
                    }
                }
                else
                {
                    // license terms have expired.
                    throw new SecurityException("License Terms Expired On: " + terms.EndDate.ToShortDateString());
                }
            }
            else
            {
                // the license file was not valid.
                throw new SecurityException("Invalid License File!");
            }
        }

        /// <summary>
        /// validate license file and return the license terms.
        /// </summary>
        /// <param name="license"></param>
        /// <param name="publicKey"></param>
        /// <returns></returns>
        internal static LicenseTerms GetValidTerms(License license, String publicKey)
        {
            // create the crypto-service provider:
            DSACryptoServiceProvider dsa = new DSACryptoServiceProvider();

            // setup the provider from the public key:
            dsa.FromXmlString(publicKey);

            // get the license terms data:
            byte[] terms = Convert.FromBase64String(license.LicenseTerms);

            // get the signature data:
            byte[] signature = Convert.FromBase64String(license.Signature);

            // verify that the license-terms match the signature data
            if (dsa.VerifyData(terms, signature))
                return LicenseTerms.FromString(license.LicenseTerms);
            else
                throw new SecurityException("Signature Not Verified!");
        }
    }
}
