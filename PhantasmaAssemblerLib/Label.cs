using Phantasma.Utils;

namespace Phantasma.AssemblerLib
{
    internal class Label : Semanteme
    {
        public string Name;

        public override void Process(ScriptBuilder sb)
        {
            sb.EmitLabel(this.Name);
        }

        public override string ToString()
        {
            return this.Name;
        }

    }
}
