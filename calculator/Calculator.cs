using System;
using System.Data;

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

            if(equation == "")
            {
                return;
            }

            DataTable table = new DataTable();
            var result = table.Compute(equation, "");
            Console.WriteLine($"Result: {result}");

            return;
        }
    }
}
