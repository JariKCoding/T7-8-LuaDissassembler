using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaDecompiler.LuaDecompile
{
    class LuaTables
    {
        public static void GetIndex(LuaFile.LuaFunction function, LuaFile.LuaOPCode opCode)
        {
            if (opCode.C > 255)
            {
                function.Registers[opCode.A] = function.Registers[opCode.B] + "[" + function.Strings[opCode.C - 256].String + "]";
            }
            else
            {
                function.Registers[opCode.A] = function.Registers[opCode.B] + "[" + function.Registers[opCode.C] + "]";
            }
        }

        public static LuaDecompiler.DecompiledOPCode SetTable(LuaFile.LuaFunction function, LuaFile.LuaOPCode opCode)
        {
            if (opCode.C > 255)
            {
                return new LuaDecompiler.DecompiledOPCode(LuaDecompiler.opCodeType.String, String.Format("{0}[{1}] = {2}",
                    function.Registers[opCode.A],
                    function.Registers[opCode.B],
                    function.Strings[opCode.C - 256].getString()));
            }
            else
            {
                return new LuaDecompiler.DecompiledOPCode(LuaDecompiler.opCodeType.String, String.Format("{0}[{1}] = {2}",
                    function.Registers[opCode.A],
                    function.Registers[opCode.B],
                    function.Registers[opCode.C]));
            }
        }

        public static LuaDecompiler.DecompiledOPCode SetTableBackwards(LuaFile.LuaFunction function, LuaFile.LuaOPCode opCode)
        {
            if (opCode.C > 255)
            {
                return new LuaDecompiler.DecompiledOPCode(LuaDecompiler.opCodeType.String, String.Format("{0}[{1}] = {2}",
                    function.Registers[opCode.A],
                    function.Strings[opCode.B].getString(),
                    function.Strings[opCode.C - 256].getString()));
            }
            else
            {
                return new LuaDecompiler.DecompiledOPCode(LuaDecompiler.opCodeType.String, String.Format("{0}[{1}] = {2}",
                    function.Registers[opCode.A],
                    function.Strings[opCode.B].getString(),
                    function.Registers[opCode.C]));
            }
        }

        public static LuaDecompiler.DecompiledOPCode EmptyTable(LuaFile.LuaFunction function, LuaFile.LuaOPCode opCode)
        {
            while(function.UpvalsStrings.Contains("table" + function.tableCount))
            {
                function.tableCount++;
            }
            function.Registers[opCode.A] = "table" + function.tableCount++;
            return new LuaDecompiler.DecompiledOPCode(LuaDecompiler.opCodeType.String, String.Format("local {0} = {1}",
                    function.Registers[opCode.A], "{}"));
        }

        public static LuaDecompiler.DecompiledOPCode SetList(LuaFile.LuaFunction function, LuaFile.LuaOPCode opCode)
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
            return new LuaDecompiler.DecompiledOPCode(LuaDecompiler.opCodeType.String, String.Format("{0} = {1}",
                function.Registers[opCode.A],
                tableString));
        }
    }
}
