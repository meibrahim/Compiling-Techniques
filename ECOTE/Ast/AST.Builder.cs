using System;
using System.Collections.Generic;
using static ECOTE.Text.Source;

namespace ECOTE.Ast
{
	partial class AST
	{
		/// <summary>
		/// Constructs an AST out of a <see cref="Tokenizer"/> instance
		/// </summary>
		public class Builder : TokenReader
		{
			private readonly Helper Helper;
			private readonly Stack<Int32> Balance;
			private readonly Stack<NamePath> Skipped;
			private readonly Stack<AccessFlags> Access;
			private readonly Stack<ScopeTypes> ScopeType;

			public Builder(Tokenizer tokens) : base(tokens)
			{
				ScopeType = new Stack<ScopeTypes>();
				Access = new Stack<AccessFlags>();
				Skipped = new Stack<NamePath>();
				Balance = new Stack<Int32>();
				Helper = new Helper();
			}

			public AST Construct()
			{
				var result = new AST(Tokens);
				ScopeType.Push(ScopeTypes.Default);
				do
				{
					var node = Consume();
					if (node != null)
						result.Root.Add(node);
				} while (Position < Tokens.Count);
				return result;
			}

			private INode Consume()
			{
				var node = default(INode);
				if (Helper.TryReadEnum(Current, out AccessFlags flags))
				{
					Access.Push(flags);
					Read();
				}
				else if (IsEnum())
					node = ConsumeEnum();
				else if (IsClass())
					node = ConsumeClass();
				else if (IsStruct())
					node = ConsumeStruct();
				else if (IsInterface())
					node = ConsumeInterface();
				else if (IsNamespace())
					node = ConsumeNamespace();
				else if (Position > 0 && Position + 1 < Tokens.Count)
				{
					if (IsMethod())
						node = ConsumeMethod();
					else if (IsVariable())
						node = ConsumeVariable();
					else if (IsProperty())
						node = ConsumeProperty();
					else if (IsOperator())
						node = ConsumeOperator();
					else if (IsAttribute())
						node = ConsumeAttribute();
					else if (IsLabel())
						node = ConsumeLabel();
					else if (IsLiteral())
						node = ConsumeLiteral();
					else if (IsCommand())
						node = ConsumeCommand();
					else if (IsKeyword())
						node = ConsumeKeyword();
					else if (IsReference())
						node = ConsumeReference();
					else if (IsTypePath())
					{
						ConsumeTypePath();
						if (IsArguments(0))
						{
							--Position;
							ScopeType.Push(ScopeTypes.Command);
							node = new Command()
							{
								Token = ConsumeToken(),
								Path = ConsumeSkipped(),
								Arguments = ConsumeArguments()
							};
							ScopeType.Pop();
						}
						else
						{
							var index = Helper.GetBracket(Current);
							if (Helper.IsArgument(index) && Helper.IsClosing(index) && Skipped.Count > 0)
							{
								node = new Cast()
								{
									Token = Previous,
									Type = ConsumeSkipped()
								};
							}
							if (Current == ";")
							{
								node = new Reference()
								{
									Token = Current,
									Path = ConsumeSkipped(),
								};
							}
						}
					}
					else
						Console.WriteLine($"Other: {Current}");
				}
				return node;
			}

			private Boolean IsMethod()
			{
				var indexP = Helper.GetBracket(Previous);
				var indexN = Helper.GetBracket(Next);
				return (Helper.IsValidType(Previous) || (Helper.IsClosing(indexP) && Helper.IsArray(indexP)))
					&& Helper.IsValidName(Current)
					&& (Next == "=>" || (Helper.IsArgument(indexN) && Helper.IsOpening(indexN)));
			}

			private Boolean IsVariable()
			{
				var indexP = Helper.GetBracket(Previous);
				var indexN = Helper.GetBracket(Next);
				return (Helper.IsValidType(Previous) || (Helper.IsClosing(indexP) && Helper.IsArray(indexP)))
					&& Helper.IsValidName(Current)
					&& (Helper.IsOperator(Next) || (Helper.IsClosing(indexN) && Helper.IsArgument(indexN)));
			}

