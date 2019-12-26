using System;
using System.IO;
using LicensingLibrary.General.IO;

namespace LicensingLibrary.General.Security
{
    /// <summary>
    /// the license agreement for the software.
    /// embedded are the license terms (ie start and end dates) and a digital signature used to verify the 
    /// the license terms. this way, the consumer may be able to see what the license terms are, but if they attempt to change them
    /// (in order to extend thier license) then they will not be able to generate a matching signature.
    /// </summary>
    public class License
    {
        #region Properties

        /// <summary>
        /// the license terms. obscured.
        /// </summary>
        public string LicenseTerms { get; set; }

        /// <summary>
        /// the signature.
        /// </summary>
        public string Signature { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// saves the license to an xml file.
        /// </summary>
        /// <param name="fileName"></param>
        public void Save(String fileName)
        {
            Serializer.Save<License>(this, fileName);
        }

        /// <summary>
        /// saves the license to a stream as xml.
        /// </summary>
        /// <param name="stream"></param>
        public void Save(Stream stream)
        {
            Serializer.Save<License>(this, stream);
        }

        /// <summary>
        /// create a license object from a license file.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static License Load(String fileName)
        {
            // read the filename:
            return Serializer.Load<License>(new FileInfo(fileName));
        }

        /// <summary>
        /// load a license from stream xml data.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static License Load(Stream data)
        {
            // read the data stream:
            return Serializer.Load<License>(data);
        }

        #endregion
    }
}
