﻿using Phantasma.VM.Utils;

namespace Phantasma.AssemblerLib
{
    internal class Label : Semanteme
    {
        public string Name;

        public override void Process(ScriptBuilder sb)
        {
            sb.EmitLabel(Name);
        }

        public override string ToString()
        {
            return Name;
        }

    }
}
