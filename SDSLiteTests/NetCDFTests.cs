using Microsoft.Research.Science.Data;
using Microsoft.Research.Science.Data.NetCDF4;
using NUnit.Framework;
using System;
using System.Linq;

namespace SDSLiteTests
{
    public class NetCDFTests: GenericFileTests
    {
        protected override DataSetUri dsUri(string fileName, params object[] attributes)
        {
            var dsUri = new NetCDFUri()
            {
                FileName = fileName,
            };
            if (attributes != null) {
                dsUri.Deflate = attributes
                    .Select(a => a as DeflateLevel?)
                    .FirstOrDefault(d => d != null) ?? DeflateLevel.Normal;
            }
            return dsUri;
        }

        [Test]
        public void Deflate_reduces_file_size()
        {
            var data = Enumerable.Repeat(Math.PI, 2048).ToArray();
            var len_uncompressed = Empty_AddVariable(data, DeflateLevel.Store);
            var len_compressed = Empty_AddVariable(data, DeflateLevel.Best);
            Assert.IsTrue(len_compressed < len_uncompressed);
        }

        [Test]
        public void Test_inq_libvers()
        {
            var ver = NetCDFInterop.NetCDF.nc_inq_libvers();
            Assert.IsNotNull(ver);
            Assert.IsTrue(ver.StartsWith("4."));
        }
    }
}
