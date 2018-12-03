using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaDecompiler
{
    class LuaConditions
    {
        public static void IfIsTrueFalse(LuaFile.LuaFunction function, LuaFile.LuaOPCode opCode)
        {
            function.DisassebleStrings.Add(String.Format("if r({0}) == {1}, skip next opcode // if {2}{3} then skip next line",
                opCode.A,
                (opCode.C == 1) ? "false" : "true",
                (opCode.C == 1) ? "not " : "",
                function.Registers[opCode.A]));
        }

        public static void IfIsEqual(LuaFile.LuaFunction function, LuaFile.LuaOPCode opCode)
        {
            if(opCode.C > 255)
            {
                function.DisassebleStrings.Add(String.Format("if {4}r({0}) == c[{1}], skip next opcode // if {4}{2} == {3} then skip next line",
                    opCode.B,
                    opCode.C - 256,
                    function.Registers[opCode.B],
                    function.Strings[opCode.C - 256].getString(),
                    (opCode.A == 1) ? "not " : ""));
            }
            else
            {
                function.DisassebleStrings.Add(String.Format("if {4}r({0}) == r({1}), skip next opcode // if {4}{2} == {3} then skip next line",
                    opCode.B,
                    opCode.C,
                    function.Registers[opCode.B],
                    function.Registers[opCode.C],
                    (opCode.A == 1) ? "not " : ""));
            }
        }

        public static void SkipLines(LuaFile.LuaFunction function, LuaFile.LuaOPCode opCode, int index)
        {
            string suffix = "";
            if (opCode.B == 0)
            {
                if (function.OPCodes[index + opCode.C + 2].OPCode == 0xE && !function.foreachPositions.Contains(index + opCode.C + 2))
                {
                    int baseVal = function.OPCodes[index + opCode.C + 2].A + 3;
                    function.Registers[baseVal] = "index" + ((function.forLoopCount > 0) ? function.forLoopCount.ToString() : "");
                    function.Registers[baseVal + 1] = "value" + ((function.forLoopCount > 0) ? function.forLoopCount.ToString() : "");
                    function.forLoopCount++;
                    suffix = " + start of foreach loop";
                    function.foreachPositions.Add(index + opCode.C + 2);
                }
                function.DisassebleStrings.Add(String.Format("skip the next [{0}] opcodes // advance {0} lines{1}",
                    opCode.C + 1,
                    suffix));
            }
            else
            {
                if (function.OPCodes[index + opCode.sBx + 1].OPCode == 0xE && !function.foreachPositions.Contains(index + opCode.sBx + 1))
                {
                    int baseVal = function.OPCodes[index + opCode.sBx + 1].A + 3;
                    function.Registers[baseVal] = "index" + ((function.forLoopCount > 0) ? function.forLoopCount.ToString() : "");
                    function.Registers[baseVal + 1] = "value" + ((function.forLoopCount > 0) ? function.forLoopCount.ToString() : "");
                    function.forLoopCount++;
                    suffix = " + start of foreach loop";
                    function.foreachPositions.Add(index + opCode.sBx + 1);
                }
                function.DisassebleStrings.Add(String.Format("skip the next [{0}] opcodes // advance {0} lines",
                    opCode.sBx));
            }
            

        }

        public static void Not(LuaFile.LuaFunction function, LuaFile.LuaOPCode opCode)
        {
            function.DisassebleStrings.Add(String.Format("r({0}) = not r({1}) // {2} = not {3}",
                opCode.A,
                opCode.B,
                "returnval" + function.returnValCount,
                function.Registers[opCode.B]));
            function.Registers[opCode.A] = "returnval" + function.returnValCount;
            function.returnValCount++;
        }

        public static void IfIsEqualBackwards(LuaFile.LuaFunction function, LuaFile.LuaOPCode opCode)
        {
            function.DisassebleStrings.Add("; Unhandled OP: (OPCODE_EQ_BK)");
        }

        public static void LargerThan(LuaFile.LuaFunction function, LuaFile.LuaOPCode opCode)
        {
            if (opCode.C > 255)
            {
                function.DisassebleStrings.Add(String.Format("if {4}r({0}) > c[{1}], skip next opcode // if {4}{2} > {3} then skip next line",
                    opCode.B,
                    opCode.C - 256,
                    function.Registers[opCode.B],
                    function.Strings[opCode.C - 256].getString(),
                    (opCode.A == 1) ? "not " : ""));
            }
            else
            {
                function.DisassebleStrings.Add(String.Format("if {4}r({0}) > r({1}), skip next opcode // if {4}{2} > {3} then skip next line",
                    opCode.B,
                    opCode.C,
                    function.Registers[opCode.B],
                    function.Registers[opCode.C],
                    (opCode.A == 0) ? "not " : ""));
            }   
        }

        public static void LargerThanBackwards(LuaFile.LuaFunction function, LuaFile.LuaOPCode opCode)
        {
            if (opCode.C > 255)
            {
                function.DisassebleStrings.Add(String.Format("if {4}r({0}) > c[{1}], skip next opcode // if {4}{2} > {3} then skip next line",
                    opCode.B,
                    opCode.C - 256,
                    function.Strings[opCode.B].getString(),
                    function.Strings[opCode.C - 256].getString(),
                    (opCode.A == 0) ? "not " : ""));
            }
            else
            {
                function.DisassebleStrings.Add(String.Format("if {4}r({0}) > c[{1}], skip next opcode // if {4}{2} > {3} then skip next line",
                    opCode.B,
                    opCode.C,
                    function.Strings[opCode.B].getString(),
                    function.Registers[opCode.C],
                    (opCode.A == 0) ? "not " : ""));
            }
        }

        public static void LargerOrEqualThan(LuaFile.LuaFunction function, LuaFile.LuaOPCode opCode)
        {
            if (opCode.C > 255)
            {
                function.DisassebleStrings.Add(String.Format("if {4}r({0}) >= c[{1}], skip next opcode // if {4}{2} >= {3} then skip next line",
                    opCode.B,
                    opCode.C - 256,
                    function.Registers[opCode.B],
                    function.Strings[opCode.C - 256].getString(),
                    (opCode.A == 1) ? "not " : ""));
            }
            else
            {
                function.DisassebleStrings.Add(String.Format("if {4}r({0}) >= r({1}), skip next opcode // if {4}{2} >= {3} then skip next line",
                    opCode.B,
                    opCode.C,
                    function.Registers[opCode.B],
                    function.Registers[opCode.C],
                    (opCode.A == 0) ? "not " : ""));
            }
        }

        public static void LargerOrEqualThanBackwards(LuaFile.LuaFunction function, LuaFile.LuaOPCode opCode)
        {
            if (opCode.C > 255)
            {
                function.DisassebleStrings.Add(String.Format("if {4}r({0}) >= c[{1}], skip next opcode // if {4}{2} >= {3} then skip next line",
                    opCode.B,
                    opCode.C - 256,
                    function.Strings[opCode.B].getString(),
                    function.Strings[opCode.C - 256].getString(),
                    (opCode.A == 0) ? "not " : ""));
            }
            else
            {
                function.DisassebleStrings.Add(String.Format("if {4}r({0}) >= c[{1}], skip next opcode // if {4}{2} >= {3} then skip next line",
                    opCode.B,
                    opCode.C,
                    function.Strings[opCode.B].getString(),
                    function.Registers[opCode.C],
                    (opCode.A == 0) ? "not " : ""));
            }
        }
    }
}
