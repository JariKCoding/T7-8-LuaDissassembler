using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaDecompiler
{
    class LuaTables
    {
        public static void GetIndex(LuaFile.LuaFunction function, LuaFile.LuaOPCode opCode)
        {
            if (opCode.C > 255)
            {
                function.Registers[opCode.A] = function.Registers[opCode.B] + "[" + function.Strings[opCode.C - 256].String + "]";
                function.DisassebleStrings.Add(String.Format("r({0}) = r({1})[c[{2}]] // {3}",
                    opCode.A,
                    opCode.B,
                    opCode.C - 256,
                    function.Registers[opCode.A]));
            }
            else
            {
                function.Registers[opCode.A] = function.Registers[opCode.B] + "[" + function.Registers[opCode.C] + "]";
                function.DisassebleStrings.Add(String.Format("r({0}) = r({1})[r({2})] // {3}",
                    opCode.A,
                    opCode.B,
                    opCode.C,
                    function.Registers[opCode.A]));
            }
        }

        public static void SetTable(LuaFile.LuaFunction function, LuaFile.LuaOPCode opCode)
        {
            if (opCode.C > 255)
            {
                function.DisassebleStrings.Add(String.Format("r({0})[c[{1}]] = c[{2}] // {3}[{4}] = {5}",
                    opCode.A,
                    opCode.B,
                    opCode.C - 256,
                    function.Registers[opCode.A],
                    function.Registers[opCode.B],
                    function.Strings[opCode.C - 256].getString()));
            }
            else
            {
                function.DisassebleStrings.Add(String.Format("r({0})[c[{1}]] = c[{2}] // {3}[{4}] = {5}",
                    opCode.A,
                    opCode.B,
                    opCode.C,
                    function.Registers[opCode.A],
                    function.Registers[opCode.B],
                    function.Registers[opCode.C]));
            }
        }

        public static void SetTableBackwards(LuaFile.LuaFunction function, LuaFile.LuaOPCode opCode)
        {
            function.DisassebleStrings.Add("; Unhandled OP: (OPCODE_SETTABLE_S_BK)");
        }

        public static void EmptyTable(LuaFile.LuaFunction function, LuaFile.LuaOPCode opCode)
        {
            function.Registers[opCode.A] = "table" + function.tableCount;
            function.tableCount++;
            function.DisassebleStrings.Add(String.Format("r({0}) = {4} // {3} = {4} Index: {2} Hash: {1}",
                opCode.A,
                opCode.C,
                opCode.B,
                function.Registers[opCode.A], "{}"));
        }

        public static void SetList(LuaFile.LuaFunction function, LuaFile.LuaOPCode opCode)
        {
            string tableString = "{";
            string tableRegisters = "";
            if (opCode.B > 0)
            {
                tableString += function.Registers[opCode.A + 1];
                tableRegisters += opCode.A + 1;
                if (opCode.B > 1)
                {
                    for (int j = opCode.A + 2; j <= opCode.A + opCode.B; j++)
                    {
                        tableString += ", " + function.Registers[j];
                        tableRegisters += ", " + j;
                    }
                }
            }
            tableString += "}";
            function.Registers[opCode.A] = function.Registers[opCode.A];

            function.DisassebleStrings.Add(String.Format("r({0}) = r({1}) // {2} = {3}",
                opCode.A,
                tableRegisters,
                function.Registers[opCode.A],
                tableString));
        }
    }
}
