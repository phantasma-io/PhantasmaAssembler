using Phantasma.Cryptography;
using Phantasma.VM;
using System;

namespace Phantasma.AssemblerConsole
{
    public class TestVM : VirtualMachine
    {
        public TestVM(byte[] script) : base(script)
        {

        }

        public override ExecutionContext LoadContext(string contextName)
        {
            throw new NotImplementedException();
        }

        public override ExecutionState ExecuteInterop(string method)
        {
            if (method == "Runtime.Log")
            {
                var item = Stack.Pop();
                Console.WriteLine(item);
                return ExecutionState.Running;
            }

            return ExecutionState.Halt;
        }

    }
}
