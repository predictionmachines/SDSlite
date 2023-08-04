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

        [Test]
        public void TestCreateFixedDimensionNC()
        {
            // create temporary file.
            string file = System.IO.Path.GetRandomFileName() + ".nc";
            var dataset = (NetCDFDataSet)DataSet.Open(file);

            dataset.CreateDimension("lat", 3);
            dataset.CreateDimension("lon", 2);


            var lat = dataset.AddVariable<double>("lat", new double[] { 40, 45, 50 }, "lat");
            lat.Metadata["long_name"] = "Latitude";
            lat.Metadata["actual_range"] = "40.0f, 50.0f";
            lat.Metadata["standard_name"] = "latitude";
            lat.Metadata["units"] = "degrees_north";
            lat.Metadata["axis"] = "Y";


            var lon = dataset.AddVariable<double>("lon", new double[] { 130, 140 }, "lon");
            lon.Metadata["long_name"] = "Longitude";
            lon.Metadata["actual_range"] = "130.0f, 140.0f";
            lon.Metadata["units"] = "degrees_east";
            lon.Metadata["standard_name"] = "longitude";
            lon.Metadata["axis"] = "X";

            dataset.Commit();

            var value = dataset.AddVariable<double>("value", "lat", "lon");
            value.Metadata["units"] = "m s-1";
            value.Metadata["original_units"] = "m s-1";
            value.Metadata["standard_name"] = "val";
            value.MissingValue = -1.0;

            dataset.Variables["value"].PutData(new double[,] { { 1, 2 }, { 3, 4 }, { 5, 6 } });
            dataset.Dispose();
            System.IO.File.Delete(file);
        }
    }
}
