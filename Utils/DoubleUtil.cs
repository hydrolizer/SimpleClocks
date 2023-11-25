using System;

namespace SimpleClocks.Utils
{
	public static class DoubleUtil
	{
		public static bool IsDoubleAndValid(object value, out double @double)
		{
			@double = double.NaN;
			if(!(value is double d))
				return false;
			if (!IsValidDouble(d))
				return false;
			@double = d;
			return true;
		}

		public static bool IsValidDouble(double d)
			=> !(double.IsNaN(d) || double.IsInfinity(d));

		public static double ToRadians(this double degrees) => (Math.PI / 180d) * degrees;
		public static double ToDegrees(this double radians) => (radians * 180d) / Math.PI;
		public static double Cotan(this double radians) => Math.Cos(radians) / Math.Sin(radians);
	}
}
