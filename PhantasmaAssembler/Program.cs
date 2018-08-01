using System;
using System.Collections.Generic;
using System.IO;
using Phantasma.AssemblerLib;
using Phantasma.Utils;

namespace Phantasma.AssemblerConsole
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            var arguments = new Arguments(args);

            string sourceFilePath = null;

            try
            {
                sourceFilePath = arguments.GetDefaultValue();
            }
            catch
            {
                Console.WriteLine($"{System.AppDomain.CurrentDomain.FriendlyName}.exe <filename.asm>");
                System.Environment.Exit(-1);
            }

            string[] lines = null;
            try
            {
                lines = File.ReadAllLines(sourceFilePath);
            }
            catch
            {
                Console.WriteLine("Error reading " + sourceFilePath);
                Environment.Exit(-1);
            }

            IEnumerable<Semanteme> semantemes = null;
            try
            {
                semantemes = Semanteme.ProcessLines(lines);
            }
            catch
            {
                Console.WriteLine("Error parsing " + sourceFilePath);
                Environment.Exit(-1);
            }

            var sb = new ScriptBuilder();
            byte[] script = null;

            try
            {               
                foreach (var entry in semantemes)
                {
                    //Console.WriteLine($"{entry}");
                    entry.Process(sb);
                }
                script = sb.ToScript();
            }
            catch
            {
                Console.WriteLine("Error assembling " + sourceFilePath);
                Environment.Exit(-1);
            }

            var extension = Path.GetExtension(sourceFilePath);
            var outputName = sourceFilePath.Replace(extension, ".svm");

            try
            {
                File.WriteAllBytes(outputName, script);
            }
            catch
            {
                Console.WriteLine("Error generating " + outputName);
                Environment.Exit(-1);
            }


            Console.WriteLine("Executing script...");
            var vm = new TestVM(script);
            vm.Execute();
        }
    }
}
