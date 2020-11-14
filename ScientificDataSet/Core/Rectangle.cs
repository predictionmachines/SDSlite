// Copyright © Microsoft Corporation, All Rights Reserved.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Research.Science.Data
{
	/// <summary>
	/// Represents multidimensional integer rectangle.
	/// </summary>
	public struct Rectangle
	{
		private int[] origin;
		private int[] shape;

		/// <summary>
		/// Initialies an instance of the class.
		/// </summary>
		/// <param name="origin"></param>
		/// <param name="shape"></param>
		/// <remarks>
		/// <paramref name="origin"/> and <paramref name="shape"/> cannot be null.
		/// </remarks>
		public Rectangle(int[] origin, int[] shape)
		{
			if (origin == null && shape != null)
				throw new ArgumentNullException("origin");
			if (shape == null && origin != null)
				throw new ArgumentNullException("shape");
			if (origin != null && shape.Length != origin.Length)
				throw new ArgumentException("Shape length is not equal to origin's length.");

			this.origin = origin;
			this.shape = shape;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="origin"></param>
		/// <param name="shape"></param>
		public Rectangle(int origin, int shape)
		{
			this.origin = new int[] { origin };
			this.shape = new int[] { shape };
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="rank"></param>
		public Rectangle(int rank)
		{
			this.origin = new int[rank];
			this.shape = new int[rank];
		}
		/// <summary>
		/// Gets the value indicating whether the rectangle is empty or not.
		/// </summary>
		/// <remarks>
		/// <para>
		/// Rectangle is empty if its shape is null or contains only zeros.
		/// </para>
		/// </remarks>
		public bool IsEmpty
		{
			get
			{
				return (shape == null || Array.TrueForAll(shape, s => s == 0));
			}
		}
		/// <summary>
		/// 
		/// </summary>
		public int[] Origin
		{
			get { return origin; }
		}
		/// <summary>
		/// 
		/// </summary>
		public int[] Shape
		{
			get { return shape; }
		}
		/// <summary>
		/// 
		/// </summary>
		public int Rank
		{
			get { return origin.Length; }
		}
		/// <summary>
		/// Combines two rectangle returning the mimimal rectangle containg both rectangles.
		/// </summary>
		/// <param name="r"></param>
		/// <exception cref="ArgumentException">Different ranks.</exception>
		public void CombineWith(Rectangle r)
		{
			if (Rank != r.Rank)
				throw new ArgumentException("Rectangles are defined in spaces with different ranks.");

			for (int i = 0; i < origin.Length; i++)
			{
				int max = shape[i] + origin[i];

				origin[i] = Math.Min(origin[i], r.origin[i]);

				if (max < r.origin[i] + r.shape[i])
					max = r.origin[i] + r.shape[i];
				shape[i] = max - origin[i];
			}
		}
		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			if (IsEmpty)
			{
				sb.Append("Empty");
			}
			else
			{
				sb.Append("Origin:");
				for (int i = 0; i < origin.Length; i++)
					sb.AppendFormat(" {0}", origin[i]);

				sb.Append(", shape:");
				for (int i = 0; i < shape.Length; i++)
					sb.AppendFormat(" {0}", shape[i]);
			}
			return sb.ToString();
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public override bool Equals(object obj)
		{
			if (obj == null || GetType() != obj.GetType())
			{
				return false;
			}

			Rectangle r = (Rectangle)obj;
			if (r.shape.Length != shape.Length)
				return false;
			for (int i = 0; i < shape.Length; i++)
			{
				if (shape[i] != r.shape[i]) return false;
				if (origin[i] != r.origin[i]) return false;
			}
			return true;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public override int GetHashCode()
		{
			int hash = 0;
			for (int i = 0; i < shape.Length; i++)
			{
				hash ^= origin[i] ^ shape[i];
			}
			return hash;
		}

        /// <summary>
        /// Checks whether two rectangles intersect or not.
        /// </summary>
        /// <param name="r1"></param>
        /// <param name="r2"></param>
        /// <returns></returns>
        public static bool HasIntersection(Rectangle r1, Rectangle r2)
        {
            if (r1.IsEmpty || r2.IsEmpty)
                return false;
            if (r1.Rank != r2.Rank)
                throw new ArithmeticException("r1 and r2 have different rank");

            int r = r1.Rank;
            for (int i = 0; i < r; i++)
            {
                int a = Math.Max(r1.origin[i], r2.origin[i]);
                int b = Math.Min(r1.origin[i] + r1.shape[i] - 1, r2.origin[i] + r2.shape[i] - 1);
                if (a > b)
                    return false;
            }

            return true;
        }

		/// <summary>
		/// Combines two rectangle returning the mimimal rectangle containg both rectangles.
		/// </summary>
		/// <param name="r1"></param>
		/// <param name="r2"></param>
		/// <returns></returns>
		public static Rectangle Combine(Rectangle r1, Rectangle r2)
		{
			if (r1.Rank != r2.Rank)
				throw new ArgumentException("Rectangles are defined in spaces with different ranks.");

			Rectangle r = new Rectangle(r1.Rank);

			for (int i = 0; i < r1.origin.Length; i++)
			{
				int max = r1.shape[i] + r1.origin[i];

				r.origin[i] = Math.Min(r1.origin[i], r2.origin[i]);

				if (max < r2.origin[i] + r2.shape[i])
					max = r2.origin[i] + r2.shape[i];

				r.shape[i] = max - r1.origin[i];
			}
			return r;
		}

		/// <summary>
		/// Gets the rectangle with zero origin and given shape.
		/// </summary>
		/// <param name="shape">Shape of the resulting rectangle.</param>
		/// <returns>The rectangle.</returns>
		public static Rectangle EntireShape(int[] shape)
		{
			if (shape == null)
				throw new ArgumentNullException("shape");
			return new Rectangle(new int[shape.Length], shape);
		}

		/// <summary>
		/// Gets an empty rectangle (i.e. its origin and shape are null).
		/// </summary>
		public static Rectangle EmptyRectangle
		{
			get { return new Rectangle(); }
		}
	}
}

