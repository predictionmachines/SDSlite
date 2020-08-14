using Microsoft.Research.Science.Data;
using Microsoft.Research.Science.Data.CSV;
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
    }
}
