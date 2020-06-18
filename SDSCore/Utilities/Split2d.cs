using System;
using System.Windows;
using System.Collections.Generic;
using System.Diagnostics;

namespace Microsoft.Research.Science.Data.Utilities
{
    public static class Split2d
    {
        public static Point[][] SplitFirstDimension(Variable variable)
        {
            if (variable == null)
                throw new ArgumentNullException("variable");
            if (variable.Rank != 2)
                throw new ArgumentException("Only 2d variables are supported");
            int n = variable.GetShape()[0];
            int m = variable.GetShape()[1];
            Array d = variable.GetData();
            Point[][] pts = new Point[n][];

            for (int i = 0; i < n; i++)
            {
                List<Point> tmp = new List<Point>(m);
                if (variable.TypeOfData == typeof(float))
                {
                    float[,] dt = (float[,])d;
                    object mvObj = variable.GetMissingValue();
                    if (mvObj == null)
                    {
                        for (int j = 0; j < m; j++)
                            if (!Double.IsNaN(dt[i, j]))
                                tmp.Add(new Point(j, dt[i, j]));
                    }
                    else
                    {
                        float mv;
                        try
                        {
                            mv = Convert.ToSingle(mvObj);
                        }
                        catch (Exception exc)
                        {
                            Trace.WriteLine("PolyPolyline: cannot convert missing value attribute to Double: " + exc.Message);
                            mv = float.NaN;
                        }
                        for (int j = 0; j < m; j++)
                            if (dt[i, j] != mv)
                                tmp.Add(new Point(j, dt[i, j]));
                    }
                }
                else if (variable.TypeOfData == typeof(double))
                {
                    double[,] dt = (double[,])d;
                    object mvObj = variable.GetMissingValue();
                    if (mvObj == null)
                    {
                        for (int j = 0; j < m; j++)
                            if (!Double.IsNaN(dt[i, j]))
                                tmp.Add(new Point(j, dt[i, j]));
                    }
                    else
                    {
                        double mv = Convert.ToDouble(mvObj);
                        try
                        {
                            mv = Convert.ToDouble(mvObj);
                        }
                        catch (Exception exc)
                        {
                            Trace.WriteLine("PolyPolyline: cannot convert missing value attribute to double: " + exc.Message);
                            mv = double.NaN;
                        }
                        for (int j = 0; j < m; j++)
                            if (dt[i, j] != mv)
                                tmp.Add(new Point(j, dt[i, j]));
                    }
                }
                else
                {
                    object mv = variable.GetMissingValue();
                    for (int j = 0; j < m; j++)
                    {
                        object obj = d.GetValue(i, j);
                        if (!Object.Equals(obj, mv))
                            tmp.Add(new Point(j, Convert.ToDouble(obj)));
                    }
                }
                pts[i] = tmp.ToArray();
            }
            return pts;
        }

        public static Point[][] SplitSecondDimension(Variable variable)
        {
            if (variable == null)
                throw new ArgumentNullException("variable");
            if (variable.Rank != 2)
                throw new ArgumentException("Only 2d variables are supported");
            int n = variable.GetShape()[1];
            int m = variable.GetShape()[0];
            Array d = variable.GetData();
            Point[][] pts = new Point[n][];

            for (int i = 0; i < n; i++)
            {
                List<Point> tmp = new List<Point>(m);
                if (variable.TypeOfData == typeof(float))
                {
                    float[,] dt = (float[,])d;
                    object mvObj = variable.GetMissingValue();
                    if (mvObj == null)
                    {
                        for (int j = 0; j < m; j++)
                            if (!Double.IsNaN(dt[j, i]))
                                tmp.Add(new Point(j, dt[j, i]));
                    }
                    else
                    {
                        float mv;
                        try
                        {
                            mv = Convert.ToSingle(mvObj);
                        }
                        catch (Exception exc)
                        {
                            Trace.WriteLine("PolyPolyline: cannot convert missing value attribute to float: " + exc.Message);
                            mv = float.NaN;
                        }
                        for (int j = 0; j < m; j++)
                            if (dt[j, i] != mv)
                                tmp.Add(new Point(j, dt[j, i]));
                    }
                }
                else if (variable.TypeOfData == typeof(double))
                {
                    double[,] dt = (double[,])d;

                    object mvObj = variable.GetMissingValue();
                    if (mvObj == null)
                    {
                        for (int j = 0; j < m; j++)
                            if (!Double.IsNaN(dt[j, i]))
                                tmp.Add(new Point(j, dt[j, i]));
                    }
                    else
                    {
                        double mv;
                        try
                        {
                            mv = Convert.ToDouble(mvObj);
                        }
                        catch (Exception exc)
                        {
                            Trace.WriteLine("PolyPolyline: cannot convert missing value attribute to double: " + exc.Message);
                            mv = double.NaN;
                        }
                        for (int j = 0; j < m; j++)
                            if (dt[j, i] != mv)
                                tmp.Add(new Point(j, dt[j, i]));
                    }
                }
                else
                {
                    object mv = variable.GetMissingValue();
                    for (int j = 0; j < m; j++)
                    {
                        object obj = d.GetValue(j, i);
                        if (!Object.Equals(obj, mv))
                            tmp.Add(new Point(j, Convert.ToDouble(obj)));
                    }
                }
                pts[i] = tmp.ToArray();
            }
            return pts;
        }
    }
}