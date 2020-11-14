// Copyright © Microsoft Corporation, All Rights Reserved.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Research.Science.Data
{
    internal static class Interpolation
    {
        public static double Interpolate(double x, double x1, double x2, double a, double b)
        {
            return a + (b - a) * (x - x1) / (x2 - x1);
        }

        public static object Interpolate(object x, object x1, object x2, object a, object b)
        {
            return (double)a + ((double)b - (double)a) * ((double)x - (double)x1) / ((double)x2 - (double)x1);
        }
    }
}

