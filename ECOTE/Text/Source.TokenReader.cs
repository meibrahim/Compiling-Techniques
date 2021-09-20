using System;

namespace ECOTE.Text
{
	partial class Source
	{
		/// <summary>
		/// Simply a wrapper to easily walk along the collection of Tokens
		/// </summary>
		public class TokenReader
		{
			public TokenReader(Tokenizer tokens)
			{
				Tokens = tokens ?? throw new ArgumentNullException(nameof(tokens));
			}

			public Token this[Int32 relative]
			{
				get
				{
					var index = Position + relative;
					if (index < 0 || index >= Tokens.Count)
						return default;
					return Tokens[index];
				}
			}

			public Boolean TryRead(out Token token)
			{
				token = Current;
				return Read();
			}

			public Boolean Peek()
			{
				return Position + 1 < Tokens.Count;
			}

			public Boolean Read()
			{
				return (Position = Math.Min(++Position, Tokens.Count)) < Tokens.Count;
			}

			public Token Previous { get => this[-1]; }
			public Token Current { get => this[0]; }
			public Token Next { get => this[1]; }

			public Int32 Position { get; set; }

			public Tokenizer Tokens { get; }
		}
	}
}