			private Boolean IsProperty()
			{
				var indexP = Helper.GetBracket(Previous);
				var indexN = Helper.GetBracket(Next);
				return (Helper.IsValidType(Previous) || (Helper.IsClosing(indexP) && Helper.IsArray(indexP)))
					&& Helper.IsValidName(Current)
					&& Helper.IsOpening(indexN)
					&& Helper.IsBlock(indexN);
			}

			private Boolean IsAccessor()
			{
				var indexN = Helper.GetBracket(Next);
				return (Current == "get" || Current == "set" || Current == "add" || Current == "remove")
					&& (Next == "=>" || (Helper.IsOpening(indexN) && Helper.IsBlock(indexN)));
			}

			private Boolean IsAttribute()
			{
				var index = Helper.GetBracket(Current);
				return Helper.IsOpening(index)
					&& Helper.IsArray(index)
					&& Helper.IsValidName(Next);
			}

			private Boolean IsCommand()
			{
				var indexP = Helper.GetBracket(Previous);
				var indexN = Helper.GetBracket(Next);
				return (Helper.IsOperator(Previous) || Helper.IsKeyword(Previous) || (Helper.IsOpening(indexP) && Helper.IsBlock(indexP)))
					&& Helper.IsValidName(Current)
					&& Helper.IsArgument(indexN)
					&& Helper.IsOpening(indexN);
			}

			private Boolean IsReference()
			{
				var indexP = Helper.GetBracket(Previous);
				var indexN = Helper.GetBracket(Next);
				return (Helper.IsOperator(Next) || !(Helper.IsOpening(indexN) && Helper.IsBlock(indexN)))
					&& Helper.IsValidName(Current) && Next != "."
					&& (Helper.IsOperator(Previous) || Helper.IsKeyword(Previous) || Helper.IsOpening(indexP) || (Helper.IsClosing(indexP) && Helper.IsArgument(indexP)));
			}

			private Boolean IsLabel()
			{
				var indexP = Helper.GetBracket(Previous);
				return (Previous == ";" || (Helper.IsOpening(indexP) && Helper.IsBlock(indexP)))
					&& Helper.IsValidName(Current)
					&& Next == ":";
			}

			private Boolean IsEnum() => Current == "enum";
			private Boolean IsClass() => Current == "class";
			private Boolean IsStruct() => Current == "struct";
			private Boolean IsInterface() => Current == "interface";
			private Boolean IsNamespace() => Current == "namespace"; 
			private Boolean IsLiteral() => Helper.IsLiteral(Current);
			private Boolean IsKeyword() => Helper.IsKeyword(Current);
			private Boolean IsOperator() => Helper.IsOperator(Current);
			private Boolean IsNamePath() => Helper.IsValidName(Current);
			private Boolean IsTypePath() => Helper.IsValidType(Current);

			private Boolean IsBlock(Int32 offset)
			{
				var index = Helper.GetBracket(this[offset]);
				return Helper.IsOpening(index) && Helper.IsBlock(index);
			}

			private Boolean IsArray(Int32 offset)
			{
				var index = Helper.GetBracket(this[offset]);
				return Helper.IsOpening(index) && Helper.IsArray(index);
			}

			private Boolean IsArguments(Int32 offset)
			{
				var index = Helper.GetBracket(this[offset]);
				return Helper.IsOpening(index) && Helper.IsArgument(index);
			}

			private AccessFlags ConsumeFlags()
			{
				var flags = AccessFlags.Default;
				while (Access.Count > 0) flags |= Access.Pop();
				return flags;
			}

			private Method ConsumeMethod()
			{
				ScopeType.Push(ScopeTypes.MethodArgs);
				var result = new Method()
				{
					Name = Current,
					Token = ConsumeToken(),
					Type = ConsumeSkipped(),
					Access = ConsumeFlags(),
					Template = ConsumeTemplate(),
					Arguments = ConsumeArguments()
				};
				ScopeType.Pop();
				ScopeType.Push(ScopeTypes.MethodBody);
				if (IsBlock(0))
					result.Body = ConsumeBlock();
				else if (Current == ";")
					ConsumeToken();
				else if (Current == "=>")
				{
					ConsumeToken();
					result.Body = ConsumeStatement();
				}
				ScopeType.Pop();
				return result;
			}

