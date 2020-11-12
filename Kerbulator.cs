﻿// File encoding should remain in UTF-8!

using System;
using System.IO;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Kerbulator {
	public class Kerbulator {
		public static bool DEBUG = true;
		public static void Debug(string s) {
			if(DEBUG)
				Console.Write(s);
		}
		public static void DebugLine(string s) {
			if(DEBUG)
				Console.WriteLine(s);
		}

		private Dictionary<string, Operator> operators;
		private Dictionary<string, Object> globals;
		private Dictionary<string, BuildInFunction> buildInFunctions;
		private SortedDictionary<string, JITFunction> functions;

		public Kerbulator(string functionDir) {
			operators = new Dictionary<string, Operator>();
			operators.Add("=", new Operator("=", 1, Arity.BINARY)); // Assignment
			operators.Add("-", new Operator("-", 4, Arity.BOTH)); // Substraction or negation
			operators.Add("+", new Operator("+", 4, Arity.BINARY)); // Addition
			operators.Add("/", new Operator("/", 5, Arity.BINARY)); // Division
			operators.Add("÷", new Operator("÷", 5, Arity.BINARY)); // Division
			operators.Add("√", new Operator("√", 5, Arity.BOTH)); // Square Root or ^(1/n)
			operators.Add("%", new Operator("%", 5, Arity.BINARY)); // Modulo
			operators.Add("*", new Operator("*", 5, Arity.BINARY)); // Multiplication
			operators.Add("·", new Operator("·", 5, Arity.BINARY)); // Multiplication
			operators.Add("×", new Operator("×", 5, Arity.BINARY)); // Multiplication
			operators.Add("^", new Operator("^", 6, Arity.BINARY)); // Multiplication
			operators.Add("|", new Operator("|", 6, Arity.UNARY)); // Absolute
			operators.Add("⌊", new Operator("⌊", 6, Arity.UNARY)); // Floor
			operators.Add("⌈", new Operator("⌈", 6, Arity.UNARY)); // Ceiling
			operators.Add("<", new Operator("<", 3, Arity.BINARY)); // Less than
			operators.Add(">", new Operator(">", 3, Arity.BINARY)); // Greater than
			operators.Add("<=", new Operator("<=", 3, Arity.BINARY)); // Less than or equal
			operators.Add("≤", new Operator("≤", 3, Arity.BINARY)); // Less than or equal
			operators.Add(">=", new Operator(">=", 3, Arity.BINARY)); // Greater than or equal
			operators.Add("≥", new Operator("≥", 3, Arity.BINARY)); // Greater than or equal
			operators.Add("==", new Operator("==", 3, Arity.BINARY)); // Equals
			operators.Add("!=", new Operator("!=", 3, Arity.BINARY)); // Not equals
			operators.Add("≠", new Operator("≠", 3, Arity.BINARY)); // Not equals
			operators.Add("!", new Operator("!", 6, Arity.UNARY)); // Boolean not
			operators.Add("¬", new Operator("¬", 6, Arity.UNARY)); // Boolean not
			operators.Add("and", new Operator("and", 2, Arity.BINARY)); // AND
			operators.Add("∧", new Operator("∧", 2, Arity.BINARY)); // AND
			operators.Add("or", new Operator("or", 2, Arity.BINARY)); // OR
			operators.Add("∨", new Operator("∨", 2, Arity.BINARY)); // OR
			operators.Add("buildin-function", new Operator("buildin-function", 5, Arity.BINARY)); // Execute buildin function as unary operator
			operators.Add("user-function", new Operator("user-function", 5, Arity.BINARY)); // Execute user function as unary operator

			buildInFunctions = new Dictionary<string, BuildInFunction>();
			buildInFunctions.Add("abs", new BuildInFunction("abs", 1));
			buildInFunctions.Add("acos", new BuildInFunction("acos", 1));
			buildInFunctions.Add("acos_rad", new BuildInFunction("acos_rad", 1));
			buildInFunctions.Add("asin", new BuildInFunction("asin", 1));
			buildInFunctions.Add("asin_rad", new BuildInFunction("asin_rad", 1));
			buildInFunctions.Add("atan", new BuildInFunction("atan", 1));
			buildInFunctions.Add("atan_rad", new BuildInFunction("atan_rad", 1));
			buildInFunctions.Add("atan2", new BuildInFunction("atan2", 2));
			buildInFunctions.Add("atan2_rad", new BuildInFunction("atan2_rad", 2));
			buildInFunctions.Add("ceil", new BuildInFunction("ceil", 1));
			buildInFunctions.Add("cos", new BuildInFunction("cos", 1));
			buildInFunctions.Add("cos_rad", new BuildInFunction("cos_rad", 1));
			buildInFunctions.Add("cosh", new BuildInFunction("cosh", 1));
			buildInFunctions.Add("cosh_rad", new BuildInFunction("cosh_rad", 1));
			buildInFunctions.Add("exp", new BuildInFunction("exp", 1));
			buildInFunctions.Add("floor", new BuildInFunction("floor", 1));
			buildInFunctions.Add("ln", new BuildInFunction("ln", 1));
			buildInFunctions.Add("log", new BuildInFunction("log", 1));
			buildInFunctions.Add("log10", new BuildInFunction("log10", 1));
			buildInFunctions.Add("max", new BuildInFunction("max", 2));
			buildInFunctions.Add("min", new BuildInFunction("min", 2));
			buildInFunctions.Add("pow", new BuildInFunction("pow", 2));
			buildInFunctions.Add("rand", new BuildInFunction("rand", 0));
			buildInFunctions.Add("round", new BuildInFunction("round", 2));
			buildInFunctions.Add("sign", new BuildInFunction("sign", 1));
			buildInFunctions.Add("sin", new BuildInFunction("sin", 1));
			buildInFunctions.Add("sin_rad", new BuildInFunction("sin_rad", 1));
			buildInFunctions.Add("sinh", new BuildInFunction("sinh", 1));
			buildInFunctions.Add("sinh_rad", new BuildInFunction("sinh_rad", 1));
			buildInFunctions.Add("sqrt", new BuildInFunction("sqrt", 1));
			buildInFunctions.Add("tan", new BuildInFunction("tan", 1));
			buildInFunctions.Add("tan_rad", new BuildInFunction("tan_rad", 1));
			buildInFunctions.Add("tanh", new BuildInFunction("tanh", 1));
			buildInFunctions.Add("tanh_rad", new BuildInFunction("tanh_rad", 1));
			buildInFunctions.Add("len", new BuildInFunction("len", 1));
			buildInFunctions.Add("mag", new BuildInFunction("mag", 1));
			buildInFunctions.Add("norm", new BuildInFunction("norm", 1));
			buildInFunctions.Add("dot", new BuildInFunction("dot", 2));
			buildInFunctions.Add("cross", new BuildInFunction("cross", 2));
			buildInFunctions.Add("all", new BuildInFunction("all", 1));
			buildInFunctions.Add("any", new BuildInFunction("any", 1));

			globals = new Dictionary<string, Object>();
			globals.Add("pi", Math.PI);
			globals.Add("π", Math.PI);
			globals.Add("e", Math.E);
			globals.Add("G", 6.67408E-11);
			globals.Add("Inf", double.PositiveInfinity);
			globals.Add("∞", double.PositiveInfinity);

			functions = new SortedDictionary<string, JITFunction>();
			JITFunction.Scan(functionDir, this);
		}

		public Dictionary<string, Object> Globals {
			get { return globals; }
			protected set { }
		}
		public Dictionary<string, Operator> Operators {
			get { return operators; }
			protected set { }
		}

		public Dictionary<string, BuildInFunction> BuildInFunctions {
			get { return buildInFunctions; }
			protected set { }
		}

		public SortedDictionary<string, JITFunction> Functions {
			get { return functions; }
			protected set { }
		}

		public string Run(string functionId) {
			if(!functions.ContainsKey(functionId))
				throw new Exception("Function not found: "+ functionId);

			JITFunction f = functions[functionId];
			if(f.InError) {
				throw new Exception(f.ErrorString);
			}

			List<Object> r = functions[functionId].Execute(new List<Object>());

			if(f.InError)
				throw new Exception(f.ErrorString);

			return FormatResult(f, r);
		}

		public List<Object> Run(JITFunction f) {
			if(f.InError) {
				throw new Exception(f.ErrorString);
			}

			List<Object> r = f.Execute(new List<Object>());

			if(f.InError)
				throw new Exception(f.ErrorString);

			return r;
		}

		public Object RunExpression(string expression) {
			JITExpression e = new JITExpression(expression, this);
			return e.Execute();
		}
		
		public static string FormatVar(Object var) {
			if(var.GetType() == typeof(Object[])) {
				Object[] list = (Object[]) var;
				string result = "[";
				for(int i=0; i<list.Length-1; i++)
					result += FormatVar(list[i]) + ", ";
				result += FormatVar(list[list.Length-1]) + "]";
				return result;
			} else {
				return ((double)var).ToString(System.Globalization.CultureInfo.InvariantCulture);
			}
		}

		public static string FormatResult(JITFunction f, List<Object> result) {
			string str = "";
			for(int i=0; i<result.Count-1; i++)
				str += f.Outs[i] +" = "+ FormatVar(result[i]) +", ";
			str += f.Outs[result.Count-1] +" = "+ FormatVar(result[result.Count-1]);
			return str;
		}

		public static void Main(string[] args) {
			string filename;
			if(args[0] == "-v") {
				Kerbulator.DEBUG = true;
				filename = args[1];
			} else {
				filename = args[0];
			}

			Kerbulator k = new Kerbulator("./tests");
			if(filename.EndsWith(".test")) {
				Console.WriteLine("Running unit tests in "+ filename);
				StreamReader file = File.OpenText(filename);
				string line;
				while( (line = file.ReadLine()) != null ) {
					if(line.Trim().StartsWith("#"))
						continue;

					string[] parts = line.Split(';');
					string expression = parts[0].Trim();
					string expectedResult = parts[1].Trim();

					try {
						string r = FormatVar( k.RunExpression(expression) );
						if(r.Equals(expectedResult))
							Console.WriteLine(expression +" = "+ r +" [PASS]");
						else
							Console.WriteLine(expression +" = "+ r +" [FAIL] "+ expectedResult);
					} catch(Exception e) {
						if(expectedResult == "ERROR")
							Console.WriteLine(expression +" = ERROR [PASS]");
						else
							Console.WriteLine(expression +" = ERROR [FAIL] "+ e.Message);
					}
				}
				file.Close();
				return;
			} else {
				Console.WriteLine("Running file "+ filename +"\n");
				Console.WriteLine(k.Run(filename), "\n");
			}
		}
	}
}
