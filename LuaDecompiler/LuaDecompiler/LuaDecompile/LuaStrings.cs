using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaDecompiler.LuaDecompile
{
    class LuaStrings
    {
        public static void ConnectWithDot(LuaFile.LuaFunction function, LuaFile.LuaOPCode opCode)
        {
            function.Registers[opCode.A] = function.Registers[opCode.B] + "." + function.Strings[opCode.C].String;
        }

        public static LuaDecompiler.DecompiledOPCode SetField(LuaFile.LuaFunction function, LuaFile.LuaOPCode opCode, int index, bool isString = false)
        {
            if (opCode.C > 255)
            {
                return new LuaDecompiler.DecompiledOPCode(LuaDecompiler.opCodeType.String, (String.Format("{0}.{1} = {2}",
                    function.Registers[opCode.A],
                    function.Strings[opCode.B].String,
                    function.Strings[opCode.C - 256].getString())));
            }
            else
            {
                if (function.Registers[opCode.C].Contains("__FUNC")
                    && (function.OPCodes[index - 1].OPCode == 0x4A || function.OPCodes[index - 1].OPCode == 0x54)
                    && (function.Registers[opCode.A].Substring(0, 3) == "CoD" || function.Registers[opCode.A].Substring(0, 3) == "LUI"))
                {
                    for (int i = index; i > 0; i--)
                    {
                        if (function.OPCodes[i].OPCode == 0x4A)
                        {
                            function.subFunctions[function.OPCodes[i].Bx].functionName = function.Registers[opCode.A] + "." + function.Strings[opCode.B].String;
                            return new LuaDecompiler.DecompiledOPCode(LuaDecompiler.opCodeType.empty, "");
                        }
                    }
                }
                else
                {
                    return new LuaDecompiler.DecompiledOPCode(LuaDecompiler.opCodeType.String, (String.Format("{0}.{1} = {2}",
                        function.Registers[opCode.A],
                        function.Strings[opCode.B].String,
                        function.Registers[opCode.C])));
                }

            }
            return new LuaDecompiler.DecompiledOPCode(LuaDecompiler.opCodeType.String, "idk");
        }

        public static void ConnectWithDoubleDot(LuaFile.LuaFunction function, LuaFile.LuaOPCode opCode)
        {
            string output = "(" + function.Registers[opCode.B];
            for (int i = opCode.B + 1; i <= opCode.C; i++)
            {
                output += " .. " + function.Registers[i];
            }
            output += ")";
            function.Registers[opCode.A] = output;
        }

        public static void ConnectWithColon(LuaFile.LuaFunction function, LuaFile.LuaOPCode opCode)
        {
            if (opCode.C > 255)
            {
                function.Registers[opCode.A] = function.Registers[opCode.B] + ":" + function.Strings[opCode.C - 256].String;
            }
            else
            {
                function.Registers[opCode.A] = function.Registers[opCode.B] + ":" + function.Registers[opCode.C];
            }
        }
    }
}
