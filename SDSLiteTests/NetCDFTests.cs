using System;
using NUnit.Framework;

using Microsoft.Research.Science.Data;
using System.Linq;

namespace SDSLiteTests
{
    public class NetCDFTests
    {
        public void Empty_AddVariable<T>(T[] data)
        {
            var fn = System.IO.Path.GetTempFileName();
            try
            {
                var dsuri = new Microsoft.Research.Science.Data.NetCDF4.NetCDFUri();
                dsuri.FileName = fn;
                dsuri.OpenMode = ResourceOpenMode.Create;
                var vname = "a";
                using (var ds = DataSet.Open(dsuri))
                {
                    ds.AddVariable(typeof(T), vname, data, "a");
                    ds.Commit();
                }
                dsuri.OpenMode = ResourceOpenMode.ReadOnly;
                using (var ds = DataSet.Open(dsuri))
                {
                    Assert.IsTrue(ds.Variables.Contains(vname));
                    var v = ds[vname];
                    Assert.AreEqual(1, v.Rank);
                    Assert.AreEqual(typeof(T), v.TypeOfData);
                    var d = v.GetData() as T[];
                    Assert.IsNotNull(d);
                    Assert.AreEqual(data.Length, d.Length);
                    for (int i = 0; i < data.Length; i++)
                        Assert.AreEqual(data[i], d[i], String.Format("index={0}", i));
                }

            }
            finally
            {
                System.IO.File.Delete(fn);
            }
        }
        [Test]
        public void Empty_AddVariable_double()
        {
            Empty_AddVariable<double>(new double[]{ -1.0, 0.0, 3.0, Math.PI, Double.Epsilon, Double.PositiveInfinity });
        }
        [Test]
        public void Empty_AddVariable_float()
        {
            Empty_AddVariable<float>(new float[] { -1.0f, 0.0f, 3.0f, 4.0f, Single.Epsilon, Single.NegativeInfinity });
        }
        [Test]
        public void Empty_AddVariable_int()
        {
            Empty_AddVariable<int>(new int[] { 1, 0, Int32.MinValue, Int32.MaxValue });
        }
        [Test]
        public void Empty_AddVariable_string()
        {
            Empty_AddVariable(new string[] { "string1", "", null, new String(Enumerable.Repeat('x',4096).ToArray()),"line1\r\nline2", "English Русский" });
        }
        //[Test]
        //public void Empty_AddVariable_string()
        //{
        //    Empty_AddVariable<string>(new string[] { "a", "", "b"});
        //}
        public void Empty_AddAttribute<T>(T data)
        {
            var fn = System.IO.Path.GetTempFileName();
            try
            {
                var dsuri = new Microsoft.Research.Science.Data.NetCDF4.NetCDFUri();
                dsuri.FileName = fn;
                dsuri.OpenMode = ResourceOpenMode.Create;
                var vname = "a";
                using (var ds = DataSet.Open(dsuri))
                {
                    ds.Metadata[vname]=data;
                }
                dsuri.OpenMode = ResourceOpenMode.ReadOnly;
                using (var ds = DataSet.Open(dsuri))
                {
                    Assert.IsTrue(ds.Metadata.ContainsKey(vname));
                    var v = ds.Metadata[vname];
                    Assert.IsNotNull(v);
                    Assert.AreEqual(typeof(T), v.GetType());
                    if (typeof(T).IsArray) {
                        var darr = data as Array;
                        var varr = v as Array;
                        for (int i = 0; i < darr.Length; i++)
                            Assert.AreEqual(darr.GetValue(i), varr.GetValue(i), String.Format("index={0}", i));
                    }
                    else
                        Assert.AreEqual(data, v);
                }

            }
            finally
            {
                System.IO.File.Delete(fn);
            }
        }
        [Test]
        public void Empty_AddAttribute_double()
        {
            Empty_AddAttribute(Math.PI);
        }
        [Test]
        public void Empty_AddAttribute_double0()
        {
            Empty_AddAttribute(Math.PI);
        }
        [Test]
        public void Empty_AddAttribute_double1()
        {
            Empty_AddAttribute(new double[]{Math.PI});
        }
        [Test]
        public void Empty_AddAttribute_double2()
        {
            Empty_AddAttribute(new double[]{0.0,1.0});
        }
        [Test]
        public void Empty_AddAttribute_float()
        {
            Empty_AddAttribute(0.0f);
        }
        [Test]
        public void Empty_AddAttribute_int()
        {
            Empty_AddAttribute(Int32.MinValue);
        }
        [Test]
        public void Empty_AddAttribute_uint()
        {
            Empty_AddAttribute(UInt32.MaxValue);
        }
        [Test]
        public void Empty_AddAttribute_string()
        {
            Empty_AddAttribute("string");
        }
        [Test]
        public void Empty_AddAttribute_string_empty()
        {
            Empty_AddAttribute("");
        }
        [Test]
        public void Empty_AddAttribute_string0()
        {
            Empty_AddAttribute(new string[]{});
        }
        [Test]
        public void Empty_AddAttribute_string1()
        {
            Empty_AddAttribute(new string[] { "string" });
        }
        [Test]
        public void Empty_AddAttribute_string2()
        {
            Empty_AddAttribute(new string[] { "", "string", "English, Русский" });
        }
        [Test]
        public void Empty_AddAttribute_UtfString()
        {
            Empty_AddAttribute("Русский");
        }

