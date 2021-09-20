using ECOTE.Ast;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using static ECOTE.Ast.AST;
using static ECOTE.Text.Source;
using System.Collections.ObjectModel;
using System.Collections;

namespace ECOTE
{
	/// <summary>
	/// This lad here traces our variable usage 
	/// </summary>
	class ScopeTracer : Visitor
	{
		readonly Stack<Scope> Stack = new Stack<Scope>();

		public Scope Result { get; private set; }

		public class Scope : IReadOnlyList<Scope>
		{
			readonly List<Scope> Children = new List<Scope>();
			readonly Tokenizer Tokens;

			public Scope(String name, Scope parent)
			{
				Name = name ?? throw new ArgumentNullException(nameof(name));
				Parent = parent ?? throw new ArgumentNullException(nameof(parent));
				Tokens = Parent.Tokens;
				parent?.Children.Add(this);
			}

			public Scope(Tokenizer tokens)
			{
				Name = String.Empty;
				Tokens = tokens;
				Parent = null;
			}

			public Int32 GetPairs(IList<UsagePair> pairs)
			{
				var result = 0;
				foreach (var member in this.Members)
				{
					if (member is Variable variable)
                    {
						if (variable.Initializer != null) 
						{
							var pair = new UsagePair()
							{
								Def = variable,
								Ref = new Reference()
								{
									Path = new NamePath() 
									{
										Name = variable.Name
									},
									Token = variable.Token
								},
								DefScope = this,
								RefScope = this,
								IsAssignment = true
							};
							pairs.Add(pair);
						}
					}
                }
				foreach (var member in this.Members)
					result += GetPairs(this, member, pairs);
				foreach (var child in this)
					result += child.GetPairs(pairs);
				return result;
			}

			public Int32 GetPairs(Scope defScope, IMember definition, IList<UsagePair> pairs)
			{
				var result = 0;
				foreach (var reference in this.References)
				{
					if (reference.Path.Name == definition.Name)
					{
						var pair = new UsagePair()
						{
							Def = definition,
							Ref = reference,
							DefScope = defScope,
							RefScope = this
						};
						var iToken = Tokens.IndexOf(reference.Token);
						var iPrev = iToken - 1;
						var iNext = iToken + 1;
						if (iPrev >= 0)
						{
							var prev = Tokens[iPrev];
							if (prev.Value == "++" || prev.Value == "--")
							{
								pair.IsAssignment = true;
							}
						}
						if (iNext < Tokens.Count)
						{
							var next = Tokens[iNext];
							var valid = new String[] { "=", "++", "--", "+=", "-=", "*=", "/=", "%=", "^=", "~=", "&=", "|=" };
							if (valid.Contains(next.Value))
							{
								pair.IsAssignment = true;
							}
						}
						pairs.Add(pair);
					}
				}
				foreach (var child in this)
					result += child.GetPairs(defScope, definition, pairs);
				return result;
			}

			public Boolean IsAncestor(Scope of)
			{
				while (of != null)
				{
					if (of.Parent == this)
						return true;
					of = of.Parent;
				}
				return false;
			}

			public override String ToString() => Path;
			public Scope this[Int32 index] => Children[index];

			public String Name { get; }
			public String Path
			{
				get
				{
					if (Parent != null)
					{
						if (!String.IsNullOrEmpty(Parent.Path))
						{
							if (!String.IsNullOrEmpty(Name))
								return $"{Parent.Path}{Kind}{Name}";
							return Parent.Path;
						}
					}
					return Name;
				}
			}
			
			public Char Kind { get; set; } = '.';
			public Scope Parent { get; }
			public Int32 Count => Children.Count;
			public List<IMember> Members { get; } = new List<IMember>();
			public List<Reference> References { get; } = new List<Reference>();

			public IEnumerator<Scope> GetEnumerator() => Children.GetEnumerator();
			IEnumerator IEnumerable.GetEnumerator() => Children.GetEnumerator();
		}

