﻿using Phantasma.Utils;
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
                    Code = ProcessMove();
                    break;
                case InstructionName.LOAD:
                    Code = ProcessLoad();
                    break;
                case InstructionName.COPY:
                case InstructionName.POP:
                case InstructionName.SWAP:
                case InstructionName.EXTCALL:
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

        private byte[] ProcessMove()
        {
            if (Arguments.Length != 2) throw new CompilerException(LineNumber, ERR_INCORRECT_NUMBER);
            var sb = new ScriptBuilder();
            if (Arguments[0].StartsWith("r") && Arguments[1].StartsWith("r"))
            {
                if (Int32.TryParse(Arguments[0].Substring(1), out int src))
                {
                    if (Int32.TryParse(Arguments[1].Substring(1), out int dest))
                    {
                        
                        sb.EmitMove(src, dest);
                    }
                }
            }
            return sb.ToScript();
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
            BigInteger bi;
            var sb = new ScriptBuilder();
            if (Arguments[0].StartsWith("r"))
            {
                var reg = Int32.Parse(Arguments[0].Substring(1));
                if (BigInteger.TryParse(Arguments[1], out bi))
                {
                    sb.EmitLoad(reg, bi);
                }
                else if (Arguments[1].StartsWith("0x"))
                {
                    sb.EmitLoad(reg, ParseHex(Arguments[1]));
                }
                else if (string.Compare(Arguments[1], "false", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    sb.EmitLoad(reg, false);
                }
                else if (string.Compare(Arguments[1], "true", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    sb.EmitLoad(reg, true);
                }
                else if (Arguments[1].StartsWith('\"'))
                {
                    sb.EmitLoad(reg, Arguments[1]);
                }
                return sb.ToScript();
            }
            throw new CompilerException(LineNumber, ERR_SYNTAX_ERROR); //todo
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
