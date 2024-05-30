using Ink.Parsed;
using System;

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
				return ResultOfFunction(functionCall);
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


		public object ResultOfFunction(FunctionCall functionCall)
		{
			int totalArgs = functionCall.arguments != null ? functionCall.arguments.Count : 0;
			object[] args = new object[totalArgs];

			for (int i = 0; i < totalArgs; ++i)
			{
				args[i] = EvaluateAtRuntime(functionCall.arguments[i]);
			}

			// Check Ink built-in functions first
			if (FunctionCall.IsBuiltIn(functionCall.name))
			{
				return EvaluateSpecialNativeFunction(functionCall.name, args);
			}

			// Check for function written in Ink
			Container funcContainer = KnotContainerWithName(functionCall.name);
			if (funcContainer != null)
			{
				return EvaluateFunction(functionCall.name, args);
			}

			// Check for external functions 
			return EvaluateExternalFunction(functionCall.name, args);
		}

		public object EvaluateExternalFunction(string funcName, object[] args)
		{
			ExternalFunctionDef funcDef;

			bool foundExternal = _externals.TryGetValue(funcName, out funcDef);
			if (!foundExternal)
			{
				throw new System.Exception("External Function doesn't exist: '" + funcName + "'");
			}

			return funcDef.function(args);
		}

		public object EvaluateSpecialNativeFunction(string funcName, object[] args)
		{
			// Special Native Functions are just functions for Ink Lists,
			// so let's handle that directly.
			InkList list = TurnToInkList(args[0]);

			// These are the same calls Ink performs at NativeFunctionCall.cs
			switch (funcName)
			{
				case NativeFunctionCall.ListMin:
					return list.MinAsList();

				case NativeFunctionCall.ListMax:
					return list.MaxAsList();

				case NativeFunctionCall.All:
					return list.all;

				case NativeFunctionCall.Count:
					return list.Count;

				case NativeFunctionCall.ValueOfList:
					return list.maxItem.Value;

				case NativeFunctionCall.Invert:
					return list.inverse;
			}

			return null;
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
			switch (opName)
			{
				case NativeFunctionCall.And:
					return (bool)lhs && (bool)rhs;

				case NativeFunctionCall.Or:
					return (bool)lhs || (bool)rhs;

				case NativeFunctionCall.NotEquals:
					return !lhs.Equals(rhs);

				case NativeFunctionCall.Equal:
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

			if (!(target is Ink.Runtime.Value))
			{
				return null;
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
				// Modifying Operations
				case NativeFunctionCall.Add:
					return list.Union(otherList);

				case NativeFunctionCall.Subtract:
					return list.Without(otherList);

				case NativeFunctionCall.Intersect:
					return list.Intersect(otherList);


				// Boolean checks
				case NativeFunctionCall.NotEquals:
					return !list.Equals(otherList);

				case NativeFunctionCall.Equal:
					return list.Equals(otherList);

				case NativeFunctionCall.Greater:
					return list.GreaterThan(otherList);

				case NativeFunctionCall.GreaterThanOrEquals:
					return list.GreaterThanOrEquals(otherList);

				case NativeFunctionCall.Less:
					return list.LessThan(otherList);

				case NativeFunctionCall.LessThanOrEquals:
					return list.LessThanOrEquals(otherList);

				case NativeFunctionCall.Has:
					return list.Contains(otherList);

				case NativeFunctionCall.Hasnt:
					return !list.Contains(otherList);
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