		public class UsagePair
		{
			public IMember Def { get; set; }
			public Reference Ref { get; set; }
			public Scope DefScope { get; set; }
			public Scope RefScope { get; set; }
			public Boolean IsAssignment { get; set; }

			public override String ToString()
			{
				if (IsAssignment)
				{
					return $"Def {Ref.Path}";
				}
				else
				{
					return $"Ref {Ref.Path}";
				}
			}
		}

		public override void Traverse(AST ast)
		{
			Stack.Push(new Scope(ast.Tokens));
			base.Traverse(ast);
			Result = Stack.Pop();
		}

		/*public override void Visit(Block node)
		{
			Stack.Push(new Scope(String.Empty, Stack.Peek()));
			base.Visit(node);
			Stack.Pop();
		}

		public override void Visit(AST.Array node)
		{
			Stack.Push(new Scope(String.Empty, Stack.Peek()));
			base.Visit(node);
			Stack.Pop();
		}

		public override void Visit(Arguments node)
		{
			Stack.Push(new Scope(String.Empty, Stack.Peek()));
			base.Visit(node);
			Stack.Pop();
		}*/

		public override void Visit(Class node)
		{
			Stack.Push(new Scope(node.Name, Stack.Peek()) { Kind = ':' });
			base.Visit(node);
			Stack.Pop();
		}

		public override void Visit(Struct node)
		{
			Stack.Push(new Scope(node.Name, Stack.Peek()) { Kind = ':' });
			base.Visit(node);
			Stack.Pop();
		}

		public override void Visit(Interface node)
		{
			Stack.Push(new Scope(node.Name, Stack.Peek()) { Kind = ':' });
			base.Visit(node);
			Stack.Pop();
		}

		public override void Visit(Namespace node)
		{
			Stack.Push(new Scope(node.Name, Stack.Peek()) { Kind = ':' });
			base.Visit(node);
			Stack.Pop();
		}

		public override void Visit(Method node)
		{
			Stack.Push(new Scope(node.Name, Stack.Peek()));
			base.Visit(node);
			Stack.Pop();
		}

		public override void Visit(Property node)
		{
			Stack.Peek().Members.Add(node);
			Stack.Push(new Scope(node.Name, Stack.Peek()));
			base.Visit(node);
			Stack.Pop();
		}

		public override void Visit(Variable node)
		{
			Stack.Peek().Members.Add(node);
			base.Visit(node);
		}

		public override void Visit(Reference node)
		{
			Stack.Peek().References.Add(node);
			base.Visit(node);
		}

		public override void Visit(Keyword.If node)
		{
			Stack.Push(new Scope(node.Name, Stack.Peek()));
			base.Visit(node);
			Stack.Pop();
		}

		public override void Visit(Keyword.While node)
		{
			Stack.Push(new Scope(node.Name, Stack.Peek()));
			base.Visit(node);
			Stack.Pop();
		}

		public override void Visit(Keyword.For node)
		{
			Stack.Push(new Scope(node.Name, Stack.Peek()));
			base.Visit(node);
			Stack.Pop();
		}

		public override void Visit(Keyword.New node)
		{
			Stack.Push(new Scope(node.Name, Stack.Peek()));
			base.Visit(node);
			Stack.Pop();
		}

		public override void Visit(Keyword.Return node)
		{
			Stack.Push(new Scope(node.Name, Stack.Peek()));
			base.Visit(node);
			Stack.Pop();
		}

		public override void Visit(Keyword.Switch node)
		{
			Stack.Push(new Scope(node.Name, Stack.Peek()));
			base.Visit(node);
			Stack.Pop();
		}

		public override void Visit(Keyword.Do node)
		{
			Stack.Push(new Scope(node.Name, Stack.Peek()));
			base.Visit(node);
			Stack.Pop();
		}

		public override void Visit(Keyword.Case node)
		{
			Stack.Push(new Scope(node.Name, Stack.Peek()));
			base.Visit(node);
			Stack.Pop();
		}
	}
}
