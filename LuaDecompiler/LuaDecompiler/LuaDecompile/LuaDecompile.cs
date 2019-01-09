using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;

namespace LuaDecompiler
{
    class LuaDecompiler
    {
        private StreamWriter outputWriter { get; set; }
        private LuaFile luaFile { get; set; }

        public LuaDecompiler(LuaFile luaFile)
        {
            this.luaFile = luaFile;
        }

        public enum opCodeType { String, skipLines, FunctionClosure, empty, orCondition, forEach, Else, conditionToReg }

        public class DecompiledOPCode
        {
            public opCodeType opcodeType { get; set; }
            public string OpString { get; set; }
            public int skipLines { get; set; }
            public int elseLines { get; set; }
            public DecompiledOPCode(opCodeType type, string str)
            {
                this.opcodeType = type;
                this.OpString = str;
            }
            public DecompiledOPCode(opCodeType type, string str, int skipLines)
            {
                this.opcodeType = type;
                this.OpString = str;
                this.skipLines = skipLines;
            }
            public DecompiledOPCode(opCodeType type, string str, int skipLines, int elseLines)
            {
                this.opcodeType = type;
                this.OpString = str;
                this.skipLines = skipLines;
                this.elseLines = elseLines;
            }
        }

        public void Decompile(string outputFile)
        {
            // Check if we found a better name (Bo4 hashes)
            if (luaFile.fakeName != "")
                outputFile = Path.GetDirectoryName(outputFile) + "\\" + luaFile.fakeName + ".lua";
            // Make a new StreamWriter
            this.outputWriter = new StreamWriter(outputFile + "dc");
            outputWriter.WriteLine("-- Decompiled by LuaDecompiler by JariK\n");
            
            foreach(LuaFile.LuaFunction function in luaFile.Functions)
            {
                resetValues(function);
            }
            // Start the decompile with the init function
            functionDecompile(luaFile.Functions[0], 0, true);
            // Close the StreamWriter so it saves
            outputWriter.Close();
        }

        private void resetValues(LuaFile.LuaFunction function)
        {
            function.returnValCount = 0;
            function.tableCount = 0;
            function.forLoopCount = 0;
        }

