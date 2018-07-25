using System;
using System.Diagnostics;
using System.IO;
using Phantasma.AssemblerLib;
using Phantasma.VM;

namespace Phantasma.AssemblerConsole
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            //if (args.Length == 0) return;
            //if (!File.Exists(args[0])) return;
            var lines = File.ReadAllLines("hello.asm");
            var semantemes = Semanteme.ProcessLines(lines);
            var table = new AddressTable(semantemes);
            var script = table.ToScript();
            //string out_path = args.Length >= 2 ? args[1] : Path.ChangeExtension(args[0], "avm");
            File.WriteAllBytes("result.pvm", script);
            var vm = new TestVM(script);
            vm.Execute();
        }
    }


    public class TestVM : VirtualMachine
    {
        public TestVM(byte[] script) : base(script)
        {

        }

        public override ExecutionState ExecuteInterop(string method)
        {
            if (method == "Runtime.Log")
            {
                var item = stack.Pop();
                Debug.WriteLine(item);
                return ExecutionState.Running;
            }

            return ExecutionState.Halt;
        }

        public override ExecutionContext LoadContext(byte[] key)
        {
            throw new NotImplementedException();
        }
    }
}
