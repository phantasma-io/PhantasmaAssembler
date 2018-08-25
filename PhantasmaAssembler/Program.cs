using System;
using System.Collections.Generic;
using System.IO;
using Phantasma.Blockchain;
using Phantasma.AssemblerLib;
using Phantasma.Utils;
using Phantasma.Cryptography;
using Phantasma.VM.Types;
using Phantasma.Utils.Log;

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
            catch (Exception e)
            {
                Console.WriteLine("Error parsing " + sourceFilePath + " :" + e.Message);
                Environment.Exit(-1);
            }

            var sb = new ScriptBuilder();
            byte[] script = null;

            try
            {               
                foreach (var entry in semantemes)
                {
                    Console.WriteLine($"{entry}");
                    entry.Process(sb);
                }
                script = sb.ToScript();
            }
            catch (Exception e)
            {
                Console.WriteLine("Error assembling " + sourceFilePath + " :" + e.Message);
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
            var keys = KeyPair.Generate();
            var chain = new Chain(keys, new ConsoleLogger());
            var tx = new Transaction(script, 0, 0);

            var vm = new RuntimeVM(chain, tx);
            var state = vm.Execute();
            Console.WriteLine("State = " + state);

            /*var vm = new TestVM(script);
            vm.Execute();*/
        }
    }
}
