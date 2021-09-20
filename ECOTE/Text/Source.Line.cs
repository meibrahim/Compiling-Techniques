using System;

namespace ECOTE.Text
{
	partial class Source
	{
		/// <summary>
		/// Allows us to easily map out line numbers, columns, et cetera
		/// </summary>
		public struct Line
		{
			public Int32 Offset;
			public Int32 Number;
			public Int32 Length;
			public Source Source;

			public Line(Source source, Int32 offset, Int32 number, Int32 length)
			{
				Source = source;
				Offset = offset;
				Number = number;
				Length = length;
			}

			public Char this[Int32 index]
			{
				get => Source[Offset + index];
			}

			public override String ToString()
			{
				return Source.Code.Substring(Offset, Length);
			}
		}
	}
}
