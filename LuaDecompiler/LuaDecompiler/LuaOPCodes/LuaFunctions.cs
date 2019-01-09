using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaDecompiler
{
    class LuaFunctions
    {
        public static void Closure(LuaFile.LuaFunction function, LuaFile.LuaOPCode opCode)
        {
            function.Registers[opCode.A] = String.Format("__FUNC_{0:X}_", function.subFunctions[opCode.Bx].beginPosition);
            function.doingUpvals = opCode.A;
            function.lastFunctionClosure = opCode.Bx;
            function.DisassembleStrings.Add(String.Format("r({0}) = closure({1}) // {2}",
                opCode.A,
                opCode.Bx,
                function.Registers[opCode.A]));
        }

        public static void GetUpValue(LuaFile.LuaFunction function, LuaFile.LuaOPCode opCode)
        {
            function.Registers[opCode.A] = function.UpvalsStrings[opCode.B];
            function.DisassembleStrings.Add(String.Format("r({0}) = upval({1}) // {2}",
                opCode.A,
                opCode.B,
                function.UpvalsStrings[opCode.B]));
        }

        public static void CallFunctionWithParameters(LuaFile.LuaFunction function, LuaFile.LuaOPCode opCode, int index, bool tailCall = false)
        {
            string funcName = function.Registers[opCode.A];
            int parameterCount = opCode.B - 1;
            int returnValues = opCode.C - 1;
            if (returnValues < 0)
            {
                returnValues = 0;
                tailCall = true;
            }
                
            string parameterRegisters = "";
            string parametersString = "";
            if (parameterCount > 0)
            {
                parameterRegisters += opCode.A + 1;
                for (int j = opCode.A + 2; j <= opCode.A + parameterCount; j++)
                {
                    parameterRegisters += ", " + j;
                }
                if (funcName.Contains(":") && opCode.A + 2 <= opCode.A + parameterCount)
                {
                    parametersString += function.Registers[opCode.A + 2];
                    for (int j = opCode.A + 3; j <= opCode.A + parameterCount; j++)
                    {
                        parametersString += ", " + function.Registers[j];
                    }
                }
                else
                {
                    parametersString += function.Registers[opCode.A + 1];
                    for (int j = opCode.A + 2; j <= opCode.A + parameterCount; j++)
                    {
                        parametersString += ", " + function.Registers[j];
                    }
                }
            }
            else if(parameterCount != 0)
            {
                parameterRegisters += opCode.A + 1;
                for (int j = opCode.A + 2; j <= function.OPCodes[index - 1].A; j++)
                {
                    parameterRegisters += ", " + j;
                }
                
                int startpoint = 2;
                parametersString = function.Registers[opCode.A + 1];
                if (funcName.Contains(":"))
                {
                    parametersString = function.Registers[opCode.A + 2];
                    startpoint = 3;
                }
                for (int j = opCode.A + startpoint; j <= function.OPCodes[index - 1].A; j++)
                {
                    parametersString += ", " + function.Registers[j];
                }
            }
            if (returnValues > 0)
            {
                string returnRegisters = opCode.A.ToString();
                string returnStrings = "returnval" + function.returnValCount;
                function.Registers[opCode.A] = function.getNewReturnVal();
                if (returnValues > 1)
                {
                    for (int j = opCode.A + 1; j < opCode.A + returnValues; j++)
                    {
                        returnRegisters += ", " + j.ToString();
                        returnStrings += ", returnval" + function.returnValCount;
                        function.Registers[j] = function.getNewReturnVal();
                    }
                }
                function.DisassembleStrings.Add(String.Format("r({0}) = call r({1}) with r({2}) // {3} = {4}({5})",
                    returnRegisters,
                    opCode.A,
                    parameterRegisters,
                    returnStrings,
                    funcName,
                    parametersString));
                function.DecompileStrings.Add(String.Format("local {0} = {1}({2})",
                    returnStrings,
                    funcName,
                    parametersString));
            }
            else
            {
                function.Registers[opCode.A] = funcName + "(" + parametersString + ")";
                function.DisassembleStrings.Add(String.Format("{5}call r({0}) with r({1}) // {3}({4})",
                    opCode.A,
                    parameterRegisters,
                    function.Registers[opCode.A],
                    funcName,
                    parametersString,
                    (tailCall) ? "return " : ""));
                function.DecompileStrings.Add(String.Format("{0}{1}({2})",
                    (tailCall) ? "return " : "",
                    funcName,
                    parametersString));
            }
        }

        public static void CallFunctionWithoutParameters(LuaFile.LuaFunction function, LuaFile.LuaOPCode opCode)
        {
            if (opCode.C > 1)
            {
                string returnRegisters = opCode.A.ToString();
                string returnStrings = function.getNewReturnVal();
                if (opCode.C > 2)
                {
                    for (int j = opCode.A + 1; j < opCode.A + opCode.C - 1; j++)
                    {
                        returnRegisters += ", " + j.ToString();
                        returnStrings += ", returnval" + function.returnValCount;
                        function.Registers[j] = function.getNewReturnVal();
                    }
                }
                function.DisassembleStrings.Add(String.Format("r({0}) = call r({1}) // {2} = {3}()",
                    returnRegisters,
                    opCode.A,
                    returnStrings,
                    function.Registers[opCode.A]));
                function.DecompileStrings.Add(String.Format("local {0} = {1}()",
                    returnStrings,
                    function.Registers[opCode.A]));
                for (int j = opCode.A; j < opCode.A + opCode.C - 1; j++)
                    function.returnValCount--;
                function.Registers[opCode.A] = function.getNewReturnVal();
                if (opCode.C > 2)
                {
                    for (int j = opCode.A + 1; j < opCode.A + opCode.C - 1; j++)
                    {
                        function.Registers[j] = function.getNewReturnVal();
                    }
                }
            }
            else
            {
                function.DisassembleStrings.Add(String.Format("call r({0}) // {1}()",
                    opCode.A,
                    function.Registers[opCode.A]));
                function.DecompileStrings.Add(String.Format("{0}()",
                    function.Registers[opCode.A]));
            }
        }
    }
}
