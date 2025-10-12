using NCalc;
using System;

namespace OSaft.calculator
{
    internal class Calculator
    {
        public static void StringToAlgebra(string[] args)
        {
            string equation = "";
            for(int i = 0; i < args.Length; i++)
            {
                equation = equation + args[i];
            }
            Expression expression = new Expression(equation);
            try
            {
                var result = expression.Evaluate();
                Console.WriteLine($"Valid expression. Result = {result}");
            }
            catch (EvaluationException ex)
            {
                Console.WriteLine($"Invalid expression: {ex.Message}");
            }
            return;
        }
    }
}
