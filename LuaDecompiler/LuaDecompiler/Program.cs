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
            
            AssetNameCache = new PackageIndex();
            AssetNameCache.Load(AppDomain.CurrentDomain.BaseDirectory + "\\PackageIndex\\bo4_localizedstrings.wni");
            AssetNameCache.Load(AppDomain.CurrentDomain.BaseDirectory + "\\PackageIndex\\bo4_enums.wni");
            AssetNameCache.Load(AppDomain.CurrentDomain.BaseDirectory + "\\PackageIndex\\bo4_functions.wni");
            AssetNameCache.Load(AppDomain.CurrentDomain.BaseDirectory + "\\PackageIndex\\bo4_materials.wni");
            Console.WriteLine("Bo3/4 Lua Disassembler by JariK");
            LuaFile.errors = 0;
            string[] files = new string[1];
            if (args.Length == 0)
            {
                Console.WriteLine("Give the folder that you want to decompile: ");
                string folder = Console.ReadLine();
                if (Directory.Exists(folder))
                {
                    files = Directory.GetFiles(folder, "*.lua*", SearchOption.AllDirectories);
                }
            }
            else
            {
                files = args.Where(x => (Path.GetExtension(x) == ".lua" || Path.GetExtension(x) == ".luac") && File.Exists(x)).ToArray();
            }
            
            foreach (string fileName in files)
            {
                if(Path.GetExtension(fileName) != ".lua" && Path.GetExtension(fileName) != ".luac")
                {
                    continue;
                }
                Console.WriteLine("Exporting file: " + Path.GetFileName(fileName));
                LuaFile luaFile = new LuaFile(fileName);
                luaFile.Disassemble();
                luaFile.WriteDisassemble(fileName);
                LuaDecompiler lua = new LuaDecompiler(luaFile);
                lua.Decompile(fileName);
            }
            if(LuaFile.errors > 0 || files.Length > 1)
                Console.ReadLine();
        }
    }
}
