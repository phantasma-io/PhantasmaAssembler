using Phantasma.VM;
using System;

namespace Phantasma.AssemblerConsole
{
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
                Console.WriteLine(item);
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
