using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaDecompiler
{
    class LuaStrings
    {
        /// <summary>
        /// Connects a register value and a constant value with a .
        /// </summary>
        /// <param name="function"></param>
        /// <param name="opCode"></param>
        public static void ConnectWithDot(LuaFile.LuaFunction function, LuaFile.LuaOPCode opCode)
        {
            function.Registers[opCode.A] = function.Registers[opCode.B] + "." + function.Strings[opCode.C].String;
            function.DisassembleStrings.Add(String.Format("r({0}) = r({1}).field({2}) // {3}",
                opCode.A,
                opCode.B,
                opCode.C,
                function.Registers[opCode.A]));
        }

        public static void SetField(LuaFile.LuaFunction function, LuaFile.LuaOPCode opCode, int index, bool isString = false)
        {
            if(opCode.C > 255)
            {
                if (function.Strings[opCode.C - 256].StringType == LuaFile.StringType.String)
                    isString = true;
                function.DisassembleStrings.Add(String.Format("r({0}).field(c[{1}]) = c[{2}] // {3}.{4} = {5}",
                    opCode.A,
                    opCode.B,
                    opCode.C - 256,
                    function.Registers[opCode.A],
                    function.Strings[opCode.B].String,
                    function.Strings[opCode.C - 256].getString()));
                function.DecompileStrings.Add(String.Format("{0}.{1} = {2}",
                    function.Registers[opCode.A],
                    function.Strings[opCode.B].String,
                    function.Strings[opCode.C - 256].getString()));
            }
            else
            {
                if (function.Registers[opCode.C].Contains("__FUNC") 
                    && (function.OPCodes[index - 1].OPCode == 0x4A || function.OPCodes[index - 1].OPCode == 0x54)
                    && (function.Registers[opCode.A].Substring(0, 4) == "CoD." || function.Registers[opCode.A].Substring(0, 4) == "LUI."))
                {
                    for(int i = index; i > 0; i--)
                    {
                        if(function.OPCodes[i].OPCode == 0x4A)
                        {
                            function.subFunctions[function.OPCodes[i].Bx].functionName = function.Registers[opCode.A] + "." + function.Strings[opCode.B].String;
                            break;
                        }
                    }
                }
                else
                {
                    function.DecompileStrings.Add(String.Format("{0}.{1} = {2}",
                        function.Registers[opCode.A],
                        function.Strings[opCode.B].String,
                        (isString) ? "\"" + function.Registers[opCode.C] + "\"" : function.Registers[opCode.C]));
                }
                function.DisassembleStrings.Add(String.Format("r({0}).field(c[{1}]) = r({2}) // {3}.{4} = {5}",
                    opCode.A,
                    opCode.B,
                    opCode.C,
                    function.Registers[opCode.A],
                    function.Strings[opCode.B].String,
                    (isString) ? "\"" + function.Registers[opCode.C] + "\"" : function.Registers[opCode.C]));
                
            }
        }

        /// <summary>
        /// Connects 2 register values with a double .
        /// </summary>
        /// <param name="function"></param>
        /// <param name="opCode"></param>
        public static void ConnectWithDoubleDot(LuaFile.LuaFunction function, LuaFile.LuaOPCode opCode)
        {
            string output = "(" + function.Registers[opCode.B];
            string registers = "r(" + opCode.B + ")";
            for (int i = opCode.B + 1; i <= opCode.C; i++)
            {
                output += " .. " + function.Registers[i];
                registers += "..r(" + i + ")";
            }
            output += ")";
            function.Registers[opCode.A] = output;
            function.DisassembleStrings.Add(String.Format("r({0}) = {1} // {2}",
                opCode.A,
                registers,
                function.Registers[opCode.A]));
        }

        /// <summary>
        /// Connects a register value and a constant value with a :
        /// </summary>
        /// <param name="function"></param>
        /// <param name="opCode"></param>
        public static void ConnectWithColon(LuaFile.LuaFunction function, LuaFile.LuaOPCode opCode)
        {
            if (opCode.C > 255)
            {
                function.Registers[opCode.A] = function.Registers[opCode.B] + ":" + function.Strings[opCode.C - 256].String;
                function.DisassembleStrings.Add(String.Format("r({0}) = r({1}):c[{2}] // {3}",
                    opCode.A,
                    opCode.B,
                    opCode.C - 256,
                    function.Registers[opCode.A]));
            }
            else
            {
                
                function.Registers[opCode.A] = function.Registers[opCode.B] + ":" + function.Registers[opCode.C];
                function.DisassembleStrings.Add(String.Format("r({0}) = r({1}):r({2}) // {3}",
                    opCode.A,
                    opCode.B,
                    opCode.C,
                    function.Registers[opCode.A]));
            }
        }
    }
}