			private Variable ConsumeVariable()
			{
				var result = new Variable()
				{
					Name = Current,
					Token = ConsumeToken(),
					Type = ConsumeSkipped(),
					Access = ConsumeFlags()
				};
				if (Current == "=")
				{
					ConsumeToken();
					result.Initializer = ConsumeStatement();
				}
				if (Current == "," && ScopeType.Peek() != ScopeTypes.MethodArgs)
				{
					ConsumeToken();
					Skipped.Push(result.Type);
					result.Next = ConsumeVariable();
				}
				if (Current == ";")
				{
					ConsumeToken();
				}
				return result;
			}

			private Property ConsumeProperty()
			{
				ScopeType.Push(ScopeTypes.Property);
				var result = new Property()
				{
					Name = Current,
					Token = ConsumeToken(),
					Type = ConsumeSkipped(),
					Access = ConsumeFlags()
				};
				if (Current == "=>")
				{
					ConsumeToken();
					result.Getter = new Method()
					{
						Token = Current,
						Name = "Get",
						Type = result.Type,
						Body = ConsumeStatement()
					};
				}
				else if (IsBlock(0))
				{
					ConsumeToken();
					if (IsAccessor())
					{
						result.Getter = ConsumeAccessor();
					}
					if (IsAccessor())
					{
						result.Setter = ConsumeAccessor();
					}
					if (result.Getter.Name == "set")
					{
						var temp = result.Setter;
						result.Setter = result.Getter;
						result.Getter = temp;
					}
					ConsumeToken();
				}
				ScopeType.Pop();
				return result;
			}

			private Method ConsumeAccessor()
			{
				var result = new Method()
				{
					Name = Current,
					Token = ConsumeToken()
				};
				if (IsBlock(0))
					result.Body = ConsumeBlock();
				else if (Current == ";")
					ConsumeToken();
				else if (Current == "=>")
				{
					ConsumeToken();
					result.Body = ConsumeStatement();
					ConsumeToken();
				}
				return result;
			}

			private Enum ConsumeEnum()
			{
				ScopeType.Push(ScopeTypes.Type);
				var result = new Enum()
				{
					Token = ConsumeToken(),
					Name = ConsumeToken(),
					Access = ConsumeFlags(),
					Inherits = ConsumeInherits(),
					Body = ConsumeBlock()
				}; 
				ScopeType.Pop();
				return result;
			}

			private Class ConsumeClass()
			{
				ScopeType.Push(ScopeTypes.Type);
				var result = new Class()
				{
					Token = ConsumeToken(),
					Name = ConsumeToken(),
					Access = ConsumeFlags(),
					Template = ConsumeTemplate(),
					Inherits = ConsumeInherits(),
					Body = ConsumeBlock()
				}; 
				ScopeType.Pop();
				return result;
			}

			private Struct ConsumeStruct()
			{
				ScopeType.Push(ScopeTypes.Type);
				var result = new Struct()
				{
					Token = ConsumeToken(),
					Name = ConsumeToken(),
					Access = ConsumeFlags(),
					Template = ConsumeTemplate(),
					Inherits = ConsumeInherits(),
					Body = ConsumeBlock()
				};
				ScopeType.Pop();
				return result;
			}

			private Interface ConsumeInterface()
			{
				ScopeType.Push(ScopeTypes.Type);
				var result = new Interface()
				{
					Token = ConsumeToken(),
					Name = ConsumeToken(),
					Access = ConsumeFlags(),
					Template = ConsumeTemplate(),
					Inherits = ConsumeInherits(),
					Body = ConsumeBlock()
				};
				ScopeType.Pop();
				return result;
			}

			private Namespace ConsumeNamespace()
			{
				ScopeType.Push(ScopeTypes.Default);
				var result = new Namespace()
				{
					Token = ConsumeToken(),
					Name = ConsumeToken(),
					Body = ConsumeBlock()
				};
				ScopeType.Pop();
				return result;
			}

			private Attribute ConsumeAttribute()
			{
				throw new NotImplementedException();
			}

