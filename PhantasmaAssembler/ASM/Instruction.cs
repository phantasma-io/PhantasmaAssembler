using Phantasma.Utils;
using System;
using System.Globalization;
using System.Numerics;
using System.Text;
using System.Linq;
using Phantasma.VM;

namespace PhantasmaAssembler.ASM
{
    internal class Instruction : Semanteme
    {
        private const string ERR_INCORRECT_NUMBER = "incorrect number of arguments";
        private const string ERR_INVALID_ARGUMENT = "invalid argument";
        private const string ERR_SYNTAX_ERROR = "syntax error";

        public InstructionName Name;
        public string[] Arguments;
        public byte[] Code;

        private byte MakeScriptOp()
        {
            return (byte)(Opcode)Enum.Parse(typeof(Opcode), Name.ToString());
        }

        private byte[] ParseHex(string hex)
        {
            if (string.IsNullOrEmpty(hex))
                return new byte[0];
            if (hex.Length % 2 == 1)
                throw new CompilerException(LineNumber, ERR_INVALID_ARGUMENT);
            if (hex.StartsWith("0x")) hex = hex.Substring(2);
            byte[] result = new byte[hex.Length / 2];
            for (int i = 0; i < result.Length; i++)
                result[i] = byte.Parse(hex.Substring(i * 2, 2), NumberStyles.AllowHexSpecifier);
            return result;
        }

        public void Process()
        {
            switch (Name)
            {
                case InstructionName.PUSH:
                    Code = ProcessPush();
                    break;
                case InstructionName.MOVE:
                    if (Arguments.Length != 2) throw new CompilerException(LineNumber, ERR_INCORRECT_NUMBER);
                    byte[] result = new byte[3];
                    result[0] = MakeScriptOp();

                    for (int i = 0; i < Arguments.Length; i++)
                    {
                        if (Arguments[i].StartsWith("r"))
                        {
                            int x = 0;
                            if (Int32.TryParse(Arguments[i].Substring(1), out x))
                            {
                                result[i + 1] = Convert.ToByte(x);
                            }
                        }
                    }
                    Code = result;
                    break;
                case InstructionName.COPY:
                case InstructionName.LOAD:
                    Code = ProcessLoad();
                    break;
                case InstructionName.POP:
                case InstructionName.SWAP:
                case InstructionName.EXTCALL:
                case InstructionName.JMP:
                case InstructionName.JMPIF:
                case InstructionName.JMPNOT:
                case InstructionName.RET:
                case InstructionName.CAT:
                case InstructionName.SUBSTR:
                case InstructionName.LEFT:
                case InstructionName.RIGHT:
                case InstructionName.SIZE:
                case InstructionName.NOT:
                case InstructionName.AND:
                case InstructionName.OR:
                case InstructionName.XOR:
                case InstructionName.EQUAL:
                case InstructionName.LT:
                case InstructionName.GT:
                case InstructionName.LTE:
                case InstructionName.GTE:
                case InstructionName.MIN:
                case InstructionName.MAX:
                case InstructionName.INC:
                case InstructionName.DEC:
                case InstructionName.SIGN:
                case InstructionName.NEGATE:
                case InstructionName.ABS:
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
                case InstructionName.CALL:
                    Code = ProcessJump();
                    break;

                default:
                    throw new CompilerException(LineNumber, ERR_SYNTAX_ERROR);
            }
        }

        private byte[] ProcessAppCall()
        {
            if (Arguments.Length != 1) throw new CompilerException(LineNumber, ERR_INCORRECT_NUMBER);
            byte[] hash = ParseHex(Arguments[0]);
            if (hash.Length != 20) throw new CompilerException(LineNumber, ERR_INVALID_ARGUMENT);
            byte[] result = new byte[21];
            result[0] = MakeScriptOp();
            Buffer.BlockCopy(hash, 0, result, 1, 20);
            return result;
        }

        private byte[] ProcessLoad()
        {
            if (Arguments.Length != 2) throw new CompilerException(LineNumber, ERR_INCORRECT_NUMBER);
            throw new NotImplementedException();
        }

        internal byte[] ProcessJump(short offset = 0)
        {
            if (Arguments.Length != 1) throw new CompilerException(LineNumber, ERR_INCORRECT_NUMBER);
            byte[] data = BitConverter.GetBytes(offset);
            byte[] result = new byte[3];
            result[0] = MakeScriptOp();
            Buffer.BlockCopy(data, 0, result, 1, sizeof(short));
            return result;
        }

        private byte[] ProcessOthers()
        {
            if (Arguments.Length != 0) throw new CompilerException(LineNumber, ERR_INCORRECT_NUMBER);
            return new[] { MakeScriptOp() };
        }

        private byte[] ProcessPush()
        {
            if (Arguments.Length != 1) throw new CompilerException(LineNumber, ERR_INCORRECT_NUMBER);
            BigInteger bi;
            ScriptBuilder sb = new ScriptBuilder();
            //if (BigInteger.TryParse(Arguments[0], out bi))
            throw new NotImplementedException();
            //else if (string.Compare(Arguments[0], "true", true) == 0)
            //    return new[] { (byte)Opcode.PUSHT };
            //else if (string.Compare(Arguments[0], "false", true) == 0)
            //    return new[] { (byte)Opcode.PUSHF };
            //else if (Arguments[0].StartsWith("0x"))
            //    using (ScriptBuilder sb = new ScriptBuilder())
            //        return sb.EmitPush(ParseHex(Arguments[0])).ToArray();
            //else
            throw new CompilerException(LineNumber, ERR_INVALID_ARGUMENT);
        }

        //private byte[] ProcessSysCall()
        //{
        //    if (Arguments.Length != 1) throw new CompilerException(LineNumber, ERR_INCORRECT_NUMBER);
        //    byte[] data = Encoding.ASCII.GetBytes(Arguments[0]);
        //    if (data.Length > 252) throw new CompilerException(LineNumber, ERR_INVALID_ARGUMENT);
        //    byte[] result = new byte[data.Length + 2];
        //    result[0] = (byte)Opcode.SYSCALL;
        //    result[1] = (byte)data.Length;
        //    Buffer.BlockCopy(data, 0, result, 2, data.Length);
        //    return result;
        //}
    }
}
