using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaDecompiler
{
    class LuaOperators
    {
        public static void Return(LuaFile.LuaFunction function, LuaFile.LuaOPCode opCode, int index)
        {
            if (index > 0)
            {
                if(function.OPCodes[index - 1].OPCode == 0x16)
                    opCode.B = 1;
            }
            if (opCode.B > 1)
            {
                string registers = opCode.A.ToString();
                string returns = function.Registers[opCode.A];
                for(int i = opCode.A + 1; i <= opCode.A + opCode.B - 2; i++)
                {
                    registers += ", " + i;
                    returns += ", " + function.Registers[i];
                }
                function.DisassebleStrings.Add(String.Format("return r({0}) // {1}", registers, returns));
            } 
            else
                function.DisassebleStrings.Add("return");
        }

        public static void Length(LuaFile.LuaFunction function, LuaFile.LuaOPCode opCode)
        {
            function.Registers[opCode.A] = "#" + function.Registers[opCode.B];
            function.DisassebleStrings.Add(String.Format("r({0}) = len(r({1})) // {2}",
                opCode.A,
                opCode.B,
                function.Registers[opCode.B]));
        }

        public static void Add(LuaFile.LuaFunction function, LuaFile.LuaOPCode opCode)
        {
            DoOperator(function, opCode, "+");
        }

        public static void AddBackWards(LuaFile.LuaFunction function, LuaFile.LuaOPCode opCode)
        {
            DoOperatorBackWards(function, opCode, "+");
        }

        public static void Subtract(LuaFile.LuaFunction function, LuaFile.LuaOPCode opCode)
        {
            DoOperator(function, opCode, "-");
        }

        public static void SubtractBackWards(LuaFile.LuaFunction function, LuaFile.LuaOPCode opCode)
        {
            DoOperatorBackWards(function, opCode, "-");
        }

        public static void Multiply(LuaFile.LuaFunction function, LuaFile.LuaOPCode opCode)
        {
            DoOperator(function, opCode, "*");
        }

        public static void MultiplyBackWards(LuaFile.LuaFunction function, LuaFile.LuaOPCode opCode)
        {
            DoOperatorBackWards(function, opCode, "*");
        }

        public static void Divide(LuaFile.LuaFunction function, LuaFile.LuaOPCode opCode)
        {
            DoOperator(function, opCode, "/");
        }

        public static void DivideBackWards(LuaFile.LuaFunction function, LuaFile.LuaOPCode opCode)
        {
            DoOperatorBackWards(function, opCode, "/");
        }

        public static void Modulo(LuaFile.LuaFunction function, LuaFile.LuaOPCode opCode)
        {
            DoOperator(function, opCode, "%");
        }

        public static void ModuloBackWards(LuaFile.LuaFunction function, LuaFile.LuaOPCode opCode)
        {
            DoOperatorBackWards(function, opCode, "%");
        }

        public static void Power(LuaFile.LuaFunction function, LuaFile.LuaOPCode opCode)
        {
            DoOperator(function, opCode, "^");
        }

        public static void PowerBackWards(LuaFile.LuaFunction function, LuaFile.LuaOPCode opCode)
        {
            DoOperatorBackWards(function, opCode, "^");
        }

        public static void DoOperator(LuaFile.LuaFunction function, LuaFile.LuaOPCode opCode, string Operator)
        {
            if (opCode.C > 255)
            {
                function.DisassebleStrings.Add(String.Format("r({0}) = c[{1}] {6} r({2}) // {3} = {4} {6} {5}",
                    opCode.A,
                    opCode.C - 256,
                    opCode.B,
                    "returnval" + function.returnValCount,
                    function.Strings[opCode.C - 256].String,
                    function.Registers[opCode.B],
                    Operator));
            }
            else
            {
                function.DisassebleStrings.Add(String.Format("r({0}) = r({1}) {6} r({2}) // {3} = {4} {6} {5}",
                    opCode.A,
                    opCode.C,
                    opCode.B,
                    "returnval" + function.returnValCount,
                    function.Registers[opCode.C],
                    function.Registers[opCode.B],
                    Operator));
            }

            function.Registers[opCode.A] = "returnval" + function.returnValCount;
            function.returnValCount++;
        }

        public static void DoOperatorBackWards(LuaFile.LuaFunction function, LuaFile.LuaOPCode opCode, string Operator)
        {
            function.Registers[opCode.A] = "returnval" + function.returnValCount;
            function.returnValCount++;
            function.DisassebleStrings.Add(String.Format("r({0}) = r({1}) {6} c({2}) // {3} = {4} {6} {5}",
                opCode.A,
                opCode.C,
                opCode.B,
                function.Registers[opCode.A],
                function.Registers[opCode.C],
                function.Strings[opCode.B].String,
                Operator));
        }

        public static void ShiftLeft(LuaFile.LuaFunction function, LuaFile.LuaOPCode opCode)
        {
            function.DisassebleStrings.Add("; Unhandled OP: (OPCODE_LEFT_SHIFT)");
        }

        public static void ShiftLeftBackwards(LuaFile.LuaFunction function, LuaFile.LuaOPCode opCode)
        {
            function.DisassebleStrings.Add("; Unhandled OP: (OPCODE_LEFT_SHIFT_BK)");
        }

        public static void BinaryAnd(LuaFile.LuaFunction function, LuaFile.LuaOPCode opCode)
        {
            function.DisassebleStrings.Add("; Unhandled OP: (OPCODE_BIT_AND)");
        }

        public static void BinaryOr(LuaFile.LuaFunction function, LuaFile.LuaOPCode opCode)
        {
            function.DisassebleStrings.Add("; Unhandled OP: (OPCODE_BIT_OR)");
        }

        public static void TestSet(LuaFile.LuaFunction function, LuaFile.LuaOPCode opCode)
        {
            function.DisassebleStrings.Add("; Unhandled OP: (OPCODE_TESTSET)");
        }

        public static void Close(LuaFile.LuaFunction function, LuaFile.LuaOPCode opCode)
        {
            function.DisassebleStrings.Add("; Unhandled OP: (OPCODE_CLOSE)");
        }

        public static void VarArg(LuaFile.LuaFunction function, LuaFile.LuaOPCode opCode)
        {
            function.DisassebleStrings.Add("; Unhandled OP: (OPCODE_VARARG)");
        }

        public static void NotR1(LuaFile.LuaFunction function, LuaFile.LuaOPCode opCode)
        {
            function.DisassebleStrings.Add("; Unhandled OP: (OPCODE_NOT_R1)");
        }

        public static void SetupVal(LuaFile.LuaFunction function, LuaFile.LuaOPCode opCode)
        {
            function.DisassebleStrings.Add("; Unhandled OP: (OPCODE_SETUPVAL)");
        }
    }
}
