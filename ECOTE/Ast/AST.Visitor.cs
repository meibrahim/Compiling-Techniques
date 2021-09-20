namespace ECOTE.Ast
{
	partial class AST
	{
		public interface IVisitor
		{
			void Visit(Block node);
			void Visit(Array node);
			void Visit(Arguments node);
			void Visit(Method node);
			void Visit(Reference node);
			void Visit(Variable node);
			void Visit(Property node);
			void Visit(Operator node);
			void Visit(Keyword.If node);
			void Visit(Keyword.Switch node);
			void Visit(Keyword.Case node);
			void Visit(Keyword.For node);
			void Visit(Keyword.Do node);
			void Visit(Keyword.While node);
			void Visit(Keyword.Return node);
			void Visit(Keyword.New node);
			void Visit(Keyword.Break node);
			void Visit(Keyword.Continue node);
			void Visit(Cast node);
			void Visit(Enum node);
			void Visit(Class node);
			void Visit(Struct node);
			void Visit(Interface node);
			void Visit(Namespace node);
			void Visit(NamePath node);
			void Visit(Literal node);
			void Visit(Label node);
			void Visit(Command node);
			void Visit(Template node);
			void Visit(Inherits node);
			void Visit(Attribute node);

			void Traverse(AST ast);
		}

		/// <summary>
		/// Implements basic visitation pattern, but doesn't do anything on its own
		/// </summary>
		public class Visitor : IVisitor
		{
			public virtual void Visit(Block node)
			{
				for (var i = 0; i < node.Count; ++i)
					node[i].Accept(this);
			}

			public virtual void Visit(Array node)
			{
				for (var i = 0; i < node.Count; ++i)
					node[i].Accept(this);
			}

			public virtual void Visit(Arguments node)
			{
				for (var i = 0; i < node.Count; ++i)
					node[i].Accept(this);
			}

			public virtual void Visit(Method node)
			{
				node.Arguments?.Accept(this);
				node.Body?.Accept(this);
			}

			public virtual void Visit(Variable node)
			{
				node.Initializer?.Accept(this);
				node.Next?.Accept(this);
			}

			public virtual void Visit(Property node)
			{
				node.Getter?.Accept(this);
				node.Setter?.Accept(this);
			}

			public virtual void Visit(Operator node)
			{
				
			}

			public virtual void Visit(Keyword.If node)
			{
				node.Logic.Accept(this); 
				node.True?.Accept(this);
				node.False?.Accept(this);
			}

			public virtual void Visit(Keyword.Switch node)
			{
				for (var i = 0; i < node.Cases.Count; ++i)
					node.Cases[i].Accept(this);
			}

			public virtual void Visit(Keyword.Case node)
			{
			}

			public virtual void Visit(Keyword.For node)
			{
				node.Logic.Accept(this);
				node.Body?.Accept(this);
			}

			public virtual void Visit(Keyword.Do node)
			{
				node.Loop.Accept(this);
			}

			public virtual void Visit(Keyword.While node)
			{
				node.Logic.Accept(this);
				node.Body?.Accept(this);
			}

			public virtual void Visit(Keyword.Return node)
			{
				node.Value?.Accept(this);
			}

			public virtual void Visit(Keyword.New node)
			{
				node.Value?.Accept(this);
			}

			public virtual void Visit(Keyword.Break node)
			{
			}

			public virtual void Visit(Keyword.Continue node)
			{
			}

			public virtual void Visit(Enum node)
			{
				node.Body?.Accept(this);
				node.Inherits?.Accept(this);
			}

			public virtual void Visit(Class node)
			{
				node.Body?.Accept(this);
				node.Inherits?.Accept(this);
			}

			public virtual void Visit(Struct node)
			{
				node.Body?.Accept(this);
				node.Inherits?.Accept(this);
			}

			public virtual void Visit(Interface node)
			{
				node.Body?.Accept(this);
				node.Inherits?.Accept(this);
			}

			public virtual void Visit(Namespace node)
			{
				node.Body?.Accept(this);
			}

			public virtual void Visit(Reference node)
			{
				node.Path.Accept(this);
			}

			public virtual void Visit(NamePath node)
			{
				node.Array?.Accept(this);
				node.Template?.Accept(this);
				node.Child?.Accept(this);
			}

			public virtual void Visit(Literal node)
			{
			}

			public virtual void Visit(Label node)
			{
			}

			public virtual void Visit(Command node)
			{
				node.Path?.Accept(this);
				node.Arguments?.Accept(this);
			}

			public virtual void Visit(Template node)
			{
				for (var i = 0; i < node.Count; ++i)
					node[i].Accept(this);
			}

			public virtual void Visit(Inherits node)
			{
				for (var i = 0; i < node.Count; ++i)
					node[i].Accept(this);
			}

			public virtual void Visit(Attribute node)
			{
			}

			public virtual void Visit(Cast node)
			{
			}

			public virtual void Traverse(AST ast)
			{
				ast.Root.Accept(this);
			}
		}
	}
}
