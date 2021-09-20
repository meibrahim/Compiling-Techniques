using System;
using System.Linq;
using static ECOTE.Ast.AST;

namespace ECOTE
{
	public class Helper
	{
		public Boolean IsLiteral(String token)
		{
			return IsCharLiteral(token)
				|| IsStringLiteral(token)
				|| IsObjectLiteral(token)
				|| IsBooleanLiteral(token)
				|| IsDecimalLiteral(token)
				|| IsHexLiteral(token);
		}

		public Boolean IsObjectLiteral(String token)
		{
			return token == "null" || token == "default";
		}

		public Boolean IsCharLiteral(String token)
		{
			return token.StartsWith("\'");
		}

		public Boolean IsStringLiteral(String token)
		{
			return token.StartsWith("\"");
		}

		public Boolean IsBooleanLiteral(String token)
		{
			return token == "true" || token == "false";
		}

		public Boolean IsDecimalLiteral(String token)
		{
			token = token.ToLowerInvariant();
			if (!token.Where(x => "1234567890.fmu".IndexOf(x) == -1).Any())
			{
				if (token.Where(x => "fmu".IndexOf(x) != -1).Any())
					token = token.Substring(0, token.Length - 1);
				return Byte.TryParse(token, out Byte _)
					|| SByte.TryParse(token, out SByte _)
					|| Int16.TryParse(token, out Int16 _)
					|| Int32.TryParse(token, out Int32 _)
					|| Int64.TryParse(token, out Int64 _)
					|| UInt16.TryParse(token, out UInt16 _)
					|| UInt32.TryParse(token, out UInt32 _)
					|| UInt64.TryParse(token, out UInt64 _)
					|| Single.TryParse(token, out Single _)
					|| Double.TryParse(token, out Double _)
					|| Decimal.TryParse(token, out Decimal _);
			}
			return false;
		}

		public Boolean IsHexLiteral(String token)
		{

			if (token.StartsWith("0x"))
			{
				token = token.Substring(2).ToUpperInvariant();
				return !token.Where(x => "ABCDEF01234567".IndexOf(x) == -1).Any();
			}
			return false;
		}

		public Boolean TryReadEnum<T>(String token, out T result) where T : struct
		{
			return System.Enum.TryParse(token, true, out result) && IsLowerCase(token) && !IsLiteral(token);
		}

		public Boolean IsValidEnum<T>(String token) where T : struct
		{
			return System.Enum.TryParse(token, true, out T result) && IsLowerCase(token) && !IsLiteral(token);
		}

		public Boolean IsLowerCase(String token) => !token.Where(x => Char.IsUpper(x)).Any();
		public Boolean IsUpperCase(String token) => !token.Where(x => Char.IsLower(x)).Any();

		public Boolean IsValidName(String token)
		{
			return !IsReserved(token) && !(token.Where(x => !Char.IsLetterOrDigit(x) && x != '_').Any());
		}

		public Boolean IsValidType(String token)
		{
			return IsValidName(token) || ReservedTypes.Contains(token);
		}

		public Int32 GetBracket(String token)
		{
			if (token.Length != 1)
				return -1;
			return Brackets.IndexOf(token);
		}

		const String Brackets = Opening + Closing;
		const String Opening = "{([";
		const String Closing = "})]";
		public Boolean IsOpening(String token) => IsOpening(GetBracket(token));
		public Boolean IsClosing(String token) => IsClosing(GetBracket(token));
		public Boolean IsOpening(Int32 index) => index < 3 && index > -1;
		public Boolean IsClosing(Int32 index) => index >= 3 && index < 6;
		public Boolean IsArray(Int32 index) => index % 3 == 2;
		public Boolean IsArgument(Int32 index) => index % 3 == 1;
		public Boolean IsBlock(Int32 index) => index % 3 == 0;

		static readonly String[] Operators = ", . ; : + - * / % ^ ~ ! ? $ & | > && || < = => == >= <= != += -= *= /= %= ^= ~= ++ -- >> << >>= <<=".Split(' ');
		public Boolean IsDeclaration(String token) => Declarations.Contains(token);
		public Boolean IsOperator(String token) => Operators.Contains(token);
		public Boolean IsKeyword(String token) => Keywords.Contains(token);

		public Boolean IsReserved(String token)
		{
			return IsValidEnum<AccessFlags>(token)
				|| ReservedTypes.Contains(token)
				|| Declarations.Contains(token)
				|| Keywords.Contains(token);
		}

		static String[] ReservedTypes { get; } = new String[]
		{
				"void", "char", "bool", "string", "dynamic", "decimal", "single", "double",
				"byte", "sbyte", "ushort", "short", "uint", "int", "ulong", "long"
		};

		static String[] Declarations { get; } = new String[]
		{
				"enum", "class", "struct", "interface", "namespace", "operator", "prop",
		};

		static String[] Keywords { get; } = new String[]
		{
				"if", "else", "for", "while", "do", "break", "continue", "new",
				"goto", "return", "switch", "case", "yield", "await", "async",
				"using", "fixed"
		};
	}
}