        private void CheckGroupCreating(string groupName, string parentGroupName = null)
        {
            Tuple<Type, string, Array>[] variableInfo =
            {
                new Tuple<Type, string, Array>(typeof(float), "testVar1", new float[] { 42.0f, 73.0f, 1.0f, 0.0f }),
                new Tuple<Type, string, Array>(typeof(int), "testVar2", new int[] { 4, 8, 15, 16 }),
                new Tuple<Type, string, Array>(typeof(string), "testVar3", new string[] { "str", "rts", "23", "---"})
            };

            string dimensionName = "testDim1";
            string tmpFileName = System.IO.Path.GetTempFileName() + ".nc";

            // create a test dataset group and create variables and dimensions, that this group must contain
            using (var ds = DataSet.Open(tmpFileName + "?openMode=create&groupName=" + groupName))
            {
                foreach(var info in variableInfo)
                {
                    ds.AddVariable(info.Item1, info.Item2, info.Item3, dimensionName);
                }

                ds.Commit();
            }

            // check that group contains variables and dimensions
            using (var ds = DataSet.Open(tmpFileName + "?openMode=readOnly&groupName=" + groupName))
            {
                foreach(var info in variableInfo)
                {
                    Assert.True(ds.Variables.Contains(info.Item2));
                    var vals = ds.Variables[info.Item2].GetData();
                    for(int i = 0; i < vals.Length; i++)
                    {
                        Assert.Equals(info.Item3.GetValue(i), vals.GetValue(i));
                    }
                }

                Assert.True(ds.Dimensions.Contains(dimensionName));
            }

            // if the given group is not the root group
            if (!string.IsNullOrEmpty(groupName) && !groupName.Equals("/"))
            {
                // check that the root group doesn't contain variables and dimensions from the non-root group
                using (var ds = DataSet.Open(tmpFileName + "?openMode=readOnly"))
                {
                    foreach (var info in variableInfo)
                    {
                        Assert.False(ds.Variables.Contains(info.Item2));
                    }

                    Assert.False(ds.Dimensions.Contains(dimensionName));
                }
            }

            // if the parent group is given
            if(!string.IsNullOrEmpty(parentGroupName))
            { 
                // check that parent group doesn't contain variables and dimensions from the child group
                using (var ds = DataSet.Open(tmpFileName + "?openMode=readOnly&groupName=" + parentGroupName))
                {
                    foreach(var info in variableInfo)
                    {
                        Assert.False(ds.Variables.Contains(info.Item2));
                    }

                    Assert.False(ds.Dimensions.Contains(dimensionName));
                }
            }

            System.IO.File.Delete(tmpFileName);
        }

        [Test]
        public void RootGroupTest1()
        {
            CheckGroupCreating("");
        }
        [Test]
        public void RootGroupTest2()
        {
            CheckGroupCreating("/");
        }
        [Test]
        public void SimpleGroupTest()
        {
            CheckGroupCreating("testGroup");
        }
        [Test]
        public void CheckParentGroupTest()
        {
            CheckGroupCreating("testGroup", "/");
        }
        [Test]
        public void NestedGroupTest1()
        {
            CheckGroupCreating("parent/child", "parent");
        }
        [Test]
        public void NestedGroupTest2()
        {
            CheckGroupCreating("parent/child/subchild", "parent/child");
        }
        [Test]
        public void GroupNotFoundTest()
        {
            string tmpFileName = System.IO.Path.GetTempFileName() + ".nc";
            string rightTestGroup = "testGrp";
            string wrongTestGroup = "testGrp/childGrp";

            using (var ds = DataSet.Open(tmpFileName + "?openMode=create&groupName=" + rightTestGroup))
            {
            }

            try
            {
                Assert.Throws<DataSetCreateException>(() =>
                {
                    using (var ds = DataSet.Open(tmpFileName + "?openMode=readOnly&groupName=" + wrongTestGroup))
                    {
                    }
                });
            }
            finally
            {
                System.IO.File.Delete(tmpFileName);
            }
        }
        [Test]
        public void DifferentWaysToDefineGroupTest()
        {
            string tmpFileName = System.IO.Path.GetTempFileName() + ".nc";
            string groupName = "group/subgroup";
            string testVarName = "var";
            string dimensionName = "dimension";

            using (var ds = DataSet.Open(tmpFileName + "?openMode=create&groupName=" + groupName))
            {
                ds.AddVariable(typeof(int), testVarName, new int[] { 42 }, dimensionName);
                ds.Commit();
            }

            using (var ds = DataSet.Open(tmpFileName + "?openMode=readOnly&groupName=/" + groupName))
            {
                Assert.True(ds.Variables.Contains(testVarName));
                Assert.True(ds.Dimensions.Contains(dimensionName));
            }

            using (var ds = DataSet.Open(tmpFileName + "?openMode=readOnly&groupName=" + groupName + "/"))
            {
                Assert.True(ds.Variables.Contains(testVarName));
                Assert.True(ds.Dimensions.Contains(dimensionName));
            }

            using (var ds = DataSet.Open(tmpFileName + "?openMode=readOnly&groupName=/" + groupName + "/"))
            {
                Assert.True(ds.Variables.Contains(testVarName));
                Assert.True(ds.Dimensions.Contains(dimensionName));
            }

            System.IO.File.Delete(tmpFileName);
        }
    }
}