        private void functionDecompile(LuaFile.LuaFunction function, int functionLevel, bool skipHeader = false)
        {
            // Check if we need to write the name of the function ( not with init )
            if (!skipHeader)
            {
                writeWithTabs(functionLevel - 1, "");
                // Check if we have a local function or global
                string funcName = function.getName(true).Substring(0, 4);
                if (funcName != "LUI." && funcName.Substring(0, 4) != "CoD.")
                    outputWriter.Write("local ");
                // Write the function name
                outputWriter.Write("function " + function.getName(true) + "(");
                // Add all the arguments
                int index = 0, jj = 0;
                while(index < function.parameterCount)
                {
                    if(function.UpvalsStrings.Contains("arg" + jj))
                    {
                        jj++;
                        continue;
                    }
                    function.Registers[index] = "arg" + jj;
                    outputWriter.Write(((index > 0) ? ", " : "") + "arg" + jj++);
                    index++;
                }
                outputWriter.Write(")\n");
            }

            List<int> ifLevels = new List<int>();
            List<int> elseLevels = new List<int>();
            for (int i = 0; i < function.OPCodes.Count; i++)
            {   
                DecompiledOPCode opcode = decompileOPCode(function, i, functionLevel);
                int skipLineAfter = 0;
                if (opcode.opcodeType == opCodeType.String)
                {
                    writeWithTabs(functionLevel + ifLevels.Count, opcode.OpString + "\n");
                }
                else if(opcode.opcodeType == opCodeType.FunctionClosure && function.getName() != "__INIT__" && function.lastFunctionClosure > -1)
                {
                    functionDecompile(function.subFunctions[function.lastFunctionClosure], functionLevel + 1);
                }
                else if(opcode.opcodeType == opCodeType.skipLines || opcode.opcodeType == opCodeType.forEach)
                {
                    writeWithTabs(functionLevel + ifLevels.Count, opcode.OpString + "\n");
                    ifLevels.Add(opcode.skipLines + 2);
                }
                else if(opcode.opcodeType == opCodeType.Else)
                {
                    if(ifLevels.Count > 0)
                        ifLevels.RemoveAt(ifLevels.Count - 1);
                    ifLevels.Add(opcode.skipLines + 1 + opcode.elseLines);
                    elseLevels.Add(opcode.skipLines);
                }
                else if(opcode.opcodeType == opCodeType.conditionToReg)
                {
                    writeWithTabs(functionLevel + ifLevels.Count, opcode.OpString + "\n");
                    skipLineAfter += 3;
                }

                for(int k = 0; k < skipLineAfter + 1; k++)
                {
                    if (k > 0)
                        i++;
                    for (int j = 0; j < ifLevels.Count; j++)
                    {
                        ifLevels[j]--;
                        if (ifLevels[j] <= 0)
                        {
                            ifLevels.RemoveAt(j);
                            writeWithTabs(functionLevel + ifLevels.Count, "end\n");
                        }
                    }
                    for (int j = 0; j < elseLevels.Count; j++)
                    {
                        elseLevels[j]--;
                        if (elseLevels[j] <= 0)
                        {
                            elseLevels.RemoveAt(j);
                            writeWithTabs(functionLevel + ifLevels.Count - 1, "else\n");
                        }
                    }
                }
            }

            // Add the end of the function (not init)
            if (!skipHeader)
            {
                writeWithTabs(functionLevel - 1, "end\n");
            }
            outputWriter.Write("\n");

            // Print the other functions that are part of init
            if (function.getName() == "__INIT__")
            {
                for (int i = 0; i < function.subFunctions.Count; i++)
                {
                    functionDecompile(function.subFunctions[i], functionLevel + 1);
                }
            }
        }

        public void doFunctionClosure(LuaFile.LuaFunction function, int functionLevel)
        {
            functionDecompile(function.subFunctions[function.lastFunctionClosure], functionLevel + 1);
        }

