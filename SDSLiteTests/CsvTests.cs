using Microsoft.Research.Science.Data;
using Microsoft.Research.Science.Data.CSV;
using Microsoft.Research.Science.Data.Utilities;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SdsLiteTests
{
    public class CsvTests
    {
        [Test]
        public void NoMissingValueAttribute()
        {
            var fn = Path.GetTempFileName();
            File.WriteAllText(fn, "a\n0"); // minimum viable csv
            var dsuri = new CsvUri();
            dsuri.FileName = fn;
            dsuri.OpenMode = ResourceOpenMode.ReadOnly;
            var ds = DataSet.Open(dsuri);
            Assert.AreEqual(1, ds.Variables.Count);
            Assert.IsTrue(ds.Variables[0].Metadata.ContainsKey("Name"));
            Assert.IsTrue(ds.Variables[0].Metadata.ContainsKey("DisplayName"));
            Assert.IsTrue(ds.Variables[0].Metadata.ContainsKey("csv_column"));
            Assert.AreEqual(3, ds.Variables[0].Metadata.Count);
        }

        [Test]
        public void InferMissingValueAttribute()
        {
            var fn = Path.GetTempFileName();
            File.WriteAllText(fn, "t,u\n0,first\n,second\n1,\n2,last");
            var dsuri = new CsvUri();
            dsuri.FileName = fn;
            dsuri.OpenMode = ResourceOpenMode.ReadOnly;
            dsuri.FillUpMissingValues = true; // this generates MissingValue attributes
            var ds = DataSet.Open(dsuri);
            Assert.AreEqual(2, ds.Variables.Count);
            {
                var v = (Variable<double>)ds.Variables["t"];
                Assert.IsTrue(v.Metadata.ContainsKey("Name"));
                Assert.IsTrue(v.Metadata.ContainsKey("DisplayName"));
                Assert.IsTrue(v.Metadata.ContainsKey("csv_column"));
                Assert.IsTrue(v.Metadata.ContainsKey(v.Metadata.KeyForMissingValue));
                Assert.AreEqual(4, v.Metadata.Count);
                Assert.IsTrue(double.IsNaN((double)v.MissingValue));
                Assert.IsTrue(double.IsNaN(v[1]));
            }
            {
                var v = (Variable<string>)ds.Variables["u"];
                Assert.IsTrue(v.Metadata.ContainsKey("Name"));
                Assert.IsTrue(v.Metadata.ContainsKey("DisplayName"));
                Assert.IsTrue(v.Metadata.ContainsKey("csv_column"));
                Assert.IsTrue(v.Metadata.ContainsKey(v.Metadata.KeyForMissingValue));
                Assert.AreEqual(4, v.Metadata.Count);
                Assert.IsTrue(v.MissingValue == null);
                Assert.IsNull(v[2]);
            }
        }
    }
}
