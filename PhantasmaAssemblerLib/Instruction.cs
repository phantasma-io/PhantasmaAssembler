using System;
using System.Globalization;
using System.Numerics;
using Phantasma.Core;
using Phantasma.Utils;
using Phantasma.VM;

namespace Phantasma.AssemblerLib
{
    internal class Instruction : Semanteme
    {
        private const string ERR_INCORRECT_NUMBER = "incorrect number of arguments";
        private const string ERR_INVALID_ARGUMENT = "invalid argument";
        private const string ERR_SYNTAX_ERROR = "syntax error";
        private const string REG_PREFIX = "r";
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
                case InstructionName.INC:
                case InstructionName.DEC:
                    Code = Process1Reg();
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
                case InstructionName.MOVE:
                    Code = Process2Reg();
                    break;

                //3 reg
                case InstructionName.AND:
                case InstructionName.OR:
                case InstructionName.XOR:
                case InstructionName.CAT:
                case InstructionName.EQUAL:
                case InstructionName.LT:
                case InstructionName.GT:
                case InstructionName.LTE:
                case InstructionName.GTE:
                case InstructionName.ADD:
                case InstructionName.SUB:
                case InstructionName.MUL:
                case InstructionName.DIV:
                case InstructionName.MOD:
                case InstructionName.SHL:
                case InstructionName.SHR:
                case InstructionName.MIN:
                case InstructionName.MAX:
                case InstructionName.PUT:
                case InstructionName.GET:
                    Code = Process3Reg();
                    break;

                case InstructionName.EXTCALL:
                    Code = ProcessExtCall();
                    break;

                case InstructionName.SUBSTR:
                case InstructionName.LEFT:
                case InstructionName.RIGHT:
                    Code = ProcessRightLeft();
                    break;

                case InstructionName.CTX:
                    Code = ProcessCtx();
                    break;
                case InstructionName.SWITCH:
                    Code = ProcessSwitch();
                    break;
                case InstructionName.RET:
                    Code = ProcessOthers();
                    break;
                case InstructionName.JMP:
                case InstructionName.JMPIF:
                case InstructionName.JMPNOT:
                    Code = ProcessJump();
                    break;
                case InstructionName.CALL:
                    Code = ProcessCall();
                    break;

