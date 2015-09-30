using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Research.Science.Data.NetCDF4
{
    internal static class ChunkSizeSelector 
    {
        public const int suggestedChunkPower = 13; // 8 Kb
        public const int maxAllowedChunkPower = 17; // 128 Kb

        public static int GetChunkSize(Type type, int rank)
        {
            int sizeBytes = GetSizeOfType(type);
            int s = Log2m((uint)sizeBytes);
            for (int n = suggestedChunkPower; n <= maxAllowedChunkPower; n++)
            {
                if (n <= s) continue;
                int p = (n - s) / rank;
                if (p * rank == n - s) return Pow(p);
            }
            for (int n = suggestedChunkPower - 1; n > s; n--)
            {
                int p = (n - s) / rank;
                if (p * rank == n - s) return Pow(p);
            }
            return 1;
        }


        static int Pow(int p)
        {
            return 1 << p;
        }

        static int Log2m(uint n)
        {
            if (n < 0) throw new ArgumentException("n must be positive");
            if (n == 1) return 0;
            int log = 32; // max
            while ((n & 0x80000000) == 0)
            {
                n <<= 1;
                log--;
            }
            if ((n ^ 0x80000000) == 0) log--;
            return log;
        }

        private static int GetSizeOfType(Type type)
        {
            if (type == typeof(Double))
                return sizeof(Double);
            else if (type == typeof(Single))
                return sizeof(Single);
            else if (type == typeof(Int16))
                return sizeof(Int16);
            else if (type == typeof(Int32))
                return sizeof(Int32);
            else if (type == typeof(Int64))
                return sizeof(Int64);
            else if (type == typeof(UInt64))
                return sizeof(UInt64);
            else if (type == typeof(UInt32))
                return sizeof(UInt32);
            else if (type == typeof(UInt16))
                return sizeof(UInt16);
            else if (type == typeof(Byte))
                return sizeof(Byte);
            else if (type == typeof(SByte))
                return sizeof(SByte);
            else if (type == typeof(String))
                return 32;
            else if (type == typeof(DateTime))
                return sizeof(double);
            else if (type == typeof(Boolean))
                return sizeof(byte);
            return 1;
        }
    }
}
