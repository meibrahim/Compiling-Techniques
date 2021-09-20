using System;
using System.Collections.ObjectModel;
using static ECOTE.Text.Source;

namespace ECOTE.Ast
{
	partial class AST
	{
        public AST(Tokenizer tokens)
        {
            Tokens = tokens ?? throw new ArgumentNullException(nameof(tokens));
        }

        public Block Root { get; } = new Block();
		public Tokenizer Tokens { get; }

		public void Accept(IVisitor visitor) => visitor.Visit(Root);

		public interface INode
		{
			Token Token { get; }
			void Accept(IVisitor visitor);
		}

		public interface INamed
		{
			String Name { get; }
		}

		public interface ITyped
		{
			NamePath Type { get; }
		}

		public interface IAccess
		{
			AccessFlags Access { get; }
		}

		public interface IDeclaration : INamed
		{
		}

		public interface IMember : INamed, ITyped, IAccess, INode
		{
		}

		public abstract class Node : INode
		{
			public Token Token { get; set; }
			public abstract void Accept(IVisitor visitor);
		}

		public abstract class Scope : Collection<INode>, INode 
		{
			public Token Token { get; set; }
			public abstract void Accept(IVisitor visitor);
		}

		public class Block : Scope
		{
			public override void Accept(IVisitor visitor) => visitor.Visit(this);
		}

		public class Array : Scope
		{
			public override void Accept(IVisitor visitor) => visitor.Visit(this);
		}

		public class Arguments : Scope
		{
			public override void Accept(IVisitor visitor) => visitor.Visit(this);
		}

		public class Statement : Block
		{
			public override void Accept(IVisitor visitor) => visitor.Visit(this);
		}

		public class Template : Collection<INode>, INode
		{
			public Token Token { get; set; }
			public virtual void Accept(IVisitor visitor) => visitor.Visit(this);
		}

		public class Operator : Node, INamed
		{
			public String Name { get; set; }

			public override String ToString() => Name;
			public override void Accept(IVisitor visitor) => visitor.Visit(this);
		}

		public abstract class Keyword : Node, INamed
		{
			public abstract String Name { get; }

			public class If : Keyword
			{
				public override String Name => "if";
				public Arguments Logic { get; set; }
				public Block True { get; set; }
				public Block False { get; set; }
				public override void Accept(IVisitor visitor) => visitor.Visit(this);
			}

			public class For : Keyword
			{
				public override String Name => "for";
				public Arguments Logic { get; set; }
				public Block Body { get; set; }
				public override void Accept(IVisitor visitor) => visitor.Visit(this);
			}

			public class While : Keyword
			{
				public override String Name => "while";
				public Arguments Logic { get; set; }
				public Block Body { get; set; }
				public override void Accept(IVisitor visitor) => visitor.Visit(this);
			}

			public class Do : Keyword
			{
				public override String Name => "do";
				public While Loop { get; set; }
				public Block Body { get; set; }

				public override void Accept(IVisitor visitor) => visitor.Visit(this);
			}

			public class Case : Keyword
			{
				public override String Name => "case";
				public String Value { get; set; }
				public Block Body { get; set; }
				public override void Accept(IVisitor visitor) => visitor.Visit(this);
			}

			public class Switch : Keyword
			{
				public override String Name => "switch";
				public Collection<Case> Cases { get; set; }
				public override void Accept(IVisitor visitor) => visitor.Visit(this);
			}

			public class Return : Keyword
			{
				public override String Name => "return";
				public Statement Value { get; set; }
				public override void Accept(IVisitor visitor) => visitor.Visit(this);
			}

			public class New : Keyword
			{
				public override String Name => "new";
				public Statement Value { get; set; }
				public override void Accept(IVisitor visitor) => visitor.Visit(this);
			}


			public class Break : Keyword
			{
				public override String Name => "break";
				public override void Accept(IVisitor visitor) => visitor.Visit(this);
			}

			public class Continue : Keyword
			{
				public override String Name => "continue";
				public override void Accept(IVisitor visitor) => visitor.Visit(this);
			}
		}

