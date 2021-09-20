using System;
using System.Collections.Generic;

namespace ECOTE.Text
{
	/// <summary>
	/// Wraps strings for easier parsing
	/// </summary>
	partial class Source
	{
		readonly List<Line> lines;

		public Source() : this(String.Empty)
		{
		}

		public Source(String code)
		{
			lines = new List<Line>();
			Update(code);
		}

		public Char this[Int32 offset] => Code?[offset] ?? '\0';

		public Char this[Int32 line, Int32 column] => Code[lines[line].Offset + column];

		public void Update(String code)
		{
			Code = code;
			lines.Clear();
			var line = new Line() { Source = this, Number = 1 };
			for (var i = 0; i < code.Length; ++i) 
			{
				if (code[i] == '\n')
				{
					line.Length = i - line.Offset;
					lines.Add(line);
					line.Offset = i + 1;
					line.Number++;
				}
			}
			line.Length = code.Length - line.Offset;
			lines.Add(line);
		}

		public Int32 Length { get => Code?.Length ?? 0; }
		public String Code { get; private set; }
		public IReadOnlyList<Line> Lines { get => lines.AsReadOnly(); }
	}
}
