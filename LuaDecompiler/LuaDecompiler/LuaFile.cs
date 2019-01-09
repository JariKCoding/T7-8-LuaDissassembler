using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace LuaDecompiler
{
    public class LuaFile
    {
        private BinaryReader inputReader { get; }
        private StreamWriter outputWriter { get; set; }
        public string fakeName { get; set; }
        public static int errors { get; set; }

        // This makes the program not crash when it gets bo2 files but op codes are different
        public bool exportBlackOps2 = false;

        public Game game { get; set; }

        public enum Game { BlackOps4, BlackOps3 };

        public enum StringType { nullptr, hash, boolean, String, number}

        public class LuaFunction
        {
            public int upvalsCount { get; set; }
            public int registerCount { get; set; }
            public int opcodesCount { get; set; }
            public int parameterCount { get; set; }
            public long beginPosition { get; set; }
            public long endPosition { get; set; }
            public int subFunctionsCount { get; set; }
            public int tableCount { get; set; }
            public int returnValCount { get; set; }
            public int forLoopCount { get; set; }
            public int doingUpvals { get; set; }
            public int lastFunctionClosure { get; set; }
            public LuaFunction superFunction { get; set; }
            public List<LuaFunction> subFunctions { get; set; }
            public List<LuaOPCode> OPCodes { get; set; }
            public List<LuaString> Strings { get; set; }
            public List<String> DisassembleStrings { get; set; }
            public List<String> DecompileStrings { get; set; }
            public List<String> UpvalsStrings { get; set; }
            public List<int> foreachPositions { get; set; }
            public string[] Registers { get; set; }
            public string functionName { get; set; }

            public LuaFunction(int registerCount)
            {
                this.registerCount = registerCount;
                OPCodes = new List<LuaOPCode>();
                Strings = new List<LuaString>();
                subFunctions = new List<LuaFunction>();
                DisassembleStrings = new List<String>();
                DecompileStrings = new List<String>();
                UpvalsStrings = new List<String>();
                foreachPositions = new List<int>();
                Registers = new string[registerCount];
                for (int i = 0; i < registerCount; i++)
                    Registers[i] = "";
                functionName = "";
                tableCount = 0;
                returnValCount = 0;
                forLoopCount = 0;
                forLoopCount = -1;
                lastFunctionClosure = -1;
            }

            public string getNewReturnVal()
            {
                while(this.UpvalsStrings.Contains("returnval" + this.returnValCount))
                {
                    this.returnValCount++;
                }
                return "returnval" + this.returnValCount++;
            }

            public string getName(bool getFakeName = false)
            {
                if (beginPosition <= 0x101)
                    return "__INIT__";
                else if (getFakeName && functionName != "")
                    return functionName;
                else
                    return string.Format("__FUNC_{0:X}_", this.beginPosition);
            }

            public string toString()
            {
                string output = String.Format("; Opcodes: 0x{0:X}\n; Constants: 0x{1:X}\n; Registers: 0x{2:X}\n; Upvals: 0x{3:X}", 
                    this.opcodesCount, this.Strings.Count, this.registerCount, this.upvalsCount);
                if(this.superFunction != null)
                    output += String.Format("\n; SuperFunction: {0}", this.superFunction.getName());
                output += String.Format("\nfunction {0}(", getName());
                if(this.parameterCount > 0)
                {
                    output += "arg0";
                    for(int i = 1; i < parameterCount; i++)
                        output += ", arg" + i;
                }
                output += ")";
                for (int i = 0; i < this.Strings.Count; i++)
                {
                    output += String.Format("\n\t.const\t[{0}]\t{1}: {2}", i,
                        (this.Strings[i].StringType == StringType.String) ? "\"" + this.Strings[i].String + "\"" : this.Strings[i].String, 
                        this.Strings[i].StringType);
                }
                foreach(String DisassembleString in this.DisassembleStrings)
                {
                    output += "\n\t" + DisassembleString;
                }
                output += "\nend\n";
                return output;
            }
        }

        // TODO: change the switch with opcodes to a dictionary
        /*public static Dictionary<int, Func<LuaFunction, LuaOPCode>> opCodeIndices = new Dictionary<int, Func<LuaFunction, LuaOPCode>>()
        {
            { 0x0,      new Func<LuaFunction, LuaOPCode>(LuaStrings.ConnectWithDot) }
        };*/

        public class LuaOPCode
        {
            // All instructions have an opcode in the last 6 bits.
            //Instructions can have the following fields:
            //    `A' : 8 bits
            //    `B' : 9 bits
            //    `C' : 9 bits
            //    `Bx' : 18 bits (`B' and `C' together)
            //    `sBx' : signed Bx
            public byte A { get; set; }
            public int B { get; set; }
            public int C { get; set; }
            public int Bx { get; set; }
            public int sBx { get; set; }
            public int OPCode { get; set; }
        }

        public class LuaString
        {
            public StringType StringType { get; set; }
            public string String { get; set; }

            public string getString()
            {
                if (StringType == StringType.String && String != "nil")
                    return "\"" + String + "\"";
                else
                    return String;
            }
        }

        public List<LuaFunction> Functions { get; set;}

        public LuaFile(String fileName)
        {
            this.inputReader = new BinaryReader(new FileStream(fileName, FileMode.Open));
            Functions = new List<LuaFunction>();
            searchGameVersion();
            fakeName = "";
        }

        public void Disassemble()
        {
            LuaFunction funcion = this.loadFunction();
            for(int i = 0; i < this.Functions[0].subFunctionsCount; i++)
            {
                this.handleSubFunctions(this.Functions[0]);
            }

            for (int i = 0; i < this.Functions.Count; i++)
                this.disassembleOPCodes(this.Functions[i], i);
        }

        private void handleSubFunctions(LuaFunction function)
        {
            LuaFunction subFunction = loadFunction(this.inputReader.BaseStream.Position);
            subFunction.superFunction = function;
            
            function.subFunctions.Add(subFunction);
            for (int i = 0; i < subFunction.subFunctionsCount; i++)
            {
                this.handleSubFunctions(subFunction);
            }
        }

        public void WriteDisassemble(String outputFile)
        {
            if (fakeName != "")
                outputFile = Path.GetDirectoryName(outputFile) + "\\" + fakeName + ".lua";
            this.outputWriter = new StreamWriter(outputFile + "dec");
            outputWriter.WriteLine("; Disassembled by LuaDecompiler by JariK\n");
            for(int i=0; i < this.Functions.Count; i++)
            {
                outputWriter.WriteLine(this.Functions[i].toString());
            }
            outputWriter.Close();
        }

        public void WriteDecompile(string outputFile)
        {
            if (fakeName != "")
                outputFile = Path.GetDirectoryName(outputFile) + "\\" + fakeName + ".lua";
            this.outputWriter = new StreamWriter(outputFile + "dc");
            outputWriter.WriteLine("; Decompiled by LuaDecompiler by JariK\n");
            functionWriteDecompile(this.Functions[0], 0, true);
            outputWriter.Close();
        }

        private void functionWriteDecompile(LuaFunction function, int functionLevel, bool skipHeader = false)
        {
            if(!skipHeader)
            {
                string funcName = function.getName(true);
                writeWithRightTabs(functionLevel - 1, "");
                if (funcName.Substring(0, 4) != "LUI." && funcName.Substring(0, 4) != "CoD.")
                    outputWriter.Write("local ");
                outputWriter.Write("function " + funcName + "(");
                for(int i = 0; i < function.parameterCount; i++)
                {
                    outputWriter.Write(((i > 0) ? ", ": "") + "arg" + i);
                }
                outputWriter.Write(")\n");
            }

            for (int i = 0; i < function.DecompileStrings.Count; i++)
                writeWithRightTabs(functionLevel, function.DecompileStrings[i] + "\n");

            if(!skipHeader)
            {
                writeWithRightTabs(functionLevel - 1, "end\n");
            }
            outputWriter.Write("\n");

            if(function.getName() == "__INIT__")
            {
                for(int i = 0; i < function.subFunctions.Count; i++)
                {
                    functionWriteDecompile(function.subFunctions[i], functionLevel+1);
                }
            }
        }

        private void writeWithRightTabs(int index, string opCodeString)
        {
            string str = "";
            for (int i = 0; i < index; i++)
                str += "\t";
            outputWriter.Write(str + opCodeString);
        }

        private void searchGameVersion()
        {
            inputReader.Seek(0xF6 , SeekOrigin.Begin);
            string bo4CheckString = inputReader.ReadFixedString(6);
            if (bo4CheckString == "TXHASH")
                this.game = Game.BlackOps4;
            else
                this.game = Game.BlackOps3;
            inputReader.Seek(0, SeekOrigin.Begin);
        }

        private LuaFunction loadFunction(long location = 0)
        {
            // Go to the start of the function
            if(location == 0)
                inputReader.Seek((game == Game.BlackOps3 ? 0xF2 : 0x101), SeekOrigin.Begin);

            // Begin position
            long beginPosition = inputReader.BaseStream.Position;

            // Get some info about this function 
            int upvalsCount = inputReader.ReadInt32();
            int parameterCount = inputReader.ReadByte();
            if (beginPosition < 0x110)
                parameterCount = 0;

            // Some unknown bytes
            if (beginPosition > 0x110)
                inputReader.ReadBytes(4);

            // Get some info about this function
            int registerCount = inputReader.ReadInt32();
            int opcodesCount = inputReader.ReadInt32();

            // Create a new function object
            LuaFunction function = new LuaFunction(registerCount);
            function.beginPosition = beginPosition;
            function.opcodesCount = opcodesCount;
            function.upvalsCount = upvalsCount;
            function.parameterCount = parameterCount;

            // Some unknown bytes (BO3 and BO4)
            if(!exportBlackOps2)
                inputReader.ReadBytes(4);
            
            // Add extra bytes to make it line up
            int extra = 4 - ((int)this.inputReader.BaseStream.Position % 4);
            if(extra > 0 && extra < 4)
                inputReader.ReadBytes(extra);

            // Load all OP Codes
            this.loadOPCodes(function);

            // Load all function strings
            this.loadFunctionStrings(function);

            float endingByte = inputReader.ReadInt32();
            float endingSingle = inputReader.ReadSingle();
            function.subFunctionsCount = inputReader.ReadInt32();
                       
            function.endPosition = inputReader.BaseStream.Position;
            
            Functions.Add(function);

            return function;
        }

        private void loadOPCodes(LuaFunction function)
        {
            // Go through all op codes
            for (int i = 0; i < function.opcodesCount; i++)
            {
                // Make a new OP Code object
                LuaOPCode opCode = new LuaOPCode();
                // Read + asign all data
                opCode.A = this.inputReader.ReadByte();

                int cValue = this.inputReader.ReadByte();
                byte flags = this.inputReader.ReadByte();
                if (GetBit(flags, 0) == 1)
                    cValue += 256;
                opCode.C = cValue;

                this.inputReader.Seek(-1, SeekOrigin.Current);
                opCode.B = this.inputReader.ReadByte() >> 1;
                byte flagsB = this.inputReader.ReadByte();
                if (GetBit(flagsB, 0) == 1)
                    opCode.B += 128;
                /*if (GetBit(flagsB, 1) == 1)
                    opCode.B += 256;*/

                opCode.Bx = (opCode.B * 512) + opCode.C;
                opCode.sBx = opCode.Bx - 65536 + 1;

                this.inputReader.Seek(-1, SeekOrigin.Current);
                opCode.OPCode = this.inputReader.ReadByte() >> 1;
                // Add it to the list
                function.OPCodes.Add(opCode);
            }
        }

        public static byte GetBit(long input, int bit)
        {
            return (byte)((input >> bit) & 1);
        }

        private void loadFunctionStrings(LuaFunction function)
        {
            // Get the number of strings + loop
            int stringCount = this.inputReader.ReadInt32();
            for (int i = 0; i < stringCount; i++)
            {
                // Make a new lua string object
                LuaString str = new LuaString();
                // Get the type
                int strType = inputReader.ReadByte();
                switch (strType)
                {
                    case 1:
                        str.String = (inputReader.ReadByte() == 1) ? "true" : "false";
                        str.StringType = StringType.boolean;
                        break;
                    case 3:
                        str.String = inputReader.ReadSingle().ToString("0.000000");
                        str.StringType = StringType.number;
                        break;
                    case 4:
                        int StringLength = inputReader.ReadInt32() - 1;
                        if (!exportBlackOps2)
                            inputReader.ReadInt32();
                        str.String = inputReader.ReadNullTerminatedString();
                        str.StringType = StringType.String;
                        break;
                    case 0xD:
                        string HashString;
                        ulong assetHash = inputReader.ReadUInt64() & 0xFFFFFFFFFFFFFFF;
                        if (!Program.AssetNameCache.Entries.TryGetValue(assetHash, out HashString))
                        {
                            str.String = String.Format("0x{0:X}", assetHash);
                            str.StringType = StringType.hash;
                        }
                        else
                        {
                            str.String = HashString;
                            str.StringType = StringType.String;
                        }
                        break;
                    case 0:
                        str.String = "nil";
                        str.StringType = StringType.nullptr;
                        break;
                }
                // Add it to the list
                function.Strings.Add(str);
            }
        }

        private void disassembleOPCodes(LuaFunction function, int index)
        {
            // Add the parameter opcodes because they arent included
            for (int i = 0; i < function.parameterCount; i++)
            {
                function.Registers[i] = "arg" + i;
                function.DisassembleStrings.Add(String.Format("r({0}) = arg{0}",
                            i));
            }
            for (int i = 0; i < function.OPCodes.Count; i++)
            {
                LuaOPCode opCode = function.OPCodes[i];
                try
                { 
                    switch (opCode.OPCode)
                    {
                        case 0x0: LuaStrings.ConnectWithDot(function, opCode); break;
                        case 0x1: LuaConditions.IfIsTrueFalse(function, opCode); break;
                        case 0x2: case 0x4C: LuaFunctions.CallFunctionWithParameters(function, opCode, i); break;
                        case 0x16: LuaFunctions.CallFunctionWithParameters(function, opCode, i, true); break;
                        case 0x4: LuaConditions.IfIsEqual(function, opCode); break;
                        case 0x5: LuaConditions.IfIsEqualBackwards(function, opCode); break;
                        case 0x6: LuaRegisters.GlobalRegisterToRegister(function, opCode); break;
                        case 0x7: LuaRegisters.RegisterToRegister(function, opCode); break;
                        case 0x8: LuaStrings.ConnectWithColon(function, opCode); break;
                        case 0x9: LuaOperators.Return(function, opCode, i); break;
                        case 0xA: LuaTables.GetIndex(function, opCode); break;
                        case 0xD: LuaRegisters.BooleanToRegister(function, opCode); break;
                        case 0xE: LuaLoops.StartForEachLoop(function, opCode); break;
                        case 0xF: LuaStrings.SetField(function, opCode, i); break;
                        case 0x10: LuaTables.SetTable(function, opCode); break;
                        case 0x11: LuaTables.SetTableBackwards(function, opCode); break;
                        case 0x19: LuaRegisters.LocalConstantToRegister(function, opCode); break;
                        case 0x1A: LuaRegisters.NilToRegister(function, opCode); break;
                        case 0x1B: LuaRegisters.RegisterToGlobal(function, opCode); break;
                        case 0x1C: LuaConditions.SkipLines(function, opCode, i); break;
                        case 0x26: LuaFunctions.GetUpValue(function, opCode); break;
                        case 0x27: LuaOperators.SetupVal(function, opCode); break;
                        case 0x28: LuaOperators.Add(function, opCode); break;
                        case 0x29: LuaOperators.AddBackWards(function, opCode); break;
                        case 0x2A: LuaOperators.Subtract(function, opCode); break;
                        case 0x2B: LuaOperators.SubtractBackWards(function, opCode); break;
                        case 0x2C: LuaOperators.Multiply(function, opCode); break;
                        case 0x2D: LuaOperators.MultiplyBackWards(function, opCode); break;
                        case 0x2E: LuaOperators.Divide(function, opCode); break;
                        case 0x2F: LuaOperators.DivideBackWards(function, opCode); break;
                        case 0x30: LuaOperators.Modulo(function, opCode); break;
                        case 0x31: LuaOperators.ModuloBackWards(function, opCode); break;
                        case 0x32: LuaOperators.Power(function, opCode); break;
                        case 0x33: LuaOperators.PowerBackWards(function, opCode); break;
                        case 0x34: LuaTables.EmptyTable(function, opCode); break;
                        case 0x35: LuaOperators.UnaryMinus(function, opCode); break;
                        case 0x36: LuaConditions.Not(function, opCode); break;
                        case 0x37: LuaOperators.Length(function, opCode); break;
                        case 0x38: LuaConditions.LargerThan(function, opCode); break;
                        case 0x39: LuaConditions.LargerThanBackwards(function, opCode); break;
                        case 0x3A: LuaConditions.LargerOrEqualThan(function, opCode); break;
                        case 0x3B: LuaConditions.LargerOrEqualThanBackwards(function, opCode); break;
                        case 0x3C: LuaOperators.ShiftLeft(function, opCode); break;
                        case 0x3D: LuaOperators.ShiftLeftBackwards(function, opCode); break;
                        case 0x40: LuaOperators.BinaryAnd(function, opCode); break;
                        case 0x42: LuaOperators.BinaryOr(function, opCode); break;
                        case 0x44: LuaStrings.ConnectWithDoubleDot(function, opCode); break;
                        case 0x45: LuaOperators.TestSet(function, opCode); break;
                        case 0x46: LuaLoops.StartForLoop(function, opCode); break;
                        case 0x47: LuaLoops.EndForLoop(function, opCode, i); break;
                        case 0x48: LuaTables.SetList(function, opCode); break;
                        case 0x49: LuaOperators.Close(function, opCode); break;
                        case 0x4A: LuaFunctions.Closure(function, opCode); break;
                        case 0x4B: LuaOperators.VarArg(function, opCode); break;
                        case 0x4D: LuaFunctions.CallFunctionWithoutParameters(function, opCode); break;
                        case 0x4F: LuaConditions.IfIsTrueFalse(function, opCode); break;
                        case 0x50: LuaOperators.NotR1(function, opCode); break;
                        case 0x51: LuaStrings.ConnectWithDot(function, opCode); break;
                        case 0x52: LuaStrings.SetField(function, opCode, i ); break;
                        case 0x54:
                            if (function.doingUpvals >= 0)
                            {
                                if(opCode.A == 1)
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
                                    errors++;
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
                        default:
                            function.DisassembleStrings.Add(String.Format("Unknown data(A:{0}, B:{1}, C:{2}, Bx:{3}, sBx:{4}, X:{5})",
                                opCode.A,
                                opCode.B,
                                opCode.C,
                                opCode.Bx,
                                opCode.sBx,
                                opCode.OPCode));
                            break;
                    }
                    if ((opCode.OPCode == 0xF || opCode.OPCode == 0x52) && function.getName() == "__INIT__")
                    {
                        if (this.game == Game.BlackOps4 && function.Registers[opCode.A].Length  == 14)
                        {
                            if(function.Registers[opCode.A] == "LUI.createMenu")
                                fakeName = function.Strings[opCode.B].String;
                        }
                        else if (this.game == Game.BlackOps4 && function.Registers[opCode.A].Length > 3)
                        {
                            if ((function.Registers[opCode.A].Substring(0, 4) == "CoD." || function.Registers[opCode.A].Substring(0, 4) == "LUI." || function.Registers[opCode.A].Substring(0, 6) == "Lobby.") &&
                                (function.Strings[opCode.B].String == "new"))
                            {
                                fakeName = function.Registers[opCode.A].Substring(4);
                            }
                        }
                        else if (this.game == Game.BlackOps4 && function.Registers[opCode.A].Length < 4)
                        {
                            if ((function.Registers[opCode.A] == "CoD" || function.Registers[opCode.A] == "LUI") &&
                                (function.Registers[opCode.C] == "table0"))
                            {
                                fakeName = function.Strings[opCode.B].String;
                            }
                        }
                    }
                    if (function.doingUpvals >= 0)
                    {
                        if (opCode.OPCode == 0x4A)
                            continue;
                        if (opCode.OPCode == 0x54)
                        {
                            if(opCode.A == 1)
                            {
                                function.subFunctions[function.lastFunctionClosure].UpvalsStrings.Add(function.Registers[opCode.C]);
                            }  
                            else if(opCode.A == 2)
                            {   
                                function.subFunctions[function.lastFunctionClosure].UpvalsStrings.Add(function.UpvalsStrings[opCode.C]);
                            }
                        }
                        else
                            function.doingUpvals = -1;
                    }
                }
                catch
                {
                    Console.WriteLine("Error occured while disassembling A: {3}, B: {4}, C: {5} OPcode: {0:X} at line {1} in function {2}", opCode.OPCode, i, function.getName(), opCode.A, opCode.B, opCode.C);
                    function.DisassembleStrings.Add(String.Format("Unknown data(A:{0}, B:{1}, C:{2}, Bx:{3}, sBx:{4}, X:{5})",
                        opCode.A,
                        opCode.B,
                        opCode.C,
                        opCode.Bx,
                        opCode.sBx,
                        opCode.OPCode));
                    errors++;
                }
            }
            
        }
    }
}
