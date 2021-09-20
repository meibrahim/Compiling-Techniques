using System;
using System.Collections.ObjectModel;
using System.Linq;
using static ECOTE.Ast.AST;

namespace ECOTE.Text
{
	partial class Source
	{
		/// <summary>
		/// Turns source code into tangible tokens that we can work with to create an AST
		/// </summary>
		public class Tokenizer : Collection<Token>
		{
			public const String Notation = "=><+-/*~!@#$%^&|()[]{},.:;?\"\'";
			public const String Literals = "\"\'";
			public const String CommentLine = "//";
			public static readonly String[] CommentLong = { "/*", "*/" };
			private readonly Helper Helper = new Helper();

			public Boolean IsNotation(Char symbol) => Notation.IndexOf(symbol) != -1;
			public Boolean IsLiteral(Char symbol) => Literals.IndexOf(symbol) != -1;

			public void Tokenize(Source source)
			{
				Clear();
				var line = 0;
				var offset = 0;
				for (var i = 0; i < source.Length; ++i)
				{
					var symbol = source[i];
					if (symbol == '\n') ++line;
					var isNotation = IsNotation(symbol);				
					if (Char.IsWhiteSpace(symbol) || isNotation)
					{
						var length = i - offset;
						if (length > 0)
						{
							Add(new Token(source, offset, line + 1, Column(source, line, offset), length));
						}
						if (isNotation)
						{
							offset = i;

							// Check if we have a string or character literal
							var literal = Literals.IndexOf(symbol);
							if (literal != -1)
							{
								var prev = symbol;
								for (++i; i < source.Length; ++i)
								{
									symbol = source[i];
									if (prev != '\\' && symbol == Literals[literal])
										break;
									else
										prev = symbol;
								}
								length = i - offset + 1;
							}
							else length = 1;
							//

							// Check if we have a single-line comment
							if (CommentLine.StartsWith(symbol) && source.Length - i > CommentLine.Length)
							{
								if (source.Code.Substring(i, CommentLine.Length) == CommentLine)
								{
									for (i += CommentLine.Length; i < source.Length && source[i] != '\n' && source[i] != '\r'; ++i) ;
									length = i - offset;
									offset = i + 1;
									continue;
								}
							}
							// Check if we have a bounded comment
							if (CommentLong[0].StartsWith(symbol) && source.Length - i > CommentLong[0].Length)
							{
								if (source.Code.Substring(i, CommentLong[0].Length) == CommentLong[0])
								{
									for (i += CommentLong[0].Length; i < source.Length; ++i)
									{
										symbol = source[i];
										if (CommentLong[1].StartsWith(symbol) && source.Length - i > CommentLong[1].Length)
										{
											if (source.Code.Substring(i, CommentLong[1].Length) == CommentLong[1])
											{
												i += CommentLong[1].Length;
												break;
											}
										}
									}
									length = i - offset;
									offset = i + 1;
									continue;
								}
							}	
							//

							Add(new Token(source, offset, line + 1, Column(source, line, offset), length));
						}
						offset = i + 1;
					}
				}

				for (var i = 0; i < Count; ++i)
				{
					var token = this[i];
					if (Helper.IsOperator(token))
					{
						offset = i;
						ConcatOperators(ref offset);
						if (offset > i)
						{
							token.Length = offset - i + 1;
						}
						for (var j = offset; j > i; --j)
						{
							RemoveAt(j);
						}
					}
					else if (Helper.IsLiteral(token))
					{
						offset = i;
						ConcatLiterals(ref offset);
						if (offset > i)
						{
							token.Length = offset - i + 1;
						}
						for (var j = offset; j > i; --j)
						{
							RemoveAt(j);
						}
					}
				}
			}

			private String ConcatOperators(ref Int32 offset)
			{
				var concat = (String)this[offset];
				do
				{
					var part = this[offset + 1];
					if (Helper.IsOperator(part))
					{
						var step = concat + part;
						if (!Helper.IsOperator(step))
							break;
						else
							concat = step;
					}
					else break;
				} while (Count > ++offset);
				return concat;
			}

			private String ConcatLiterals(ref Int32 offset)
			{
				var concat = (String)this[offset];
				if (Helper.IsDecimalLiteral(concat))
				{
					var part = this[offset + 1];
					if (part == ".")
					{
						var step = concat + part + this[offset + 2];
						if (Helper.IsDecimalLiteral(step))
						{
							offset += 2;
						}
					}
				}
				return concat;
			}

			protected override void InsertItem(Int32 index, Token item)
			{
				if (Count > 0)
				{
					var previous = this.LastOrDefault();
					if (previous.Offset + previous.Length > item.Offset)
						throw new ArgumentException(nameof(item));
				}
				base.InsertItem(index, item);
			}

			private static Int32 Column(Source source, Int32 line, Int32 offset)
			{
				var result = 1;
				var lineRef = source.lines[line];
				for (var i = 0; i < offset - lineRef.Offset; ++i) 
				{
					if (lineRef[i] == '\t')
						result += TabSize;
					else
						++result;
				}
				return result;
			}

			public const Int32 TabSize = 4;
		}
	}
}
