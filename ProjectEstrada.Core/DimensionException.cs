using System;

namespace ProjectEstrada.Core
{
	public class DimensionException : Exception
	{
		public override string Message { get; }

		public DimensionException() { }

		public DimensionException(string message)
		{
			Message = message;
		}
	}
}