                default:
                    throw new CompilerException(LineNumber, ERR_SYNTAX_ERROR);
            }
        }

        private byte[] ProcessSwitch()
        {
            if (Arguments.Length != 1) throw new CompilerException(LineNumber, ERR_INCORRECT_NUMBER);
            var sb = new ScriptBuilder();

            if (Arguments[0].Length == KeyPair.PublicKeyLength)
            {
                var type = (Opcode) Enum.Parse(typeof(Opcode), Name.ToString());
                sb.Emit(type, new[]
                {
                    Convert.ToByte(Arguments[0])
                });
                return sb.ToScript();
            }

            throw new CompilerException(LineNumber, ERR_INVALID_ARGUMENT); //todo
        }

        private byte[] ProcessCtx()
        {
            if (Arguments.Length != 2) throw new CompilerException(LineNumber, ERR_INCORRECT_NUMBER);
            var sb = new ScriptBuilder();
            if (Arguments[0].StartsWith(REG_PREFIX))
                if (int.TryParse(Arguments[0].Substring(1), out var dest_reg) &&
                    Arguments[1].Length == KeyPair.PublicKeyLength)
                {
                    var type = (Opcode) Enum.Parse(typeof(Opcode), Name.ToString());
                    sb.Emit(type, new[]
                    {
                        Convert.ToByte(dest_reg),
                        Convert.ToByte(Arguments[1])
                    });

                    return sb.ToScript();
                }

            throw new CompilerException(LineNumber, ERR_INVALID_ARGUMENT); //todo
        }

        private byte[] ProcessRightLeft()
        {
            if (Arguments.Length != 3) throw new CompilerException(LineNumber, ERR_INCORRECT_NUMBER);
            var sb = new ScriptBuilder();
            if (Arguments[0].StartsWith(REG_PREFIX) && Arguments[1].StartsWith(REG_PREFIX))
                if (int.TryParse(Arguments[0].Substring(1), out var src_reg) &&
                    int.TryParse(Arguments[1].Substring(1), out var dest_reg) &&
                    int.TryParse(Arguments[2], out var length))
                {
                    var type = (Opcode) Enum.Parse(typeof(Opcode), Name.ToString());
                    sb.Emit(type, new[]
                    {
                        Convert.ToByte(src_reg),
                        Convert.ToByte(dest_reg),
                        Convert.ToByte(length)
                    });
                    return sb.ToScript();
                }

            throw new CompilerException(LineNumber, ERR_INVALID_ARGUMENT); //todo
        }

        internal byte[] ProcessCall() //TODO check
        {
            if (Arguments.Length != 1) throw new CompilerException(LineNumber, ERR_INCORRECT_NUMBER);
            var sb = new ScriptBuilder();
            if (short.TryParse(Arguments[0], out var result))
            {
                var type = (Opcode) Enum.Parse(typeof(Opcode), Name.ToString());
                sb.Emit(type, new[]
                {
                    Convert.ToByte(result)
                });
                return sb.ToScript();
            }

            throw new CompilerException(LineNumber, ERR_INVALID_ARGUMENT); //todo
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

        internal byte[] ProcessExtCall() //TODO check
        {
            if (Arguments.Length != 1) throw new CompilerException(LineNumber, ERR_INCORRECT_NUMBER);
            var sb = new ScriptBuilder();
            var extCall = Arguments[0].Trim('\"');
            if (!string.IsNullOrEmpty(extCall))
            {
                sb.EmitCall(extCall);
                return sb.ToScript();
            }

            throw new CompilerException(LineNumber, ERR_INVALID_ARGUMENT); //todo
        }

        internal byte[] Process1Reg()
        {
            if (Arguments.Length != 1) throw new CompilerException(LineNumber, ERR_INCORRECT_NUMBER);
            var sb = new ScriptBuilder();
            if (Arguments[0].StartsWith(REG_PREFIX))
                if (int.TryParse(Arguments[0].Substring(1), out var reg))
                {
                    var type = (Opcode) Enum.Parse(typeof(Opcode), Name.ToString());
                    sb.Emit(type, new[]
                    {
                        Convert.ToByte(reg)
                    });
                    return sb.ToScript();
                }

            throw new CompilerException(LineNumber, ERR_INVALID_ARGUMENT); //todo
        }

        internal byte[] Process2Reg()
        {
            if (Arguments.Length != 2) throw new CompilerException(LineNumber, ERR_INCORRECT_NUMBER);
            var sb = new ScriptBuilder();
            if (Arguments[0].StartsWith(REG_PREFIX) && Arguments[1].StartsWith(REG_PREFIX))
                if (int.TryParse(Arguments[0].Substring(1), out var src) &&
                    int.TryParse(Arguments[1].Substring(1), out var dest))
                {
                    if (Name == InstructionName.MOVE)
                    {
                        sb.EmitMove(src, dest);
                    }
                    else
                    {
                        var type = (Opcode) Enum.Parse(typeof(Opcode), Name.ToString());
                        sb.Emit(type, new[] {Convert.ToByte(src), Convert.ToByte(dest)});
                    }

                    return sb.ToScript();
                }

            throw new CompilerException(LineNumber, ERR_INVALID_ARGUMENT); //todo
        }

        internal byte[] Process3Reg()
        {
            if (Arguments.Length != 3) throw new CompilerException(LineNumber, ERR_INCORRECT_NUMBER);
            var sb = new ScriptBuilder();
            if (Arguments[0].StartsWith(REG_PREFIX) && Arguments[1].StartsWith(REG_PREFIX) &&
                Arguments[2].StartsWith(REG_PREFIX))
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
                    return sb.ToScript();
                }

            throw new CompilerException(LineNumber, ERR_INVALID_ARGUMENT); //todo
        }

        internal byte[] ProcessLoad()
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
                else if (Arguments[1].IndexOf('\"') >= 0) sb.EmitLoad(reg, Arguments[1].Trim('\"'));
                return sb.ToScript();
            }

            throw new CompilerException(LineNumber, ERR_INVALID_ARGUMENT); //todo
        }

        internal byte[] ProcessOthers()
        {
            if (Arguments.Length != 0) throw new CompilerException(LineNumber, ERR_INCORRECT_NUMBER);
            return new[] {MakeScriptOp()};
        }
    }
}