using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaDecompiler
{
    class LuaRegisters
    {
        /// <summary>
        /// Puts a register value in another register
        /// </summary>
        /// <param name="function"></param>
        /// <param name="opCode"></param>
        public static void RegisterToRegister(LuaFile.LuaFunction function, LuaFile.LuaOPCode opCode)
        {
            function.Registers[opCode.A] = function.Registers[opCode.B];
            function.DisassebleStrings.Add(String.Format("r({0}) = r({1}) // {2}",
                opCode.A,
                opCode.B,
                function.Registers[opCode.A]));
        }

        public static void GlobalRegisterToRegister(LuaFile.LuaFunction function, LuaFile.LuaOPCode opCode)
        {
            function.Registers[opCode.A] = function.Strings[opCode.Bx].String;
            function.DisassebleStrings.Add(String.Format("r({0}) = g[{1}] // {2}",
                opCode.A,
                opCode.Bx,
                function.Strings[opCode.Bx].getString()));
        }

        public static void RegisterToGlobal(LuaFile.LuaFunction function, LuaFile.LuaOPCode opCode)
        {
            function.DisassebleStrings.Add(String.Format("g[c[{0}]] = r({1}) // {2} = {3}",
                opCode.Bx,
                opCode.A,
                function.Strings[opCode.Bx].String,
                function.Registers[opCode.A]));
        }

        public static void BooleanToRegister(LuaFile.LuaFunction function, LuaFile.LuaOPCode opCode)
        {
            if (opCode.B == 0)
                function.Registers[opCode.A] = "false";
            else
                function.Registers[opCode.A] = "true";
            function.DisassebleStrings.Add(String.Format("r({0}) = {1}{2}",
                opCode.A,
                function.Registers[opCode.A],
                (opCode.C == 1) ? " // skip next opcode" : ""));
        }

        public static void NilToRegister(LuaFile.LuaFunction function, LuaFile.LuaOPCode opCode)
        {
            for (int i = opCode.A; i <= (opCode.B); i++)
            {
                function.Registers[i] = "nil";
            }
            if ((opCode.B) > opCode.A)
                function.DisassebleStrings.Add(String.Format("r({0} to {1}) inclusive = nil", opCode.A, opCode.B));
            else
                function.DisassebleStrings.Add(String.Format("r({0}) = nil", opCode.A));
        }

        public static void LocalConstantToRegister(LuaFile.LuaFunction function, LuaFile.LuaOPCode opCode)
        {
            function.Registers[opCode.A] = function.Strings[opCode.Bx].getString();
            function.DisassebleStrings.Add(String.Format("r({0}) = c[{1}] // {2}",
                opCode.A,
                opCode.Bx,
                function.Registers[opCode.A]));
        }
    }
}
