using System;
using System.Globalization;
using System.Numerics;
using Phantasma.Utils;
using Phantasma.VM;

namespace PhantasmaAssembler.ASM
{
    internal class Instruction : Semanteme
    {
        private const string ERR_INCORRECT_NUMBER = "incorrect number of arguments";
        private const string ERR_INVALID_ARGUMENT = "invalid argument";
        private const string ERR_SYNTAX_ERROR = "syntax error";
        public string[] Arguments;
        public byte[] Code;

        public InstructionName Name;

        private byte MakeScriptOp()
        {
            return (byte) (Opcode) Enum.Parse(typeof(Opcode), Name.ToString());
        }

        private byte[] ParseHex(string hex)
        {
            if (string.IsNullOrEmpty(hex))
                return new byte[0];
            if (hex.Length % 2 == 1)
                throw new CompilerException(LineNumber, ERR_INVALID_ARGUMENT);
            if (hex.StartsWith("0x")) hex = hex.Substring(2);
            var result = new byte[hex.Length / 2];
            for (var i = 0; i < result.Length; i++)
                result[i] = byte.Parse(hex.Substring(i * 2, 2), NumberStyles.AllowHexSpecifier);
            return result;
        }

        public void Process()
        {
            switch (Name)
            {
                //1 reg
                case InstructionName.PUSH:
                case InstructionName.POP:
                    Code = ProcessPush();
                    break;

                case InstructionName.MOVE:
                    Code = Process2Reg();
                    break;

                case InstructionName.LOAD:
                    Code = ProcessLoad();
                    break;

                //2 reg
                case InstructionName.SWAP:
                case InstructionName.SIZE:
                case InstructionName.NOT:
                case InstructionName.SIGN:
                case InstructionName.NEGATE:
                case InstructionName.ABS:
                case InstructionName.COPY:
                    Code = Process2Reg();
                    break;

                //3 reg
                case InstructionName.AND:
                case InstructionName.OR:
                case InstructionName.XOR:
                case InstructionName.CAT:
                    Code = Process3Reg();
                    break;

                case InstructionName.EXTCALL:
                case InstructionName.SUBSTR:
                case InstructionName.LEFT:
                case InstructionName.RIGHT:
                case InstructionName.EQUAL:
                case InstructionName.LT:
                case InstructionName.GT:
                case InstructionName.LTE:
                case InstructionName.GTE:
                case InstructionName.MIN:
                case InstructionName.MAX:
                case InstructionName.INC:
                case InstructionName.DEC:

                case InstructionName.ADD:
                case InstructionName.SUB:
                case InstructionName.MUL:
                case InstructionName.DIV:
                case InstructionName.MOD:
                case InstructionName.SHL:
                case InstructionName.SHR:
                case InstructionName.CTX:
                case InstructionName.SWITCH:
                case InstructionName.PUT:
                case InstructionName.GET:
                case InstructionName.RET:
                    Code = ProcessOthers();
                    break;
                case InstructionName.JMP:
                case InstructionName.JMPIF:
                case InstructionName.JMPNOT:
                case InstructionName.CALL:
                    Code = ProcessJump();
                    break;

                default:
                    throw new CompilerException(LineNumber, ERR_SYNTAX_ERROR);
            }
        }

        private byte[] ProcessCopy()
        {
            throw new NotImplementedException();
        }

        private byte[] Process2Reg()
        {
            if (Arguments.Length != 2) throw new CompilerException(LineNumber, ERR_INCORRECT_NUMBER);
            var sb = new ScriptBuilder();
            if (Arguments[0].StartsWith("r") && Arguments[1].StartsWith("r"))
                if (int.TryParse(Arguments[0].Substring(1), out var src))
                    if (int.TryParse(Arguments[1].Substring(1), out var dest))
                        sb.EmitMove(src, dest);
            return sb.ToScript();
        }

        private byte[] ProcessAppCall()
        {
            if (Arguments.Length != 1) throw new CompilerException(LineNumber, ERR_INCORRECT_NUMBER);
            var hash = ParseHex(Arguments[0]);
            if (hash.Length != 20) throw new CompilerException(LineNumber, ERR_INVALID_ARGUMENT);
            var result = new byte[21];
            result[0] = MakeScriptOp();
            Buffer.BlockCopy(hash, 0, result, 1, 20);
            return result;
        }

        private byte[] ProcessLoad()
        {
            if (Arguments.Length != 2) throw new CompilerException(LineNumber, ERR_INCORRECT_NUMBER);
            BigInteger bi;
            var sb = new ScriptBuilder();
            if (Arguments[0].StartsWith("r"))
            {
                var reg = int.Parse(Arguments[0].Substring(1));
                if (BigInteger.TryParse(Arguments[1], out bi))
                    sb.EmitLoad(reg, bi);
                else if (Arguments[1].StartsWith("0x"))
                    sb.EmitLoad(reg, ParseHex(Arguments[1]));
                else if (string.Compare(Arguments[1], "false", StringComparison.OrdinalIgnoreCase) == 0)
                    sb.EmitLoad(reg, false);
                else if (string.Compare(Arguments[1], "true", StringComparison.OrdinalIgnoreCase) == 0)
                    sb.EmitLoad(reg, true);
                else if (Arguments[1].StartsWith('\"')) sb.EmitLoad(reg, Arguments[1]);
                return sb.ToScript();
            }

            throw new CompilerException(LineNumber, ERR_SYNTAX_ERROR); //todo
        }

        internal byte[] ProcessJump(short offset = 0)
        {
            if (Arguments.Length != 1) throw new CompilerException(LineNumber, ERR_INCORRECT_NUMBER);
            var data = BitConverter.GetBytes(offset);
            var result = new byte[3];
            result[0] = MakeScriptOp();
            Buffer.BlockCopy(data, 0, result, 1, sizeof(short));
            return result;
        }

        private byte[] ProcessOthers()
        {
            if (Arguments.Length != 0) throw new CompilerException(LineNumber, ERR_INCORRECT_NUMBER);
            return new[] {MakeScriptOp()};
        }

        private byte[] ProcessPush()
        {
            if (Arguments.Length != 1) throw new CompilerException(LineNumber, ERR_INCORRECT_NUMBER);
            var sb = new ScriptBuilder();
            if (Arguments[0].StartsWith("r"))
                if (int.TryParse(Arguments[0].Substring(1), out var reg))
                    sb.EmitPush(reg);
            return sb.ToScript();
        }

        private byte[] Process3Reg()
        {
            if (Arguments.Length != 3) throw new CompilerException(LineNumber, ERR_INCORRECT_NUMBER);
            var sb = new ScriptBuilder();
            if (Arguments[0].StartsWith("r") && Arguments[1].StartsWith("r") && Arguments[2].StartsWith("r"))
                if (int.TryParse(Arguments[0].Substring(1), out var src_a_reg) &&
                    int.TryParse(Arguments[1].Substring(1), out var src_b_reg) &&
                    int.TryParse(Arguments[2].Substring(1), out var dest_reg))
                {
                    var type = (Opcode) Enum.Parse(typeof(Opcode), Name.ToString());
                    sb.Emit(type, new[]
                    {
                        Convert.ToByte(src_a_reg),
                        Convert.ToByte(src_b_reg),
                        Convert.ToByte(dest_reg)
                    });
                }

           
            return sb.ToScript();
        }
    }
}