using System;
using System.Collections.Generic;
using System.Reflection;

namespace FTG.Studios.BISC {
    
    public enum Opcode : byte { 
        NOP = 0x00, HLT = 0x01, SYS = 0x02, CALL = 0x03, RET = 0x04,
        LLI = 0x05, LUI = 0x06, ADD = 0x07, SUB = 0x08, MUL = 0x09, DIV = 0x0A, MOD = 0x0B, INC = 0x0C, DEC = 0x0D, NEG = 0x0E,
        NOT = 0x0F, AND = 0x10, OR = 0x11, XOR = 0x12, NAND = 0x13, NOR = 0x14, XNOR = 0x15, BSL = 0x16, BSR = 0x17
    };

    /// <summary>
    /// BISC virtual machine.
    /// </summary>
    public class VirtualMachine {

        delegate bool InstructionHandler(byte opcode, byte arg0, byte arg1, byte arg2);
        InstructionHandler[] instructions;

        const int NUM_REGISTERS = 20;
        UInt32[] registers;
        string[] register_names = { "pc", "sp", "ra", "rv", "r0", "r1", "r2", "r3", "r4", "r5", "r6", "r7", "f0", "f1", "f2", "f3", "f4", "f5", "f6", "f7" };

        UInt32 pc { get { return registers[0]; } set { registers[0] = value; } }
        UInt32 sp { get { return registers[1]; } set { registers[1] = value; } }
        UInt32 ra { get { return registers[2]; } set { registers[2] = value; } }
        UInt32 rv { get { return registers[3]; } set { registers[3] = value; } }

        public VirtualMachine() {
            registers = new UInt32[NUM_REGISTERS];
            instructions = new InstructionHandler[] { NOP, HLT, null ,null, null, LLI, LUI, ADD };
        }

        /// <summary>
        /// Executes a byte array as BISC instructions.
        /// </summary>
        /// <param name="instructions">Instructions in byte form.</param>
        public void Execute(byte[] instructions) {
            UInt32[] words = new UInt32[instructions.Length / 4];
            for (int i = 0; i < words.Length; i++) {
                UInt32 word = BitConverter.ToUInt32(instructions, i * 4);
                words[i] = word;
            }
            Execute(words);
        }

        /// <summary>
        /// Executes an array of 32-bit BISC instructions.
        /// </summary>
        /// <param name="instructions">Array of instructions.</param>
        public void Execute(UInt32[] instructions) {
            for (int i = 0; i < instructions.Length; i++) {
                ExecuteInstruction(instructions[i]);
            }
        } 

        /// <summary>
        /// Executes a single 32-bit BISC instruction.
        /// </summary>
        /// <param name="instruction">32-bit instruction.</param>
        public void ExecuteInstruction(UInt32 instruction) {
            //Console.WriteLine("Executing instruction: 0x{0:x8}", instruction);

            byte[] bytes = BitConverter.GetBytes(instruction);
            if (BitConverter.IsLittleEndian) Array.Reverse(bytes);

            byte opcode = bytes[0];
            byte arg0 = bytes[1];
            byte arg1 = bytes[2];
            byte arg2 = bytes[3];

            Console.Clear();
            Console.SetCursorPosition(0, NUM_REGISTERS);
            if (opcode >= 0 && opcode < instructions.Length) {
                Console.Write("inst: ");
                if (!instructions[opcode](opcode, arg0, arg1, arg2)) {
                    Console.WriteLine("Illegal execution: 0x{0:x8}", instruction);
                }
            } else { 
                Console.WriteLine("Illegal instruction: 0x{0:x8}", instruction);
            }
            PrintRegisters();
        }

        public void PrintRegisters() {
            Console.SetCursorPosition(0, 0);
            for (int i = 0; i < NUM_REGISTERS; i++) {
                Console.WriteLine("{0}: 0x{1:x8}", register_names[i], registers[i]);
            }
        }

        bool NOP(byte opcode, byte arg0, byte arg1, byte arg2) {
            if (opcode != ((byte)Opcode.NOP) || arg0 != 0 || arg1 != 0 || arg2 != 0) return false;
            Console.WriteLine("nop");
            return true;
        }

        bool HLT(byte opcode, byte arg0, byte arg1, byte arg2) {
            if (opcode != ((byte)Opcode.HLT) || arg0 != 0 || arg1 != 0 || arg2 != 0) return false;
            Console.WriteLine("hlt");
            return true;
        }

        bool SYS(byte opcode, byte arg0, byte arg1, byte arg2) {
            if (opcode != ((byte)Opcode.SYS) || arg0 != 0 || arg1 != 0 || arg2 != 0) return false;
            Console.WriteLine("sys");
            return true;
        }

