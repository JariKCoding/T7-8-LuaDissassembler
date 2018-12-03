using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaDecompiler
{
    class LuaLoops
    {
        public static void StartForLoop(LuaFile.LuaFunction function, LuaFile.LuaOPCode opCode)
        {
            function.Registers[opCode.A + 3] = "index" + ((function.forLoopCount > 0) ? function.forLoopCount.ToString() : "");
            function.forLoopCount++;
            function.DisassembleStrings.Add(String.Format("for {6}=r({0}), {6} < r({1}), r({2}) do // for {6}={3}, {4}, {5} do",
                opCode.A, opCode.A + 1, opCode.A + 2,
                function.Registers[opCode.A],
                function.Registers[opCode.A + 1],
                function.Registers[opCode.A + 2],
                function.Registers[opCode.A + 3]));
        }

        public static void EndForLoop(LuaFile.LuaFunction function, LuaFile.LuaOPCode opCode, int index)
        {
            if (opCode.B == 0)
            {
                function.DisassembleStrings.Add(String.Format("skip the next [{0}] opcodes // advance {0} lines",
                opCode.C + 1));
            }
            else
            {
                function.DisassembleStrings.Add(String.Format("skip the next [{0}] opcodes // advance {0} lines",
                    opCode.sBx));
            }   
        }

        public static void StartForEachLoop(LuaFile.LuaFunction function, LuaFile.LuaOPCode opCode)
        {
            function.DisassembleStrings.Add(String.Format("Start a foreach loop with val r({0}), stop: r({1}), inc: r({2}) // use these values: {3} and {4}",
                opCode.A,
                opCode.A + 1,
                opCode.A + 2,
                function.Registers[opCode.A + 3],
                function.Registers[opCode.A + 4]));
        }
    }
}
