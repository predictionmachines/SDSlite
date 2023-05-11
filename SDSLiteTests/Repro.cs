using Microsoft.Research.Science.Data;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDSLiteTests
{
    public class Repro_28
    {
        // Repro issue #28
        [Test]
        public void CanReadNetcdfNcChar()
        {
            var ncpath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".nc");
            File.WriteAllBytes(ncpath, Convert.FromBase64String(netcdf_content));
            try
            {
                using var ds = DataSet.Open(ncpath, ResourceOpenMode.Open);
                Assert.AreEqual(1, ds.Variables.Count);
                Assert.AreEqual(2, ds.Dimensions.Count);
                Assert.AreEqual("test", ds.Variables[0].Name);
                Assert.AreEqual("count", ds.Dimensions[0].Name);
                Assert.AreEqual(1, ds.Dimensions[0].Length);
                Assert.AreEqual("size", ds.Dimensions[1].Name);
                Assert.AreEqual(8, ds.Dimensions[1].Length);
                var data = ds[0].GetData();
                Assert.AreEqual(2, data.Rank);
                Assert.AreEqual("value   ", Encoding.UTF8.GetString((data.Cast<byte>().ToArray())));
                ds[0].Append(Encoding.UTF8.GetBytes("addition"));
                Assert.AreEqual(2, ds.Dimensions[0].Length);
                data = ds[0].GetData();
                Assert.AreEqual("value   addition", Encoding.UTF8.GetString((data.Cast<byte>().ToArray())));
            }
            finally
            {
                File.Delete(ncpath);
            }
        }
        // [1] test of type SByte (count:1) (size:8)
        const string netcdf_content = @"
Q0RGAQAAAAEAAAAKAAAAAgAAAARzaXplAAAACAAAAAVjb3VudAAAAAAAAAAAAAAAAAAAAAAAAAsA
AAABAAAABHRlc3QAAAACAAAAAQAAAAAAAAAAAAAAAAAAAAIAAAAIAAAAZHZhbHVlICAg";
    }

    public class Repro_38
    {
        // Repro issue #38
        [Test]
        public void CsvAddVariableNoMissingValue()
        {
            var csvPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".csv");
            try
            {
                using (var sdsout = DataSet.Open("out.csv", ResourceOpenMode.Create))
                {
                    sdsout.AddVariable<double>("a", "a");
                }

            }
            finally
            {
                File.Delete(csvPath);
            }
        }
    }
}
