using Ink.Parsed;
using System;
using System.Collections.Generic;

namespace Ink.Runtime
{
	public partial class Story
	{
		public object EvaluateAtRuntime(Expression expr)
		{
			if (expr is Ink.Parsed.VariableReference variableReference)
			{
				return GetVariableReference(variableReference);
			}

			if (expr is Ink.Parsed.Number number)
			{
				return number.value;
			}

			if (expr is Ink.Parsed.StringExpression stringExpr)
			{
				return (string)stringExpr.ToString();
			}

			if (expr is FunctionCall functionCall)
			{
				// Check Ink built-in functions first
				if (FunctionCall.IsBuiltIn(functionCall.name))
				{
					// Cannot process built-in functions yet >:(
					return false;
				}

				List<object> args = new List<object>();
				int totalArgs = functionCall.arguments != null ? functionCall.arguments.Count : 0;

				for (int i = 0; i < totalArgs; ++i)
				{
					args.Add(EvaluateAtRuntime(functionCall.arguments[i]));
				}

				// Get the content that we need to run
				Container funcContainer = KnotContainerWithName(functionCall.name);
				return (funcContainer != null) ? EvaluateFunction(functionCall.name, args.ToArray())
												: EvaluateExternalFunction(functionCall.name, args.ToArray());
			}

			if (expr is BinaryExpression binaryExpression)
			{
				string opName = binaryExpression.opName; // opName is "and" or "or" or "mod" or "has" or ...
														 // Evaluate each side of the expression recursively
				object lhs = EvaluateAtRuntime(binaryExpression.leftExpression);
				object rhs = EvaluateAtRuntime(binaryExpression.rightExpression);
				// Do some magic, defined elsewhere, to determine the runtime typs of `lhs` and `rhs` and do whatever "opName" tells us to do with the result
				return ResultOfBinaryOperation(lhs, rhs, opName);
			}

			if (expr is UnaryExpression unary)
			{
				object result = EvaluateAtRuntime(unary.innerExpression);
				return ResultOfUnaryOperation(result, unary.op);
			}

			if (expr.typeName.Equals("List"))
			{
				return GetListReference(expr);
			}

			return false;
		}

		public object EvaluateExternalFunction(string funcName, object[] args)
		{
			ExternalFunctionDef funcDef;

			bool foundExternal = _externals.TryGetValue(funcName, out funcDef);
			if (!foundExternal)
			{
				throw new System.Exception("Function doesn't exist: '" + funcName + "'");
			}

			return funcDef.function(args);
		}

		protected object GetVariableReference(Ink.Parsed.VariableReference variableReference)
		{
			string varName = variableReference.name;

			if (varName.Equals("True"))
			{
				return true;
			}

			if (varName.Equals("False"))
			{
				return false;
			}
			// Try to get the variable the usual Ink way.
			object variable = variablesState[varName];

			return variable != null
				? variable
				: variablesState.GetVariableWithName(varName); // Try a different way, it may be an InkList Item.
		}

		protected object GetListReference(Expression expr)
		{
			string varName = expr.ToString();
			if (varName[0] == '(' && varName[varName.Length - 1] == ')')
			{
				varName = varName.Replace("(", "").Replace(")", "");
				string[] listArgs = varName.Split(',');

				if (listArgs.Length <= 0)
				{
					return null;
				}

				InkList inkList = TurnToInkList(variablesState.GetVariableWithName(listArgs[0])); // Only way to get a InkList Item.
				if (listArgs.Length == 1)
				{
					return inkList;
				}

				for (int i = 1; i < listArgs.Length; ++i)
				{
					InkList otherList = TurnToInkList(variablesState.GetVariableWithName(listArgs[i]));
					inkList = inkList.Union(otherList);
				}

				return inkList;
			}

			return null;
		}

		protected object ResultOfBinaryOperation(object lhs, object rhs, string opName)
		{
			// Check Ink Lists first
			InkList thisList = TurnToInkList(lhs);
			InkList otherList = TurnToInkList(rhs);
			if (thisList != null && otherList != null)
			{
				return ResultOfInkListOperation(thisList, otherList, opName);
			}
			else if (thisList != null || otherList != null)
			{
				return null;
			}

			// Check Generic Functions
			if (opName == NativeFunctionCall.And)
			{
				return (bool)lhs && (bool)rhs;
			}
			else if (opName == NativeFunctionCall.Or)
			{
				return (bool)lhs || (bool)rhs;
			}
			else if (opName == NativeFunctionCall.NotEquals)
			{
				return !lhs.Equals(rhs);
			}
			else if (opName == NativeFunctionCall.Equal)
			{
				return lhs.Equals(rhs);
			}
			else if (opName == NativeFunctionCall.Has)
			{
				return lhs.Equals(rhs);
			}
			else if (opName == NativeFunctionCall.Hasnt)
			{
				return lhs.Equals(rhs);
			}


			// Check Numbers
			float leftValue = Convert.ToSingle(lhs);
			float rightValue = Convert.ToSingle(rhs);

			return ResultOfNumericOperation(leftValue, rightValue, opName);
		}

		protected InkList TurnToInkList(object target)
		{
			if (target is InkList)
			{
				return (InkList)target;
			}

			object value = ((Ink.Runtime.Value)target).valueObject;
			if (value is InkList)
			{
				return (InkList)value;
			}

			return null;
		}

		protected object ResultOfInkListOperation(InkList list, InkList otherList, string opName)
		{
			switch (opName)
			{
				case NativeFunctionCall.Has:
					return list.Contains(otherList);

				case NativeFunctionCall.Hasnt:
					return !list.Contains(otherList);

				case NativeFunctionCall.Intersect:
					return list.Intersect(otherList);

			}
			return null;
		}

		protected object ResultOfNumericOperation(float leftValue, float rightValue, string opName)
		{
			switch (opName)
			{
				case NativeFunctionCall.Add:
					return leftValue + rightValue;

				case NativeFunctionCall.Subtract:
					return leftValue - rightValue;

				case NativeFunctionCall.Multiply:
					return leftValue * rightValue;

				case NativeFunctionCall.Divide:
					return leftValue / rightValue;

				case NativeFunctionCall.Mod:
					return leftValue % rightValue;

				case NativeFunctionCall.Greater:
					return leftValue > rightValue;

				case NativeFunctionCall.GreaterThanOrEquals:
					return leftValue >= rightValue;

				case NativeFunctionCall.Less:
					return leftValue < rightValue;

				case NativeFunctionCall.LessThanOrEquals:
					return leftValue <= rightValue;

			}
			return null;
		}

		protected object ResultOfUnaryOperation(object value, string opName)
		{
			if (opName == NativeFunctionCall.Not || opName == "not")
			{
				return !((bool)value);
			}

			return value; // Unknown opName.
		}
	}

}