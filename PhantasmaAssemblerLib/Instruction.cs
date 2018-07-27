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
        private const string LABLE_PREFIX = "@";
        private const char STRING_PREFIX = '\"';

        public string[] Arguments;

        public InstructionName Name;

        public override string ToString()
        {
            return this.Name.ToString();
        }

        private Opcode MakeScriptOp()
        {
            return (Opcode)Enum.Parse(typeof(Opcode), Name.ToString());
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

        public override void Process(ScriptBuilder sb)
        {
            switch (Name)
            {
                //1 reg
                case InstructionName.PUSH:
                case InstructionName.POP:
                case InstructionName.INC:
                case InstructionName.DEC:
                    Process1Reg(sb);
                    break;

                case InstructionName.LOAD:
                    ProcessLoad(sb);
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
                    Process2Reg(sb);
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
                    Process3Reg(sb);
                    break;

                case InstructionName.EXTCALL:
                    ProcessExtCall(sb);
                    break;

                case InstructionName.SUBSTR:
                case InstructionName.LEFT:
                case InstructionName.RIGHT:
                    ProcessRightLeft(sb);
                    break;

                case InstructionName.CTX:
                    ProcessCtx(sb);
                    break;

                case InstructionName.SWITCH:
                    ProcessSwitch(sb);
                    break;

                case InstructionName.RET:
                    ProcessOthers(sb);
                    break;

                case InstructionName.JMPIF:
                case InstructionName.JMPNOT:
                    ProcessJumpIf(sb);
                    break;

                case InstructionName.CALL:
                case InstructionName.JMP:
                    ProcessJump(sb);
                    break;

                default:
                    throw new CompilerException(LineNumber, ERR_SYNTAX_ERROR);
            }
        }

        private void ProcessSwitch(ScriptBuilder sb)
        {
            if (Arguments.Length != 1) throw new CompilerException(LineNumber, ERR_INCORRECT_NUMBER);
            if (Arguments[0].Length == KeyPair.PublicKeyLength)
            {
                sb.Emit(MakeScriptOp(), new[]
                {
                    Convert.ToByte(Arguments[0])
                });
                return;
            }
            throw new CompilerException(LineNumber, ERR_INVALID_ARGUMENT);
        }

        private void ProcessCtx(ScriptBuilder sb)
        {
            if (Arguments.Length != 2) throw new CompilerException(LineNumber, ERR_INCORRECT_NUMBER);
            if (Arguments[0].StartsWith(REG_PREFIX))
                if (int.TryParse(Arguments[0].Substring(1), out var dest_reg) &&
                    Arguments[1].Length == KeyPair.PublicKeyLength)
                {
                    sb.Emit(MakeScriptOp(), new[]
                    {
                        Convert.ToByte(dest_reg),
                        Convert.ToByte(Arguments[1])
                    });

                    return;
                }
            throw new CompilerException(LineNumber, ERR_INVALID_ARGUMENT);
        }

        private void ProcessRightLeft(ScriptBuilder sb)
        {
            if (Arguments.Length != 3) throw new CompilerException(LineNumber, ERR_INCORRECT_NUMBER);
            if (Arguments[0].StartsWith(REG_PREFIX) && Arguments[1].StartsWith(REG_PREFIX))
                if (int.TryParse(Arguments[0].Substring(1), out var src_reg) &&
                    int.TryParse(Arguments[1].Substring(1), out var dest_reg) &&
                    int.TryParse(Arguments[2], out var length))
                {
                    sb.Emit(MakeScriptOp(), new[]
                    {
                        Convert.ToByte(src_reg),
                        Convert.ToByte(dest_reg),
                        Convert.ToByte(length)
                    });
                    return;
                }

            throw new CompilerException(LineNumber, ERR_INVALID_ARGUMENT);
        }

        private void ProcessJumpIf(ScriptBuilder sb)
        {
            if (Arguments.Length != 2) throw new CompilerException(LineNumber, ERR_INCORRECT_NUMBER);
            if (Arguments[0].StartsWith(REG_PREFIX) && Arguments[1].StartsWith(LABLE_PREFIX))
            {
                if (int.TryParse(Arguments[0].Substring(1), out var reg))
                {
                    sb.EmitConditionalJump(MakeScriptOp(), reg, Arguments[1]);
                    return;
                }
            }

            throw new CompilerException(LineNumber, ERR_INVALID_ARGUMENT);
        }

        private void ProcessJump(ScriptBuilder sb)
        {
            if (Arguments.Length != 1) throw new CompilerException(LineNumber, ERR_INCORRECT_NUMBER);
            if (Arguments[0].StartsWith(LABLE_PREFIX))
            {
                sb.EmitJump(MakeScriptOp(), Arguments[0]);
                return;
            }
            throw new CompilerException(LineNumber, ERR_INVALID_ARGUMENT);
        }

        private void ProcessExtCall(ScriptBuilder sb) //TODO check
        {
            if (Arguments.Length != 1) throw new CompilerException(LineNumber, ERR_INCORRECT_NUMBER);
            var extCall = Arguments[0].Trim(STRING_PREFIX);
            if (!string.IsNullOrEmpty(extCall))
            {
                sb.EmitExtCall(extCall);
                return;
            }
            throw new CompilerException(LineNumber, ERR_INVALID_ARGUMENT);
        }

        private void Process1Reg(ScriptBuilder sb)
        {
            if (Arguments.Length != 1) throw new CompilerException(LineNumber, ERR_INCORRECT_NUMBER);
            if (Arguments[0].StartsWith(REG_PREFIX))
                if (int.TryParse(Arguments[0].Substring(1), out var reg))
                {
                    sb.Emit(MakeScriptOp(), new[]
                    {
                        Convert.ToByte(reg)
                    });
                    return;
                }

            throw new CompilerException(LineNumber, ERR_INVALID_ARGUMENT);
        }

        private void Process2Reg(ScriptBuilder sb)
        {
            if (Arguments.Length != 2) throw new CompilerException(LineNumber, ERR_INCORRECT_NUMBER);
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
                        sb.Emit(MakeScriptOp(), new[] { Convert.ToByte(src), Convert.ToByte(dest) });
                    }

                    return;
                }

            throw new CompilerException(LineNumber, ERR_INVALID_ARGUMENT);
        }

        private void Process3Reg(ScriptBuilder sb)
        {
            if (Arguments.Length <= 1 || Arguments.Length > 3) throw new CompilerException(LineNumber, ERR_INCORRECT_NUMBER);
            if (Arguments[0].StartsWith(REG_PREFIX) && Arguments[1].StartsWith(REG_PREFIX))
                if (int.TryParse(Arguments[0].Substring(1), out var src_a_reg) &&
                    int.TryParse(Arguments[1].Substring(1), out var src_b_reg))
                {
                    if (Arguments.Length == 2)
                    {
                        sb.Emit(MakeScriptOp(), new[]
                        {
                            Convert.ToByte(src_a_reg),
                            Convert.ToByte(src_b_reg),
                            Convert.ToByte(src_a_reg)
                        });
                    }
                    else if (int.TryParse(Arguments[2].Substring(1), out var dest_reg) && Arguments[2].StartsWith(REG_PREFIX))
                    {
                        sb.Emit(MakeScriptOp(), new[]
                        {
                            Convert.ToByte(src_a_reg),
                            Convert.ToByte(src_b_reg),
                            Convert.ToByte(dest_reg)
                        });
                    }

                    return;
                }

            throw new CompilerException(LineNumber, ERR_INVALID_ARGUMENT);
        }

        private void ProcessLoad(ScriptBuilder sb)
        {
            if (Arguments.Length != 2) throw new CompilerException(LineNumber, ERR_INCORRECT_NUMBER);
            if (Arguments[0].StartsWith(REG_PREFIX))
            {
                var reg = int.Parse(Arguments[0].Substring(1));
                if (BigInteger.TryParse(Arguments[1], out var bi))
                    sb.EmitLoad(reg, bi);
                else if (Arguments[1].StartsWith("0x"))
                    sb.EmitLoad(reg, ParseHex(Arguments[1]));
                else if (string.Compare(Arguments[1], "false", StringComparison.OrdinalIgnoreCase) == 0)
                    sb.EmitLoad(reg, false);
                else if (string.Compare(Arguments[1], "true", StringComparison.OrdinalIgnoreCase) == 0)
                    sb.EmitLoad(reg, true);
                else if (Arguments[1].IndexOf(STRING_PREFIX) >= 0) sb.EmitLoad(reg, Arguments[1].Trim(STRING_PREFIX));
                return;
            }

            throw new CompilerException(LineNumber, ERR_INVALID_ARGUMENT);
        }

        private void ProcessOthers(ScriptBuilder sb)
        {
            if (Arguments.Length != 0) throw new CompilerException(LineNumber, ERR_INCORRECT_NUMBER);
            sb.Emit(MakeScriptOp());
        }
    }
}