        bool CALL(byte opcode, byte arg0, byte arg1, byte arg2) {
            if (opcode != ((byte)Opcode.CALL) || arg0 != 0 || arg1 != 0 || arg2 != 0) return false;
            Console.WriteLine("call");
            return true;
        }

        bool RET(byte opcode, byte arg0, byte arg1, byte arg2) {
            if (opcode != ((byte)Opcode.RET) || arg0 != 0 || arg1 != 0 || arg2 != 0) return false;
            Console.WriteLine("ret");
            return true;
        }

        bool LLI(byte opcode, byte arg0, byte arg1, byte arg2) {
            if (opcode != ((byte)Opcode.LLI) || arg0 >= NUM_REGISTERS) return false;
            UInt16 imm;
            if (BitConverter.IsLittleEndian) imm = BitConverter.ToUInt16(new byte[] { arg2, arg1 }, 0);
            else imm = BitConverter.ToUInt16(new byte[] { arg1, arg2 }, 0);
            Console.WriteLine("lli {0}, 0x{1:x4}", register_names[arg0], imm);
            registers[arg0] = imm;
            return true;
        }

        bool LUI(byte opcode, byte arg0, byte arg1, byte arg2) {
            if (opcode != ((byte)Opcode.LUI) || arg0 >= NUM_REGISTERS) return false;
            UInt16 imm;
            if (BitConverter.IsLittleEndian) imm = BitConverter.ToUInt16(new byte[] { arg2, arg1 }, 0);
            else imm = BitConverter.ToUInt16(new byte[] { arg1, arg2 }, 0);
            Console.WriteLine("lui {0}, 0x{1:x4}", register_names[arg0], imm);
            registers[arg0] = (UInt32)((registers[arg0] & 0xFFFF) | (UInt32)(imm << 16));
            return true;
        }

        bool ADD(byte opcode, byte arg0, byte arg1, byte arg2) {
            if (opcode != ((byte)Opcode.ADD) || arg0 >= NUM_REGISTERS || arg1 >= NUM_REGISTERS || arg2 >= NUM_REGISTERS) return false;
            Console.WriteLine("add {0}, {1} (0x{2:x8}), {3} (0x{4:x8})", register_names[arg0], register_names[arg1], registers[arg1], register_names[arg2], registers[arg2]);
            registers[arg0] = registers[arg1] + registers[arg2];
            return true;
        }

        bool SUB(byte opcode, byte arg0, byte arg1, byte arg2) {
            if (opcode != ((byte)Opcode.SUB) || arg0 >= NUM_REGISTERS || arg1 >= NUM_REGISTERS || arg2 >= NUM_REGISTERS) return false;
            Console.WriteLine("sub {0}, {1} (0x{2:x8}), {3} (0x{4:x8})", register_names[arg0], register_names[arg1], registers[arg1], register_names[arg2], registers[arg2]);
            registers[arg0] = registers[arg1] - registers[arg2];
            return true;
        }

        bool MUL(byte opcode, byte arg0, byte arg1, byte arg2) {
            if (opcode != ((byte)Opcode.MUL) || arg0 >= NUM_REGISTERS || arg1 >= NUM_REGISTERS || arg2 >= NUM_REGISTERS) return false;
            Console.WriteLine("mul {0}, {1} (0x{2:x8}), {3} (0x{4:x8})", register_names[arg0], register_names[arg1], registers[arg1], register_names[arg2], registers[arg2]);
            registers[arg0] = registers[arg1] * registers[arg2];
            return true;
        }

        bool DIV(byte opcode, byte arg0, byte arg1, byte arg2) {
            if (opcode != ((byte)Opcode.DIV) || arg0 >= NUM_REGISTERS || arg1 >= NUM_REGISTERS || arg2 >= NUM_REGISTERS) return false;
            Console.WriteLine("add {0}, {1} (0x{2:x8}), {3} (0x{4:x8})", register_names[arg0], register_names[arg1], registers[arg1], register_names[arg2], registers[arg2]);
            registers[arg0] = registers[arg1] / registers[arg2];
            return true;
        }

        bool MOD(byte opcode, byte arg0, byte arg1, byte arg2) {
            if (opcode != ((byte)Opcode.ADD) || arg0 >= NUM_REGISTERS || arg1 >= NUM_REGISTERS || arg2 >= NUM_REGISTERS) return false;
            Console.WriteLine("mod {0}, {1} (0x{2:x8}), {3} (0x{4:x8})", register_names[arg0], register_names[arg1], registers[arg1], register_names[arg2], registers[arg2]);
            registers[arg0] = registers[arg1] % registers[arg2];
            return true;
        }
    }
}