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
            AssetNameCache.Load("PackageIndex\\bo4_localizedstrings.wni");
            Console.WriteLine("Bo3/4 Lua Disassembler by JariK");
            LuaFile.errors = 0;
            foreach (string fileName in files)
            {
                Console.WriteLine("Exporting file: " + Path.GetFileName(fileName));
                LuaFile luaFile = new LuaFile(fileName);
                luaFile.Disassemble();
                luaFile.WriteDisassemble(fileName);
                luaFile.WriteDecompile(fileName);
            }
            if(LuaFile.errors > 0)
                Console.ReadLine();
        }
    }
}
