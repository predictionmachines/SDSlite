using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Research.Science.Data;
using System.IO;
using System.Globalization;

namespace  Microsoft.Research.Science.Data.Climate
{
    /// <summary>
    /// Provides static methods for REST API implementation.
    /// </summary>
    public static class RestApiUtilities
    {
        private static readonly string tempFolder = System.IO.Path.Combine(Path.GetTempPath(), "FetchClimateTemp\\");

        static RestApiUtilities()
        {
            if (!Directory.Exists(RestApiUtilities.tempFolder))
                Directory.CreateDirectory(RestApiUtilities.tempFolder);
        }

        /// <summary>
        /// Returns new name for temporary csv file.
        /// </summary>
        /// <returns>New name for temporary csv file.</returns>
        public static string GetTempCsvFileName()
        {
            return Path.Combine(RestApiUtilities.tempFolder, Path.GetRandomFileName().Replace(".", "2") + ".csv");
        }

        /// <summary>
        /// Returns bytes of specified <see cref="DataSet"/> instance after converting it into <see cref="CsvDataSet"/>.
        /// </summary>
        /// <param name="ds">Input <see cref="DataSet"/> instance.</param>
        /// <returns>Resulting bytes array.</returns>
        public static byte[] GetCsvBytes(DataSet ds)
        {
            if (ds == null)
            {
                throw new ArgumentNullException("ds");
            }
            string resultTempFileName = GetTempCsvFileName();
            byte[] bytes = null;

            try
            {
                ds.Clone(String.Format(CultureInfo.InvariantCulture, "msds:csv?file={0}&openMode=create&appendMetadata=true", resultTempFileName));
                bytes = File.ReadAllBytes(resultTempFileName);
            }
            finally
            {
                if (resultTempFileName != null)
                    File.Delete(resultTempFileName);
            }

            return bytes;
        }

        /// <summary>
        /// Returns new <see cref="MemoryDataSet"/> instance from bytes of <see cref="CsvDataSet"/> instance.
        /// </summary>
        /// <param name="bytes">Input bytes array.</param>
        /// <returns>Resulting <see cref="MemoryDataSet"/> instance.</returns>
        public static DataSet GetDataSet(byte[] bytes)
        {
            string tempFile = GetTempCsvFileName();

            try
            {
                File.WriteAllBytes(tempFile, bytes);

                using (DataSet csvDs = DataSet.Open(String.Format(CultureInfo.InvariantCulture, "msds:csv?file={0}&openMode=open&appendMetadata=true", tempFile)))
                {
                    DataSet memoryDs = csvDs.Clone("msds:memory");
                    File.Delete(tempFile);
                    tempFile = null;
                    return memoryDs;
                }
            }
            finally
            {
                if (tempFile != null)
                {
                    File.Delete(tempFile);
                    tempFile = null;
                }
            }
        }

        /// <summary>
        /// Reads specified number of bytes from stream.
        /// </summary>
        /// <param name="stream"><see cref="Stream"/>, to read from.</param>
        /// <param name="length">Number of bytes to read.</param>
        /// <returns>Readen bytes array.</returns>
        public static byte[] ReadBytes(Stream stream, int length)
        {
            byte[] buffer = new byte[length];
            int totalBytesRead = 0;
            while (totalBytesRead < length)
            {
                int bytesRead = stream.Read(buffer, totalBytesRead, length - totalBytesRead);
                totalBytesRead += bytesRead;
            }

            return buffer;
        }
    }
}
