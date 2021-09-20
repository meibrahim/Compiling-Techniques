using System;

namespace ECOTE.Ast
{
	partial class AST
	{
		[Flags]
		public enum AccessFlags : Int32
		{
			Default = 0,
			Const = 1,
			Static = 2,
			Public = 4,
			Virtual = 8,
			Abstract = 16,
			Override = 32,
			Internal = 64,
			ReadOnly = 128,
			Protected = 256,
			Anonymous = 512,
			Arguments = 1024,
			Unmanaged = 2048
		}
	}
}
