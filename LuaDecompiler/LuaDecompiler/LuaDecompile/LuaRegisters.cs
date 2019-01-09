using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaDecompiler.LuaDecompile
{
    class LuaRegisters
    {
        public static void RegisterToRegister(LuaFile.LuaFunction function, LuaFile.LuaOPCode opCode)
        {
            function.Registers[opCode.A] = function.Registers[opCode.B];
        }

        public static void GlobalRegisterToRegister(LuaFile.LuaFunction function, LuaFile.LuaOPCode opCode)
        {
            function.Registers[opCode.A] = function.Strings[opCode.Bx].String;
        }

        public static LuaDecompiler.DecompiledOPCode RegisterToGlobal(LuaFile.LuaFunction function, LuaFile.LuaOPCode opCode)
        {
            return new LuaDecompiler.DecompiledOPCode(LuaDecompiler.opCodeType.String, String.Format("{0} = {1}",
                function.Strings[opCode.Bx].String,
                function.Registers[opCode.A]));
        }

        public static void BooleanToRegister(LuaFile.LuaFunction function, LuaFile.LuaOPCode opCode, int index)
        {
            if(index > 2)
            {
                if(LuaConditions.isConditioOpCode(function.OPCodes[index - 2].OPCode))
                {
                    if(function.OPCodes[index].C == 1)
                    {
                        return;
                    }
                }
                else if (LuaConditions.isConditioOpCode(function.OPCodes[index - 3].OPCode))
                {
                    if (function.OPCodes[index - 1].C == 1)
                    {
                        return;
                    }
                }
            }
            if (opCode.B == 0)
                function.Registers[opCode.A] = "false";
            else
                function.Registers[opCode.A] = "true";
        }

        public static void NilToRegister(LuaFile.LuaFunction function, LuaFile.LuaOPCode opCode)
        {
            for (int i = opCode.A; i <= (opCode.B); i++)
            {
                function.Registers[i] = "nil";
            }
        }

        public static void LocalConstantToRegister(LuaFile.LuaFunction function, LuaFile.LuaOPCode opCode)
        {
            function.Registers[opCode.A] = function.Strings[opCode.Bx].getString();
        }
    }
}
