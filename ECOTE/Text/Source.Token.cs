using System;

namespace ECOTE.Text
{
	partial class Source
	{
		/// <summary>
		/// Simply holds the basic token boundaries, without any context
		/// </summary>
		public class Token
		{
			public Token(Source source, Int32 offset, Int32 line, Int32 column, Int32 length)
			{
				Source = source;
				Offset = offset;
				Line = line;
				Column = column;
				Length = length;
			}

			public override String ToString()
			{
				return Source?.Code.Substring(Offset, Length);
			}

			public Source Source { get; set; }
			public Int32 Offset { get; set; }
			public Int32 Line { get; set; }
			public Int32 Column { get; set; }
			public Int32 Length { get; set; }
			public String Value { get => ToString(); }

			public static implicit operator String(Token token) => token.Value;
		}
	}
}
