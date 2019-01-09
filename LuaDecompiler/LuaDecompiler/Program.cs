using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace LuaDecompiler
{
    class Program
    {
        public static PackageIndex AssetNameCache;

        static void Main(string[] args)
        {
            string[] files = args.Where(x => (Path.GetExtension(x) == ".lua" || Path.GetExtension(x) == ".luac") && File.Exists(x)).ToArray();
            AssetNameCache = new PackageIndex();
            AssetNameCache.Load("PackageIndex\\bo4_strings.wni");
            Console.WriteLine("Bo3/4 Lua Disassembler by JariK");
            LuaFile.errors = 0;
            //foreach (string fileName in files)
            foreach (string fileName in Directory.GetFiles(@"E:\ReverseEngineering\Bo4HashFinder\Bo4HashFinder\bin\Debug\LuaFile", "*.lua", SearchOption.AllDirectories))
            {
                try
                {
                    Console.WriteLine("Exporting file: " + Path.GetFileName(fileName));
                    LuaFile luaFile = new LuaFile(fileName);
                    luaFile.Disassemble();
                    luaFile.WriteDisassemble(fileName);
                    LuaDecompiler lua = new LuaDecompiler(luaFile);
                    lua.Decompile(fileName);
                }
                catch
                {

                }
            }
            //if(LuaFile.errors > 0)
                Console.ReadLine();
        }
    }
}
