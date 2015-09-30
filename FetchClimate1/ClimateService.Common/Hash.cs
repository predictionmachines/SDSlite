using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.IO;
using Microsoft.Research.Science.Data.Climate.Conventions;

namespace Microsoft.Research.Science.Data.Climate
{    

    public class ShaHash
    {
        static HashAlgorithm sha = new SHA1CryptoServiceProvider();
        

        public static string HashStreamToHexString(Stream stream)
        {
            lock ("ShaHash")
            {
                return ByteArrayToHexStr(sha.ComputeHash(stream));
            }
        }        

        public static byte[] HexStrToByteArray(string str)
        {
            byte[] res = new byte[str.Length / 2];
            for (int i = 0; i < str.Length / 2; i++)
            {
                string b = str.Substring(i * 2, 2);
                res[i] = Convert.ToByte(b, 16);
            }
            return res;
        }

        public static string ByteArrayToHexStr(byte[] data)
        {
            string str = string.Empty;
            for (int i = 0; i < data.Length; i++)
                str += Convert.ToString(data[i], 16);
            return str;
        }

        public static string CalculateShaHash(DataSet ds)
        {
            int[] yearsMin = (int[])ds.Variables[Namings.VarNameYearMin].GetData();
            int[] yearsMax = (int[])ds.Variables[Namings.VarNameYearMax].GetData();

            int[] daysMin = (int[])ds.Variables[Namings.VarNameDayMin].GetData();
            int[] daysMax = (int[])ds.Variables[Namings.VarNameDayMax].GetData();

            int[] hoursMin = (int[])ds.Variables[Namings.VarNameHourMin].GetData();
            int[] hoursMax = (int[])ds.Variables[Namings.VarNameHourMax].GetData();

            double[] latsMin = (double[])ds.Variables[Namings.VarNameLatMin].GetData();
            double[] latsMax = (double[])ds.Variables[Namings.VarNameLatMax].GetData();

            double[] lonsMin = (double[])ds.Variables[Namings.VarNameLonMin].GetData();
            double[] lonsMax = (double[])ds.Variables[Namings.VarNameLonMax].GetData();

            int cellsCount = ds.Dimensions[Namings.dimNameCells].Length;

            string metadataNameProvenanceHint = (string)ds.Metadata[Namings.metadataNameProvenanceHint];
            string metadataNameParameter = (string)ds.Metadata[Namings.metadataNameParameter];
            string metadataNameCoverage = (string)ds.Metadata[Namings.metadataNameCoverage];

            MemoryStream memStm = new MemoryStream();
            using (BinaryWriter writer = new BinaryWriter(memStm))
            {
                writer.Write(metadataNameProvenanceHint);
                writer.Write(metadataNameParameter);
                writer.Write(metadataNameCoverage);
                for (int i = 0; i < cellsCount; i++)
                {
                    writer.Write(yearsMin[i]);
                    writer.Write(yearsMax[i]);
                    writer.Write(daysMin[i]);
                    writer.Write(daysMax[i]);
                    writer.Write(hoursMin[i]);
                    writer.Write(hoursMax[i]);
                    writer.Write(latsMin[i]);
                    writer.Write(latsMax[i]);
                    writer.Write(lonsMin[i]);
                    writer.Write(lonsMax[i]);
                }
                writer.Flush();
                writer.BaseStream.Seek(0, SeekOrigin.Begin);
                return ShaHash.HashStreamToHexString(memStm);
            }
        }
    }
}
