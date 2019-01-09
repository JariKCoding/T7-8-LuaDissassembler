using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaDecompiler.LuaDecompile
{
    class LuaLoops
    {
        public static LuaDecompiler.DecompiledOPCode StartForLoop(LuaFile.LuaFunction function, LuaFile.LuaOPCode opCode)
        {
            function.Registers[opCode.A + 3] = "index" + ((function.forLoopCount > 0) ? function.forLoopCount.ToString() : "");
            function.forLoopCount++;
            return new LuaDecompiler.DecompiledOPCode(LuaDecompiler.opCodeType.forEach, String.Format("for {3}={0}, {1}, {2} do",
                function.Registers[opCode.A],
                function.Registers[opCode.A + 1],
                function.Registers[opCode.A + 2],
                function.Registers[opCode.A + 3]), opCode.C + 2);
        }
    }
}