			private Label ConsumeLabel()
			{
				var name = ConsumeToken();
				var dots = ConsumeToken();
				return new Label()
				{
					Name = name,
					Token = name
				};
			}

			private Literal ConsumeLiteral()
			{
				if (Previous == "$")
				{
					throw new NotImplementedException();
				}
				return new Literal()
				{
					Value = Current,
					Token = ConsumeToken()
				};
			}

			private Operator ConsumeOperator()
			{
				var result = new Operator()
				{
					Name = Current,
					Token = ConsumeToken(),
				};
				return result;
			}

			private Command ConsumeCommand()
			{
				ScopeType.Push(ScopeTypes.Command);
				var result = new Command()
				{
					Token = Current,
					Path = ConsumeNamePath(),
					Arguments = ConsumeArguments()
				};
				ScopeType.Pop();
				return result;
			}

			private Reference ConsumeReference()
			{
				var result = new Reference()
				{
					Token = Current,
					Path = ConsumeNamePath(true),
				};
				return result;
			}

			private Keyword ConsumeKeyword()
			{
				switch (Current)
				{
					case "if": return ConsumeIf();
					case "for": return ConsumeFor();
					case "while": return ConsumeWhile();
					case "do": return ConsumeDo();
					case "break": return ConsumeBreak();
					case "continue": return ConsumeContinue();
					case "switch": return ConsumeSwitch();
					case "case": return ConsumeCase();
					case "return": return ConsumeReturn();
					case "new": return ConsumeNew();
					default: throw new NotImplementedException();
				}
			}

			private Keyword ConsumeIf()
			{
				var result = new Keyword.If()
				{
					Token = ConsumeToken(),
					Logic = ConsumeArguments()
				};
				if (IsBlock(0))
					result.True = ConsumeBlock();
				else
					result.True = ConsumeStatement();
				if (Current == ";")
					ConsumeToken();
				if (Current == "else")
				{
					ConsumeToken();
					if (IsBlock(0))
						result.False = ConsumeBlock();
					else
						result.False = ConsumeStatement();
				}
				return result;
			}

			private Keyword ConsumeFor()
			{
				var result = new Keyword.For()
				{
					Token = ConsumeToken(),
					Logic = ConsumeArguments(),
				};
				if (IsBlock(0))
					result.Body = ConsumeBlock();
				else
					result.Body = ConsumeStatement();
				return result;
			}

			private Keyword ConsumeWhile()
			{
				var result = new Keyword.While()
				{
					Token = ConsumeToken(),
					Logic = ConsumeArguments(),
				};
				if (IsBlock(0))
					result.Body = ConsumeBlock();
				else
					result.Body = ConsumeStatement();
				return result;
			}

			private Keyword ConsumeDo()
			{
				var result = new Keyword.Do()
				{
					Token = ConsumeToken()
				};
				if (IsBlock(0))
					result.Body = ConsumeBlock();
				else
					result.Body = ConsumeStatement();
				if (Current == "while")
				{
					result.Loop = new Keyword.While()
					{
						Token = ConsumeToken(),
						Logic = ConsumeArguments(),
						Body = result.Body
					};
				}
				return result;
			}

			private Keyword ConsumeBreak()
			{
				var result = new Keyword.Break()
				{
					Token = ConsumeToken()
				};
				return result;
			}

			private Keyword ConsumeContinue()
			{
				var result = new Keyword.Continue()
				{
					Token = ConsumeToken()
				};
				return result;
			}

			private Keyword ConsumeSwitch()
			{
				throw new NotImplementedException();
			}

			private Keyword ConsumeCase()
			{
				throw new NotImplementedException();
			}

			private Keyword ConsumeReturn()
			{
				var result = new Keyword.Return()
				{
					Token = ConsumeToken(),
					Value = ConsumeStatement()
				};
				return result;
			}

			private Keyword ConsumeNew()
			{
				var result = new Keyword.New()
				{
					Token = ConsumeToken(),
					Value = ConsumeStatement()
				};
				return result;
			}

