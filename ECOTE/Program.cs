using ECOTE.Ast;
using ECOTE.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static ECOTE.Text.Source;

namespace ECOTE
{
	static class Program
	{
		[Flags]
		enum Mode { Default = 0, LDefs = 1, LRefs = 2 }

		static void Main(String[] args)
		{
			var mode = Mode.Default;
			var file = String.Empty;
			var vars = new List<String>();

			for (var i = 0; i < args.Length; ++i)
			{
				if (args[i] == "-ld")
					mode |= Mode.LDefs;
				else if (args[i] == "-lr")
					mode |= Mode.LRefs;
				else if (args[i] == "-vars")
				{
					while (++i < args.Length)
						vars.Add(args[i]);
				}
				else
				{
					if (!File.Exists(file = args[i]))
					{
						Error($"File \"{file}\" not found!");
						return;
					}
				}
			}

			if (file == String.Empty)
			{
				Usage();
			}
			else
			{
				var code = File.ReadAllText(file);
				var source = new Source(code);
				var tokens = new Source.Tokenizer();
				tokens.Tokenize(source);
				var builder = new AST.Builder(tokens);
				var ast = builder.Construct();
				var tracer = new ScopeTracer();
				tracer.Traverse(ast);
				var scopes = tracer.Result;
				var pairs = new List<ScopeTracer.UsagePair>();
				scopes.GetPairs(pairs);

				var defs = pairs.GroupBy(x => x.DefScope);
				var lastdefs = new Dictionary<String, ScopeTracer.UsagePair>();

				foreach (var def in defs)
				{
					foreach (var pair in def)
					{
						var varpath = $"{pair.DefScope.Path}.{pair.Ref.Path}";
						if (pair.IsAssignment)
                        {
							if (!lastdefs.ContainsKey(varpath))
								lastdefs.Add(varpath, pair);
							else
								lastdefs[varpath] = pair;
						}
                        else
                        {
							if (lastdefs.TryGetValue(varpath, out ScopeTracer.UsagePair mods))
							{
								Console.WriteLine("<");
								Console.WriteLine($"Variable {mods.DefScope.Path}.{mods.Def.Name} [line: {mods.Def.Token.Line}, column: {mods.Def.Token.Column}],");
								Console.WriteLine($"Defined [Modified] at [line: {mods.Ref.Token.Line}, column: {mods.Ref.Token.Column}],");
								Console.WriteLine($"{pair.RefScope.Path} [line: {pair.Ref.Token.Line}, column: {pair.Ref.Token.Column}]");
								Console.WriteLine(">");
							}
                        }
					}
					
				}
			}
		}

		static void Usage()
		{
			Console.WriteLine("Usage:");
			Console.WriteLine("ecote.exe filename.cs [-ld -lr] [-vars var0 var1 var2]");
		}

		static void Error(String message)
		{
			Console.WriteLine(message);
		}
	}
}
