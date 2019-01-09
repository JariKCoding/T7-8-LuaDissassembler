using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaDecompiler.LuaDecompile
{
    class LuaOperators
    {
        public static LuaDecompiler.DecompiledOPCode Return(LuaFile.LuaFunction function, LuaFile.LuaOPCode opCode, int index)
        {
            if (index > 0)
            {
                if (function.OPCodes[index - 1].OPCode == 0x16)
                    return new LuaDecompiler.DecompiledOPCode(LuaDecompiler.opCodeType.empty, "");
            }
            if (opCode.B > 1)
            {
                string registers = opCode.A.ToString();
                string returns = function.Registers[opCode.A];
                for (int i = opCode.A + 1; i <= opCode.A + opCode.B - 2; i++)
                {
                    registers += ", " + i;
                    returns += ", " + function.Registers[i];
                }
                return new LuaDecompiler.DecompiledOPCode(LuaDecompiler.opCodeType.String, "return " + returns);
            }
            else
            {
                if (index < function.OPCodes.Count - 1)
                    return new LuaDecompiler.DecompiledOPCode(LuaDecompiler.opCodeType.String, "return");
            }
            return new LuaDecompiler.DecompiledOPCode(LuaDecompiler.opCodeType.empty, "");
        }

        public static void Length(LuaFile.LuaFunction function, LuaFile.LuaOPCode opCode)
        {
            function.Registers[opCode.A] = "#" + function.Registers[opCode.B];
        }

        public static LuaDecompiler.DecompiledOPCode Add(LuaFile.LuaFunction function, LuaFile.LuaOPCode opCode)
        {
            return DoOperator(function, opCode, "+");
        }

        public static LuaDecompiler.DecompiledOPCode AddBackWards(LuaFile.LuaFunction function, LuaFile.LuaOPCode opCode)
        {
            return DoOperatorBackWards(function, opCode, "+");
        }

        public static LuaDecompiler.DecompiledOPCode Subtract(LuaFile.LuaFunction function, LuaFile.LuaOPCode opCode)
        {
            return DoOperator(function, opCode, "-");
        }

        public static LuaDecompiler.DecompiledOPCode SubtractBackWards(LuaFile.LuaFunction function, LuaFile.LuaOPCode opCode)
        {
            return DoOperatorBackWards(function, opCode, "-");
        }

        public static LuaDecompiler.DecompiledOPCode Multiply(LuaFile.LuaFunction function, LuaFile.LuaOPCode opCode)
        {
            return DoOperator(function, opCode, "*");
        }

        public static LuaDecompiler.DecompiledOPCode MultiplyBackWards(LuaFile.LuaFunction function, LuaFile.LuaOPCode opCode)
        {
            return DoOperatorBackWards(function, opCode, "*");
        }

        public static LuaDecompiler.DecompiledOPCode Divide(LuaFile.LuaFunction function, LuaFile.LuaOPCode opCode)
        {
            return DoOperator(function, opCode, "/");
        }

        public static LuaDecompiler.DecompiledOPCode DivideBackWards(LuaFile.LuaFunction function, LuaFile.LuaOPCode opCode)
        {
            return DoOperatorBackWards(function, opCode, "/");
        }

        public static LuaDecompiler.DecompiledOPCode Modulo(LuaFile.LuaFunction function, LuaFile.LuaOPCode opCode)
        {
            return DoOperator(function, opCode, "%");
        }

        public static LuaDecompiler.DecompiledOPCode ModuloBackWards(LuaFile.LuaFunction function, LuaFile.LuaOPCode opCode)
        {
            return DoOperatorBackWards(function, opCode, "%");
        }

        public static LuaDecompiler.DecompiledOPCode Power(LuaFile.LuaFunction function, LuaFile.LuaOPCode opCode)
        {
            return DoOperator(function, opCode, "^");
        }

        public static LuaDecompiler.DecompiledOPCode PowerBackWards(LuaFile.LuaFunction function, LuaFile.LuaOPCode opCode)
        {
            return DoOperatorBackWards(function, opCode, "^");
        }

        public static LuaDecompiler.DecompiledOPCode DoOperator(LuaFile.LuaFunction function, LuaFile.LuaOPCode opCode, string Operator)
        {
            LuaDecompiler.DecompiledOPCode str;
            string returnVal = function.getNewReturnVal();
            if (opCode.C > 255)
            {
                str = new LuaDecompiler.DecompiledOPCode(LuaDecompiler.opCodeType.String, String.Format("local {0} = {1} {2} {3}",
                    returnVal,
                    function.Strings[opCode.C - 256].String,
                    Operator,
                    function.Registers[opCode.B]));
            }
            else
            {
                str = new LuaDecompiler.DecompiledOPCode(LuaDecompiler.opCodeType.String, String.Format("local {0} = {1} {2} {3}",
                    returnVal,
                    function.Registers[opCode.C],
                    Operator,
                    function.Registers[opCode.B]));
            }

            function.Registers[opCode.A] = returnVal;
            return str;
        }

        public static LuaDecompiler.DecompiledOPCode DoOperatorBackWards(LuaFile.LuaFunction function, LuaFile.LuaOPCode opCode, string Operator)
        {
            string returnVal = function.getNewReturnVal();
            LuaDecompiler.DecompiledOPCode str = new LuaDecompiler.DecompiledOPCode(LuaDecompiler.opCodeType.String, String.Format("local {0} = {1} {2} {3}",
                returnVal,
                function.Strings[opCode.B].String,
                Operator,
                function.Registers[opCode.C]));
            function.Registers[opCode.A] = returnVal;
            return str;
        }

        public static void UnaryMinus(LuaFile.LuaFunction function, LuaFile.LuaOPCode opCode)
        {
            function.Registers[opCode.A] = "-" + function.Registers[opCode.B];
        }

        public static LuaDecompiler.DecompiledOPCode ShiftLeft(LuaFile.LuaFunction function, LuaFile.LuaOPCode opCode)
        {
            return new LuaDecompiler.DecompiledOPCode(LuaDecompiler.opCodeType.String,"-- Unhandled OP: (OPCODE_LEFT_SHIFT)");
        }

        public static LuaDecompiler.DecompiledOPCode ShiftLeftBackwards(LuaFile.LuaFunction function, LuaFile.LuaOPCode opCode)
        {
            return new LuaDecompiler.DecompiledOPCode(LuaDecompiler.opCodeType.String,"-- Unhandled OP: (OPCODE_LEFT_SHIFT_BK)");
        }

        public static LuaDecompiler.DecompiledOPCode BinaryAnd(LuaFile.LuaFunction function, LuaFile.LuaOPCode opCode)
        {
            return new LuaDecompiler.DecompiledOPCode(LuaDecompiler.opCodeType.String,"-- Unhandled OP: (OPCODE_BIT_AND)");
        }

        public static LuaDecompiler.DecompiledOPCode BinaryOr(LuaFile.LuaFunction function, LuaFile.LuaOPCode opCode)
        {
            return new LuaDecompiler.DecompiledOPCode(LuaDecompiler.opCodeType.String,"-- Unhandled OP: (OPCODE_BIT_OR)");
        }

        public static LuaDecompiler.DecompiledOPCode TestSet(LuaFile.LuaFunction function, LuaFile.LuaOPCode opCode)
        {
            return new LuaDecompiler.DecompiledOPCode(LuaDecompiler.opCodeType.String,"-- Unhandled OP: (OPCODE_TESTSET)");
        }

        public static LuaDecompiler.DecompiledOPCode Close(LuaFile.LuaFunction function, LuaFile.LuaOPCode opCode)
        {
            return new LuaDecompiler.DecompiledOPCode(LuaDecompiler.opCodeType.String, String.Format("-- Unhandled OP: (OPCODE_CLOSE) A: {0}, B: {1}, C: {2}, Bx: {3}", opCode.A, opCode.B, opCode.C, opCode.Bx));
        }

        public static LuaDecompiler.DecompiledOPCode VarArg(LuaFile.LuaFunction function, LuaFile.LuaOPCode opCode)
        {
            return new LuaDecompiler.DecompiledOPCode(LuaDecompiler.opCodeType.String,"-- Unhandled OP: (OPCODE_VARARG)");
        }

        public static LuaDecompiler.DecompiledOPCode NotR1(LuaFile.LuaFunction function, LuaFile.LuaOPCode opCode)
        {
            return new LuaDecompiler.DecompiledOPCode(LuaDecompiler.opCodeType.String,"-- Unhandled OP: (OPCODE_NOT_R1)");
        }

        public static LuaDecompiler.DecompiledOPCode SetupVal(LuaFile.LuaFunction function, LuaFile.LuaOPCode opCode)
        {
            return new LuaDecompiler.DecompiledOPCode(LuaDecompiler.opCodeType.String, "-- Unhandled OP: (OPCODE_SETUPVAL)");
        }
    }
}