			private NamePath ConsumeNamePath(Boolean isRef = false)
			{
				var result = new NamePath()
				{
					Name = Current,
					Token = ConsumeToken(),
				};
				if (!isRef)
				{
					result.Template = ConsumeTemplate();
				}
				result.Array = ConsumeArray();
				if (Current == ".")
				{
					ConsumeToken();
					result.Child = ConsumeNamePath();
					result.Child.Parent = result;
				}
				return result;
			}

			private NamePath ConsumeTypePath()
			{
				var result = ConsumeNamePath();
				Skipped.Push(result);
				return result;
			}

			private Inherits ConsumeInherits()
			{
				var result = default(Inherits);
				if (Current == ":")
				{
					result = new Inherits()
					{
						Token = ConsumeToken(),
					};
					while (Read())
					{
						if (Current == ":")
							continue;
						if (Current == ",")
							continue;
						if (Current == "{")
							break;
						result.Add(ConsumeNamePath());
					}			
				}
				return result;
			}

			private Block ConsumeBlock()
			{
				var nominal = Balance.Count;
				var result = default(Block);
				var index = Helper.GetBracket(Current);
				if (Helper.IsOpening(index) && Helper.IsBlock(index))
				{
					result = new Block() { Token = Current };
					do
					{
						index = Helper.GetBracket(Current);
						if (Helper.IsOpening(index))
						{
							Balance.Push(index);
							ConsumeToken();
						}
						else if (Helper.IsClosing(index))
						{
							if (Balance.Pop() != index % 3)
							{
								throw new Exception();
							}
							Read();
						}
						else
						{
							var node = Consume();
							if (node != null)
								result.Add(node);
						}
					}
					while (Balance.Count > nominal);
				}
				return result;
			}

			private Array ConsumeArray()
			{
				var nominal = Balance.Count;
				var result = default(Array);
				var index = Helper.GetBracket(Current);
				if (Helper.IsOpening(index) && Helper.IsArray(index))
				{
					result = new Array() { Token = Current };
					do
					{
						index = Helper.GetBracket(Current);
						if (Helper.IsOpening(index))
						{
							Balance.Push(index);
							ConsumeToken();
						}
						else if (Helper.IsClosing(index))
						{
							if (Balance.Pop() != index % 3)
							{
								throw new Exception();
							}
							Read();
						}
						else
						{
							var node = Consume();
							if (node != null)
								result.Add(node);
						}
					}
					while (Balance.Count > nominal);
				}
				return result;
			}

			private Arguments ConsumeArguments()
			{
				var nominal = Balance.Count;
				var result = default(Arguments);
				var index = Helper.GetBracket(Current);
				if (Helper.IsOpening(index) && Helper.IsArgument(index))
				{
					result = new Arguments() { Token = Current };
					do
					{
						index = Helper.GetBracket(Current);
						if (Helper.IsOpening(index))
						{
							Balance.Push(index);
							ConsumeToken();
						}
						else if (Helper.IsClosing(index))
						{
							if (Balance.Pop() != index % 3)
							{
								throw new Exception();
							}
							Read();
						}
						else
						{
							var node = Consume();
							if (node != null)
								result.Add(node);
						}
					}
					while (Balance.Count > nominal);
				}
				return result;
			}

			private Statement ConsumeStatement()
			{
				var nominal = Balance.Count;
				var result = new Statement();
				while ((Current != ";" && Current != ",") || Balance.Count > nominal)
				{
					var index = Helper.GetBracket(Current);
					if (Helper.IsOpening(index))
					{
						Balance.Push(index);
						ConsumeToken();
					}
					else if (Helper.IsClosing(index))
					{
						if (Balance.Pop() != index % 3)
						{
							throw new Exception();
						}
						Read();
					}
					else
					{
						var node = Consume();
						if (node != null)
						{
							result.Add(node);
						}
					}
				}
				return result;
			}

			private Template ConsumeTemplate()
			{
				var result = default(Template);
				if (Current == "<")
				{
					throw new NotImplementedException();
				}
				return result;
			}

			private Token ConsumeToken()
			{
				if (Read())
				{
					return Previous;
				}
				else
				{
					throw new Exception();
				}
			}

			private NamePath ConsumeSkipped()
			{
				return Skipped.Pop();
			}

			enum ScopeTypes { Default, Type, MethodArgs, MethodBody, Command, Property }
		}
	}
}
