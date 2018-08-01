﻿using System;
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
        private const string LABEL_PREFIX = "@";
        private const char STRING_PREFIX = '\"';

        public string[] Arguments;

        public Opcode Opcode;

        public override string ToString()
        {
            return this.Opcode.ToString();
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
            switch (Opcode)
            {
                //1 reg
                case Opcode.PUSH:
                case Opcode.POP:
                case Opcode.INC:
                case Opcode.DEC:
                    Process1Reg(sb);
                    break;

                case Opcode.LOAD:
                    ProcessLoad(sb);
                    break;

                //2 reg
                case Opcode.SWAP:
                case Opcode.SIZE:
                case Opcode.NOT:
                case Opcode.SIGN:
                case Opcode.NEGATE:
                case Opcode.ABS:
                case Opcode.COPY:
                case Opcode.MOVE:
                    Process2Reg(sb);
                    break;

                //3 reg
                case Opcode.AND:
                case Opcode.OR:
                case Opcode.XOR:
                case Opcode.CAT:
                case Opcode.EQUAL:
                case Opcode.LT:
                case Opcode.GT:
                case Opcode.LTE:
                case Opcode.GTE:
                case Opcode.ADD:
                case Opcode.SUB:
                case Opcode.MUL:
                case Opcode.DIV:
                case Opcode.MOD:
                case Opcode.SHL:
                case Opcode.SHR:
                case Opcode.MIN:
                case Opcode.MAX:
                case Opcode.PUT:
                case Opcode.GET:
                    Process3Reg(sb);
                    break;

                case Opcode.EXTCALL:
                    ProcessExtCall(sb);
                    break;

                case Opcode.SUBSTR:
                case Opcode.LEFT:
                case Opcode.RIGHT:
                    ProcessRightLeft(sb);
                    break;

                case Opcode.CTX:
                    ProcessCtx(sb);
                    break;

                case Opcode.SWITCH:
                    ProcessSwitch(sb);
                    break;

                case Opcode.RET:
                    ProcessOthers(sb);
                    break;

                case Opcode.JMPIF:
                case Opcode.JMPNOT:
                    ProcessJumpIf(sb);
                    break;

                case Opcode.CALL:
                    ProcessCall(sb);
                    break;

                case Opcode.JMP:
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
            if (Arguments[0].StartsWith(REG_PREFIX) && Arguments[1].StartsWith(LABEL_PREFIX))
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
            if (Arguments.Length != 1)
            {
                throw new CompilerException(LineNumber, ERR_INCORRECT_NUMBER);
            }

            if (!Arguments[0].StartsWith(LABEL_PREFIX))
            {
                throw new CompilerException(LineNumber, ERR_INVALID_ARGUMENT);
            }

            sb.EmitJump(MakeScriptOp(), Arguments[0]);
        }

        private void ProcessExtCall(ScriptBuilder sb)
        {
            if (Arguments.Length != 1)
            {
                throw new CompilerException(LineNumber, ERR_INCORRECT_NUMBER);
            }

            if (Arguments[0].IndexOf(STRING_PREFIX) >= 0)
            {
                var extCall = Arguments[0].Trim(STRING_PREFIX);

                if (string.IsNullOrEmpty(extCall))
                {
                    throw new CompilerException(LineNumber, ERR_INVALID_ARGUMENT);
                }

                sb.EmitExtCall(extCall);
            }
            else
            if (Arguments[0].StartsWith(REG_PREFIX))
            {
                var reg = byte.Parse(Arguments[0].Substring(1));
                sb.Emit(Opcode.EXTCALL, new byte[] { reg });
            }
            else
            {
                throw new CompilerException(LineNumber, ERR_INVALID_ARGUMENT);
            }
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
                    if (Opcode == Opcode.MOVE)
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
                        sb.Emit(this.Opcode, new[]
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