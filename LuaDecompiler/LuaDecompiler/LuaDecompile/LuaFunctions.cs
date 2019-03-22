using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaDecompiler.LuaDecompile
{
    class LuaFunctions
    {
        public static void Closure(LuaFile.LuaFunction function, LuaFile.LuaOPCode opCode, int index, int functionLevel, LuaDecompiler luaDecomp)
        {
            function.Registers[opCode.A] = String.Format("__FUNC_{0:X}_", function.subFunctions[opCode.Bx].beginPosition);
            function.doingUpvals = opCode.A;
            function.lastFunctionClosure = opCode.Bx;
            if (function.OPCodes[index + 1].OPCode != 0x54 && function.getName() != "__INIT__")
                luaDecomp.doFunctionClosure(function, functionLevel);
        }

        public static void GetUpValue(LuaFile.LuaFunction function, LuaFile.LuaOPCode opCode)
        {
            function.Registers[opCode.A] = function.UpvalsStrings[opCode.B];
        }

        public static LuaDecompiler.DecompiledOPCode CallFunctionWithParameters(LuaFile.LuaFunction function, LuaFile.LuaOPCode opCode, int index, bool tailCall = false)
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
            else if (parameterCount != 0)
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
                function.Registers[opCode.A] = funcName + "(" + parametersString + ")";
                string returnRegisters = opCode.A.ToString();
                string returnVal = function.getNewReturnVal();
                string returnStrings = returnVal;
                function.Registers[opCode.A] = returnVal;
                if (returnValues > 1)
                {
                    for (int j = opCode.A + 1; j < opCode.A + returnValues; j++)
                    {
                        returnRegisters += ", " + j.ToString();
                        returnVal = function.getNewReturnVal();
                        returnStrings += ", " + returnVal;
                        function.Registers[j] = returnVal;
                    }
                }
                return new LuaDecompiler.DecompiledOPCode(LuaDecompiler.opCodeType.String, String.Format("local {0} = {1}({2})",
                    returnStrings,
                    funcName,
                    parametersString));
            }
            else
            {
                function.Registers[opCode.A] = funcName + "(" + parametersString + ")";
                if(function.OPCodes[index + 1].OPCode == 0x2 || function.OPCodes[index + 1].OPCode == 0x4C || function.OPCodes[index + 1].OPCode == 0x16)
                {
                    if(function.OPCodes[index + 1].B == 0)
                    {

                        return new LuaDecompiler.DecompiledOPCode(LuaDecompiler.opCodeType.empty, "");
                    }
                }
                return new LuaDecompiler.DecompiledOPCode(LuaDecompiler.opCodeType.String, String.Format("{0}{1}({2})",
                    (tailCall) ? "return " : "",
                    funcName,
                    parametersString));
            }
        }

        public static LuaDecompiler.DecompiledOPCode CallFunctionWithoutParameters(LuaFile.LuaFunction function, LuaFile.LuaOPCode opCode, int index)
        {
            if (opCode.C > 1)
            {
                string returnRegisters = opCode.A.ToString();
                int oldStartCount = function.returnValCount;
                string returnStrings = function.getNewReturnVal();
                if (opCode.C > 2)
                {
                    for (int j = opCode.A + 1; j < opCode.A + opCode.C - 1; j++)
                    {
                        returnRegisters += ", " + j.ToString();
                        string returnVal = function.getNewReturnVal();
                        returnStrings += ", " + returnVal;
                        //function.Registers[j] = returnVal;
                    }
                }
                LuaDecompiler.DecompiledOPCode str = new LuaDecompiler.DecompiledOPCode(LuaDecompiler.opCodeType.String, String.Format("local {0} = {1}()",
                    returnStrings,
                    function.Registers[opCode.A]));
                function.returnValCount = oldStartCount;
                function.Registers[opCode.A] = function.getNewReturnVal();
                if (opCode.C > 2)
                {
                    for (int j = opCode.A + 1; j < opCode.A + opCode.C - 1; j++)
                    {
                        function.Registers[j] = function.getNewReturnVal();
                    }
                }
                return str;
            }
            else
            {
                if (function.OPCodes[index + 1].OPCode == 0x2 || function.OPCodes[index + 1].OPCode == 0x4C || function.OPCodes[index + 1].OPCode == 0x16)
                {
                    if (function.OPCodes[index + 1].B == 0)
                    {
                        function.Registers[opCode.A] = String.Format("{0}()", function.Registers[opCode.A]);
                        return new LuaDecompiler.DecompiledOPCode(LuaDecompiler.opCodeType.empty, "");
                    }
                }
                return new LuaDecompiler.DecompiledOPCode(LuaDecompiler.opCodeType.String, String.Format("{0}()",
                    function.Registers[opCode.A]));
            }
        }
    }
}
