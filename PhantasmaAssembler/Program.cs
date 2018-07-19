﻿using System.IO;
using Phantasma.AssemblerLib;

namespace PhantasmaAssembler
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            //if (args.Length == 0) return;
            //if (!File.Exists(args[0])) return;
            var lines = File.ReadAllLines("test.asm");
            var semantemes = Semanteme.ProcessLines(lines);
            var table = new AddressTable(semantemes);
            var script = table.ToScript();
            //string out_path = args.Length >= 2 ? args[1] : Path.ChangeExtension(args[0], "avm");
            File.WriteAllBytes("result.pvm", script);


        }
    }
}
