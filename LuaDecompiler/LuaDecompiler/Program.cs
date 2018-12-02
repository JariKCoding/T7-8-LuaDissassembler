using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace LuaDecompiler
{
    class Program
    {
        static void Main(string[] args)
        {
            string[] files = args.Where(x => Path.GetExtension(x) == ".lua" && File.Exists(x)).ToArray();
            Console.WriteLine("Bo3/4 Lua Disassembler by JariK");
            LuaFile.errors = 0;
            foreach (string fileName in files)
            {
                Console.WriteLine("Exporting file: " + Path.GetFileName(fileName));
                LuaFile luaFile = new LuaFile(fileName);
                luaFile.Disassemble();
                luaFile.WriteDisassemble(fileName);
            }
            if(LuaFile.errors > 0)
                Console.ReadLine();
        }
    }
}