		public class Command : Node
		{
			public NamePath Path { get; set; }
			public Arguments Arguments { get; set; }
			public override void Accept(IVisitor visitor) => visitor.Visit(this);
		}

		public class Cast : Node
		{
			public NamePath Type { get; set; }
			public override void Accept(IVisitor visitor) => visitor.Visit(this);
		}

		public class Attribute : Node
		{
			public NamePath Name { get; set; }
			public Arguments Arguments { get; set; }
			public override void Accept(IVisitor visitor) => visitor.Visit(this);
		}

		public class Reference : Node
		{
			public NamePath Path { get; set; }
			public override String ToString() => Path.ToString();
			public override void Accept(IVisitor visitor) => visitor.Visit(this);
		}

		public class Literal : Node
		{
			public String Value { get; set; }
			public override String ToString() => Value;
			public override void Accept(IVisitor visitor) => visitor.Visit(this);
		}

		public class Label : Node, INamed
		{
			public String Name { get; set; }
			public override String ToString() => $"{Name}:";
			public override void Accept(IVisitor visitor) => visitor.Visit(this);
		}

		public class NamePath : Node, INamed
		{
			public String Name { get; set; }
			public Array Array { get; set; }
			public Template Template { get; set; }
			public NamePath Parent { get; set; }
			public NamePath Child { get; set; }
			public override void Accept(IVisitor visitor) => visitor.Visit(this);

			public override String ToString()
			{
				if (Child != null)
					return $"{Name}{Template}{Array}.{Child}";
				else
					return $"{Name}{Template}{Array}";
			}
		}

		public class Inherits : Collection<NamePath>, INode
		{
			public Token Token { get; set; }
			public virtual void Accept(IVisitor visitor) => visitor.Visit(this);
		}

		public class Variable : Node, IMember
		{
			public String Name { get; set; }
			public NamePath Type { get; set; }
			public Variable Next { get; set; }
			public AccessFlags Access { get; set; }
			public Statement Initializer { get; set; }
			public override void Accept(IVisitor visitor) => visitor.Visit(this);
		}

		public class Property : Node, IMember
		{
			public String Name { get; set; }
			public Method Getter { get; set; }
			public Method Setter { get; set; }
			public NamePath Type { get; set; }
			public AccessFlags Access { get; set; }
			public override void Accept(IVisitor visitor) => visitor.Visit(this);
		}

		public class Method : Node, IMember
		{
			public String Name { get; set; }
			public NamePath Type { get; set; }
			public Block Body { get; set; }
			public Template Template { get; set; }
			public Arguments Arguments { get; set; }
			public AccessFlags Access { get; set; }
			public override void Accept(IVisitor visitor) => visitor.Visit(this);
		}

		public class Enum : Node, IDeclaration, IAccess
		{
			public String Name { get; set; }
			public Block Body { get; set; }
			public Inherits Inherits { get; set; }
			public AccessFlags Access { get; set; }
			public override void Accept(IVisitor visitor) => visitor.Visit(this);
		}

		public class Class : Node, IDeclaration, IAccess
		{
			public String Name { get; set; }
			public Block Body { get; set; }
			public Template Template { get; set; }
			public Inherits Inherits { get; set; }
			public AccessFlags Access { get; set; }
			public override void Accept(IVisitor visitor) => visitor.Visit(this);
		}

		public class Struct : Node, IDeclaration, IAccess
		{
			public String Name { get; set; }
			public Block Body { get; set; }
			public Template Template { get; set; }
			public Inherits Inherits { get; set; }
			public AccessFlags Access { get; set; }
			public override void Accept(IVisitor visitor) => visitor.Visit(this);
		}

		public class Interface : Node, IDeclaration, IAccess
		{
			public String Name { get; set; }
			public Block Body { get; set; }
			public Template Template { get; set; }
			public Inherits Inherits { get; set; }
			public AccessFlags Access { get; set; }
			public override void Accept(IVisitor visitor) => visitor.Visit(this);
		}

		public class Namespace : Node, IDeclaration
		{
			public String Name { get; set; }
			public Block Body { get; set; }
			public override void Accept(IVisitor visitor) => visitor.Visit(this);
		}
	}
}