        private DecompiledOPCode decompileOPCode(LuaFile.LuaFunction function, int index, int functionLevel)
        {
            DecompiledOPCode DecompileString = new DecompiledOPCode(opCodeType.empty, "");
            LuaFile.LuaOPCode opCode = function.OPCodes[index];
            try
            {
                switch (opCode.OPCode)
                {
                    case 0x0: LuaDecompile.LuaStrings.ConnectWithDot(function, opCode); break;
                    case 0x1: DecompileString = LuaDecompile.LuaConditions.IfIsTrueFalse(function, opCode, index); break;
                    case 0x2: case 0x4C: DecompileString = LuaDecompile.LuaFunctions.CallFunctionWithParameters(function, opCode, index); break;
                    case 0x16: DecompileString = LuaDecompile.LuaFunctions.CallFunctionWithParameters(function, opCode, index, true); break;
                    case 0x4: DecompileString = LuaDecompile.LuaConditions.IfIsEqual(function, opCode, index); break;
                    case 0x5: DecompileString = LuaDecompile.LuaConditions.IfIsEqualBackwards(function, opCode, index); break;
                    case 0x6: LuaDecompile.LuaRegisters.GlobalRegisterToRegister(function, opCode); break;
                    case 0x7: LuaDecompile.LuaRegisters.RegisterToRegister(function, opCode); break;
                    case 0x8: LuaDecompile.LuaStrings.ConnectWithColon(function, opCode); break;
                    case 0x9: DecompileString = LuaDecompile.LuaOperators.Return(function, opCode, index); break;
                    case 0xA: LuaDecompile.LuaTables.GetIndex(function, opCode); break;
                    case 0xD: LuaDecompile.LuaRegisters.BooleanToRegister(function, opCode, index); break;
                    //case 0xE: Console.WriteLine("Foreach"); break;
                    case 0xF: DecompileString = LuaDecompile.LuaStrings.SetField(function, opCode, index); break;
                    case 0x10: DecompileString = LuaDecompile.LuaTables.SetTable(function, opCode); break;
                    case 0x11: DecompileString = LuaDecompile.LuaTables.SetTableBackwards(function, opCode); break;
                    case 0x19: LuaDecompile.LuaRegisters.LocalConstantToRegister(function, opCode); break;
                    case 0x1A: LuaDecompile.LuaRegisters.NilToRegister(function, opCode); break;
                    case 0x1B: DecompileString = LuaDecompile.LuaRegisters.RegisterToGlobal(function, opCode); break;
                    case 0x1C: DecompileString = LuaDecompile.LuaConditions.SkipLines(function, opCode, index); break;
                    case 0x26: LuaDecompile.LuaFunctions.GetUpValue(function, opCode); break;
                    case 0x27: DecompileString = LuaDecompile.LuaOperators.SetupVal(function, opCode); break;
                    case 0x28: DecompileString = LuaDecompile.LuaOperators.Add(function, opCode); break;
                    case 0x29: DecompileString = LuaDecompile.LuaOperators.AddBackWards(function, opCode); break;
                    case 0x2A: DecompileString = LuaDecompile.LuaOperators.Subtract(function, opCode); break;
                    case 0x2B: DecompileString = LuaDecompile.LuaOperators.SubtractBackWards(function, opCode); break;
                    case 0x2C: DecompileString = LuaDecompile.LuaOperators.Multiply(function, opCode); break;
                    case 0x2D: DecompileString = LuaDecompile.LuaOperators.MultiplyBackWards(function, opCode); break;
                    case 0x2E: DecompileString = LuaDecompile.LuaOperators.Divide(function, opCode); break;
                    case 0x2F: DecompileString = LuaDecompile.LuaOperators.DivideBackWards(function, opCode); break;
                    case 0x30: DecompileString = LuaDecompile.LuaOperators.Modulo(function, opCode); break;
                    case 0x31: DecompileString = LuaDecompile.LuaOperators.ModuloBackWards(function, opCode); break;
                    case 0x32: DecompileString = LuaDecompile.LuaOperators.Power(function, opCode); break;
                    case 0x33: DecompileString = LuaDecompile.LuaOperators.PowerBackWards(function, opCode); break;
                    case 0x34: DecompileString = LuaDecompile.LuaTables.EmptyTable(function, opCode); break;
                    case 0x35: LuaDecompile.LuaOperators.UnaryMinus(function, opCode); break;
                    case 0x36: DecompileString = LuaDecompile.LuaConditions.Not(function, opCode); break;
                    case 0x37: LuaDecompile.LuaOperators.Length(function, opCode); break;
                    case 0x38: DecompileString = LuaDecompile.LuaConditions.LessThan(function, opCode, index); break;
                    case 0x39: DecompileString = LuaDecompile.LuaConditions.LessThanBackwards(function, opCode, index); break;
                    case 0x3A: DecompileString = LuaDecompile.LuaConditions.LessOrEqualThan(function, opCode, index); break;
                    case 0x3B: DecompileString = LuaDecompile.LuaConditions.LessOrEqualThanBackwards(function, opCode, index); break;
                    case 0x3C: DecompileString = LuaDecompile.LuaOperators.ShiftLeft(function, opCode); break;
                    case 0x3D: DecompileString = LuaDecompile.LuaOperators.ShiftLeftBackwards(function, opCode); break;
                    case 0x40: DecompileString = LuaDecompile.LuaOperators.BinaryAnd(function, opCode); break;
                    case 0x42: DecompileString = LuaDecompile.LuaOperators.BinaryOr(function, opCode); break;
                    case 0x44: LuaDecompile.LuaStrings.ConnectWithDoubleDot(function, opCode); break;
                    case 0x45: DecompileString = LuaDecompile.LuaOperators.TestSet(function, opCode); break;
                    case 0x46: DecompileString = LuaDecompile.LuaLoops.StartForLoop(function, opCode); break;
                    case 0x48: DecompileString = LuaDecompile.LuaTables.SetList(function, opCode); break;
                    case 0x49: DecompileString = LuaDecompile.LuaOperators.Close(function, opCode); break;
                    case 0x4A: LuaDecompile.LuaFunctions.Closure(function, opCode, index, functionLevel, this); break;
                    case 0x4B: DecompileString = LuaDecompile.LuaOperators.VarArg(function, opCode); break;
                    case 0x4D: DecompileString = LuaDecompile.LuaFunctions.CallFunctionWithoutParameters(function, opCode, index); break;
                    case 0x4F: DecompileString = LuaDecompile.LuaConditions.IfIsTrueFalse(function, opCode, index); break;
                    case 0x50: DecompileString = LuaDecompile.LuaOperators.NotR1(function, opCode); break;
                    case 0x51: LuaDecompile.LuaStrings.ConnectWithDot(function, opCode); break;
                    case 0x52: DecompileString = LuaDecompile.LuaStrings.SetField(function, opCode, index); break;
                    case 0x54:
                        if (function.doingUpvals >= 0)
                        {
                            if (opCode.A == 1)
                            {
                                function.DisassembleStrings.Add(String.Format("r({0}).upval({1}) = r({2}) // {3}",
                                    function.doingUpvals,
                                    function.subFunctions[function.lastFunctionClosure].UpvalsStrings.Count,
                                    opCode.C,
                                    function.Registers[opCode.C]));
                            }
                            else if (opCode.A == 2)
                            {
                                function.DisassembleStrings.Add(String.Format("r({0}).upval({1}) = upval({2}) // {3}",
                                    function.doingUpvals,
                                    function.subFunctions[function.lastFunctionClosure].UpvalsStrings.Count,
                                    opCode.C,
                                    function.UpvalsStrings[opCode.C]));
                            }
                            else
                            {
                                Console.WriteLine("Trying to get to do upvalue on level " + opCode.A);
                            }
                        }
                        else
                        {
                            function.DisassembleStrings.Add(String.Format("data({0}, {1}, {2}, {3})",
                                opCode.A,
                                opCode.C,
                                opCode.B,
                                opCode.OPCode));
                        }
                        break;
                }
                if (opCode.OPCode == 0xF && function.getName() == "__INIT__")
                {
                    if (luaFile.game == LuaFile.Game.BlackOps4 && function.Registers[opCode.A].Length > 3)
                    {
                        if ((function.Registers[opCode.A].Substring(0, 4) == "CoD." || function.Registers[opCode.A].Substring(0, 4) == "LUI.") &&
                            function.Strings[opCode.B].String == "new")
                        {
                            luaFile.fakeName = function.Registers[opCode.A].Substring(4);
                        }
                    }
                }
                if (function.doingUpvals >= 0)
                {
                    if (opCode.OPCode != 0x4A)
                    {
                        if (opCode.OPCode == 0x54)
                        {
                            if (opCode.A == 1)
                            {
                                function.subFunctions[function.lastFunctionClosure].UpvalsStrings.Add(function.Registers[opCode.C]);
                            }
                            else if (opCode.A == 2)
                            {
                                function.subFunctions[function.lastFunctionClosure].UpvalsStrings.Add(function.UpvalsStrings[opCode.C]);
                            }
                            if(function.OPCodes[index + 1].OPCode != 0x54 && function.getName() != "__INIT__")
                                doFunctionClosure(function, functionLevel);
                        }
                        else 
                        {   
                            function.doingUpvals = -1;
                        }
                    }
                }
            }
            catch
            {
                Console.WriteLine("Error occured while disassembling A: {3}, B: {4}, C: {5} OPcode: {0:X} at line {1} in function {2}", opCode.OPCode, index, function.getName(), opCode.A, opCode.B, opCode.C);
            }
            return DecompileString;
        }

        private void writeWithTabs(int index, string text)
        {
            string str = "";
            for (int i = 0; i < index; i++)
                str += "\t";
            outputWriter.Write(str + text);
        }
    }
}
