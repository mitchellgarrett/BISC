using System;
using System.Collections.Generic;
using System.Reflection;

namespace FTG.Studios.BISC {
    
    public enum Opcode : byte { NOP = 0x00, HLT = 0x01, LLI = 0x02, LUI = 0x03, ADD = 0x04 };

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
            instructions = new InstructionHandler[] { NOP, null, LLI, LUI, ADD };
        }

        public void Execute(byte[] instructions) {
            UInt32[] words = new UInt32[instructions.Length / 4];
            for (int i = 0; i < words.Length; i++) {
                UInt32 word = BitConverter.ToUInt32(instructions, i * 4);
                words[i] = word;
            }
            Execute(words);
        }

        public void Execute(UInt32[] instructions) {
            for (int i = 0; i < instructions.Length; i++) {
                ExecuteInstruction(instructions[i]);
            }
        } 

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
    }
}
