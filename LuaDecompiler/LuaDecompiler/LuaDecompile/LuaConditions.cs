using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaDecompiler.LuaDecompile
{
    class LuaConditions
    {
        private static string orString { get; set; }
        private static int codeBlockStart { get; set; }

        public static LuaDecompiler.DecompiledOPCode IfIsTrueFalse(LuaFile.LuaFunction function, LuaFile.LuaOPCode opCode, int index)
        {
            return new LuaDecompiler.DecompiledOPCode(LuaDecompiler.opCodeType.skipLines, String.Format("if {0}{1} then",
                    (opCode.C == 1) ? "not " : "",
                    function.Registers[opCode.A]), getSkipLines(function, index + 1));
        }

        public static LuaDecompiler.DecompiledOPCode IfIsEqual(LuaFile.LuaFunction function, LuaFile.LuaOPCode opCode, int index)
        {
            return doCondition(function, index, "==");
        }

        public static LuaDecompiler.DecompiledOPCode Not(LuaFile.LuaFunction function, LuaFile.LuaOPCode opCode)
        {
            string returnVal = function.getNewReturnVal();
            LuaDecompiler.DecompiledOPCode str = new LuaDecompiler.DecompiledOPCode(LuaDecompiler.opCodeType.String, String.Format("local {0} = not {1}",
                returnVal,
                function.Registers[opCode.B]));
            function.Registers[opCode.A] = returnVal;
            return str;
        }

        public static LuaDecompiler.DecompiledOPCode IfIsEqualBackwards(LuaFile.LuaFunction function, LuaFile.LuaOPCode opCode, int index)
        {
            return doConditionBackward(function, index, "==");
        }

        public static LuaDecompiler.DecompiledOPCode LessThan(LuaFile.LuaFunction function, LuaFile.LuaOPCode opCode, int index)
        {
            return doCondition(function, index, "<");
        }

        public static LuaDecompiler.DecompiledOPCode LessThanBackwards(LuaFile.LuaFunction function, LuaFile.LuaOPCode opCode, int index)
        {
            return doConditionBackward(function, index, "<");
        }

        public static LuaDecompiler.DecompiledOPCode LessOrEqualThan(LuaFile.LuaFunction function, LuaFile.LuaOPCode opCode, int index)
        {
            return doCondition(function, index, "<=");
        }

        public static LuaDecompiler.DecompiledOPCode LessOrEqualThanBackwards(LuaFile.LuaFunction function, LuaFile.LuaOPCode opCode, int index)
        {
            return doConditionBackward(function, index, "<=");
        }

        private static LuaDecompiler.DecompiledOPCode doCondition(LuaFile.LuaFunction function, int index, string Oper)
        {
            LuaFile.LuaOPCode opCode = function.OPCodes[index];
            try
            {
                if (orString.Length < 1)
                {
                    orString = "";
                    codeBlockStart = -1;
                }
            }
            catch
            {
                orString = "";
                codeBlockStart = -1;
            }
            bool conditionToReg = false;
            if (function.OPCodes.Count > index + 3)
            {
                if(function.OPCodes[index + 2].OPCode == 0xD && function.OPCodes[index + 3].OPCode == 0xD)
                {
                    conditionToReg = true;
                }
            }
            if (isConditioOpCode(function.OPCodes[index + getSkipLines(function, index + 1) - 2].OPCode) && getSkipLines(function, index + 1) > 2)
            {
                if (orString.Length < 1)
                    orString = "";
                string cond;
                if (index + getSkipLines(function, index + 1) < codeBlockStart)
                {
                    cond = "and";
                    opCode.A = (byte)(1 - opCode.A);
                }
                else
                    cond = "or";
                if (opCode.C > 255)
                    orString += String.Format("{3}{0} {2} {1} {4} ", function.Registers[opCode.B], function.Strings[opCode.C - 256].getString(), Oper, (opCode.A == 0) ? "not " : "", cond);
                else
                    orString += String.Format("{3}{0} {2} {1} {4} ", function.Registers[opCode.B], function.Registers[opCode.C], Oper, (opCode.A == 0) ? "not " : "", cond);

                if (codeBlockStart == -1)
                    codeBlockStart = index + getSkipLines(function, index + 1);
                return new LuaDecompiler.DecompiledOPCode(LuaDecompiler.opCodeType.orCondition, orString);
            }
            LuaDecompiler.DecompiledOPCode str;
            int elseLines = 0;
            if (function.OPCodes[index + getSkipLines(function, index + 1) + 1].OPCode == 0x1C)
            {
                if (!isConditioOpCode(function.OPCodes[index + getSkipLines(function, index + 1) - 2].OPCode))
                {
                    elseLines = getSkipLines(function, index + getSkipLines(function, index + 1) + 1);
                }
            }
            if (opCode.C > 255)
            {
                if (conditionToReg)
                {
                    string returnVal = function.getNewReturnVal();
                    str = new LuaDecompiler.DecompiledOPCode(LuaDecompiler.opCodeType.conditionToReg, String.Format("local {5} = ({4}{2}{0} {3} {1})",
                        function.Registers[opCode.B],
                        function.Strings[opCode.C - 256].getString(),
                        (opCode.A == 1) ? "not " : "",
                        Oper, orString, returnVal));
                    function.Registers[function.OPCodes[index + 2].A] = returnVal;
                }
                else
                {
                    str = new LuaDecompiler.DecompiledOPCode(LuaDecompiler.opCodeType.skipLines, String.Format("if {4}{2}{0} {3} {1} then",
                        function.Registers[opCode.B],
                        function.Strings[opCode.C - 256].getString(),
                        (opCode.A == 1) ? "not " : "",
                        Oper, orString), getSkipLines(function, index + 1), elseLines);
                }
            }
            else
            {
                if (conditionToReg)
                {
                    string returnVal = function.getNewReturnVal();
                    str = new LuaDecompiler.DecompiledOPCode(LuaDecompiler.opCodeType.conditionToReg, String.Format("local {5} = ({4}{2}{0} {3} {1})",
                        function.Registers[opCode.B],
                        function.Registers[opCode.C],
                        (opCode.A == 0) ? "not " : "",
                        Oper, orString, returnVal));
                    function.Registers[function.OPCodes[index + 2].A] = returnVal;
                }
                else
                {
                    str = new LuaDecompiler.DecompiledOPCode(LuaDecompiler.opCodeType.skipLines, String.Format("if {4}{2}{0} {3} {1} then",
                        function.Registers[opCode.B],
                        function.Registers[opCode.C],
                        (opCode.A == 0) ? "not " : "",
                        Oper, orString), getSkipLines(function, index + 1), elseLines);
                }
            }
            if (orString.Length > 0)
                orString = "";
            codeBlockStart = -1; 
            return str;
        }

        public static bool isConditioOpCode(int opCode)
        {
            if ((opCode >= 0x38 && opCode <= 0x3B) || opCode == 0x4F || opCode == 0x4 || opCode == 0x5 || opCode == 0x1)
                return true;
            return false;
        }

        private static LuaDecompiler.DecompiledOPCode doConditionBackward(LuaFile.LuaFunction function, int index, string Oper)
        {
            LuaFile.LuaOPCode opCode = function.OPCodes[index];
            try
            {
                if (orString.Length < 1)
                {
                    orString = "";
                    codeBlockStart = -1;
                }
            }
            catch
            {
                orString = "";
                codeBlockStart = -1;
            }
            bool conditionToReg = false;
            if (function.OPCodes.Count > index + 3)
            {
                if (function.OPCodes[index + 2].OPCode == 0xD && function.OPCodes[index + 3].OPCode == 0xD)
                {
                    conditionToReg = true;
                }
            }
            if (function.OPCodes.Count > index + 3)
            {
                if(function.OPCodes[index + 2].OPCode == 0xD && function.OPCodes[index + 3].OPCode == 0xD)
                {
                    if(function.OPCodes[index + 2].C == 1)
                    {
                        LuaDecompiler.DecompiledOPCode stri;
                        string returnVal = function.getNewReturnVal();
                        if (opCode.C > 255)
                        {
                            stri = new LuaDecompiler.DecompiledOPCode(LuaDecompiler.opCodeType.String, String.Format("{5} = ({4}{2}{0} {3} {1})",
                                function.Registers[opCode.B],
                                function.Strings[opCode.C - 256].getString(),
                                (opCode.A == 1) ? "not " : "",
                                Oper, orString, returnVal));
                        }
                        else
                        {
                            stri = new LuaDecompiler.DecompiledOPCode(LuaDecompiler.opCodeType.String, String.Format("{5} = ( {4}{2}{0} {3} {1})",
                                function.Strings[opCode.B].getString(),
                                function.Registers[opCode.C],
                                (opCode.A == 0) ? "not " : "",
                                Oper, orString, returnVal));
                        }
                        function.Registers[function.OPCodes[index + 2].A] = returnVal;
                        return stri;
                    }
                }
            }
            if (isConditioOpCode(function.OPCodes[index + getSkipLines(function, index + 1) - 2].OPCode))
            {
                if (orString.Length < 1)
                    orString = "";
                string cond;
                if (index + getSkipLines(function, index + 1) < codeBlockStart)
                {
                    cond = "and";
                    opCode.A = (byte)(1 - opCode.A);
                }
                else
                    cond = "or";
                if (opCode.C > 255)
                    orString += String.Format("{3}{0} {2} {1} {4} ", function.Registers[opCode.B], function.Strings[opCode.C - 256].getString(), Oper, (opCode.A == 0) ? "not " : "", cond);
                else
                    orString += String.Format("{3}{0} {2} {1} {4} ", function.Strings[opCode.B].getString(), function.Registers[opCode.C], Oper, (opCode.A == 0) ? "not " : "", cond);

                if (codeBlockStart == -1)
                    codeBlockStart = index + getSkipLines(function, index + 1);
                return new LuaDecompiler.DecompiledOPCode(LuaDecompiler.opCodeType.orCondition, orString);
            }
            if (function.OPCodes[index + getSkipLines(function, index + 1) - 1].OPCode == 0x1C)
            {
                if (!isConditioOpCode(function.OPCodes[index + getSkipLines(function, index + 1) - 2].OPCode))
                {
                    
                }
            }
            LuaDecompiler.DecompiledOPCode str;
            if (opCode.C > 255)
            {
                if (conditionToReg)
                {
                    string returnVal = function.getNewReturnVal();
                    str = new LuaDecompiler.DecompiledOPCode(LuaDecompiler.opCodeType.skipLines, String.Format("local {5} = ({4}{2}{0} {3} {1})",
                        function.Registers[opCode.B],
                        function.Strings[opCode.C - 256].getString(),
                        (opCode.A == 1) ? "not " : "",
                        Oper, orString, returnVal));
                    function.Registers[function.OPCodes[index + 2].A] = returnVal;
                }
                else
                {
                    str = new LuaDecompiler.DecompiledOPCode(LuaDecompiler.opCodeType.skipLines, String.Format("if {4}{2}{0} {3} {1} then",
                        function.Registers[opCode.B],
                        function.Strings[opCode.C - 256].getString(),
                        (opCode.A == 1) ? "not " : "",
                        Oper, orString), getSkipLines(function, index + 1));
                }   
            }
            else
            {
                if (conditionToReg)
                {
                    string returnVal = function.getNewReturnVal();
                    str = new LuaDecompiler.DecompiledOPCode(LuaDecompiler.opCodeType.skipLines, String.Format("local {5} = ({4}{2}{0} {3} {1})",
                        function.Strings[opCode.B].getString(),
                        function.Registers[opCode.C],
                        (opCode.A == 0) ? "not " : "",
                        Oper, orString, returnVal));
                    function.Registers[function.OPCodes[index + 2].A] = returnVal;
                }
                else
                {
                    str = new LuaDecompiler.DecompiledOPCode(LuaDecompiler.opCodeType.skipLines, String.Format("if {4}{2}{0} {3} {1} then",
                        function.Strings[opCode.B].getString(),
                        function.Registers[opCode.C],
                        (opCode.A == 0) ? "not " : "",
                        Oper, orString), getSkipLines(function, index + 1));
                }  
            }
            if (orString.Length > 0)
                orString = "";
            codeBlockStart = -1;
            return str;
        }

        private static int getSkipLines(LuaFile.LuaFunction function, int index)
        {
            if (function.OPCodes[index].B == 0)
                return function.OPCodes[index].C + 1;
            else
                return function.OPCodes[index].sBx;
        }

        public static LuaDecompiler.DecompiledOPCode SkipLines(LuaFile.LuaFunction function, LuaFile.LuaOPCode opCode, int index)
        {
            if(index + getSkipLines(function, index) < function.OPCodes.Count)
            {
                if (function.OPCodes[index + getSkipLines(function, index) + 1].OPCode == 0xE)
                {
                    int baseVal = function.OPCodes[index + opCode.C + 2].A + 3;
                    function.Registers[baseVal] = "index" + ((function.forLoopCount > 0) ? function.forLoopCount.ToString() : "");
                    function.Registers[baseVal + 1] = "value" + ((function.forLoopCount > 0) ? function.forLoopCount.ToString() : "");
                    function.forLoopCount++;
                    return new LuaDecompiler.DecompiledOPCode(LuaDecompiler.opCodeType.forEach, String.Format("for {0},{1} in {2}, {3}, {4} do", 
                        function.Registers[baseVal], function.Registers[baseVal + 1], function.Registers[baseVal - 3], function.Registers[baseVal - 2]
                        , function.Registers[baseVal - 1]), getSkipLines(function, index));
                }
            }
            if(index + getSkipLines(function, index) < function.OPCodes.Count)
            {
                if (function.OPCodes[index + getSkipLines(function, index)].OPCode == 0x1C)
                {
                    if(isConditioOpCode(function.OPCodes[index - 1].OPCode))
                    {
                        return new LuaDecompiler.DecompiledOPCode(LuaDecompiler.opCodeType.Else, "", getSkipLines(function, index), getSkipLines(function, index + getSkipLines(function, index)));
                    }
                }
            }
            return new LuaDecompiler.DecompiledOPCode(LuaDecompiler.opCodeType.empty, "");
        }
    }
}
