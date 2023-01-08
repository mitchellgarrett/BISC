﻿using System;
using System.Collections.Generic;

namespace FTG.Studios.BISC {

	/// <summary>
	/// BISC virtual machine.
	/// </summary>
	public class VirtualMachine {

		public enum Flags { None = 0x0, SingleStep = 0x1, Debug = 0x2 };

		delegate bool InstructionHandler(byte opcode, byte arg0, byte arg1, byte arg2);
		InstructionHandler[] instructions;

		UInt32[] registers;

		UInt32 pc { get { return registers[0]; } set { registers[0] = value; } }
		UInt32 sp { get { return registers[1]; } set { registers[1] = value; } }
		UInt32 ra { get { return registers[2]; } set { registers[2] = value; } }
		UInt32 rv { get { return registers[3]; } set { registers[3] = value; } }
		UInt32 rt { get { return registers[4]; } set { registers[4] = value; } }

		const UInt32 STACK_SIZE = 256;
		const UInt32 STACK_END = STACK_SIZE;
		const UInt32 STACK_START = STACK_END - STACK_SIZE;

		Dictionary<UInt32, byte> memory;

		Program program;
		public bool IsRunning { get; private set; }
		public Flags Options;

		public VirtualMachine() {
			Initialize();
		}

		public VirtualMachine(Flags options) {
			this.Options = options;
			Initialize();
		}

		void Initialize() {
			registers = new UInt32[Specification.NUM_REGISTERS];
			memory = new Dictionary<UInt32, byte>();
			instructions = new InstructionHandler[] {
				NOP, HLT, SYS, CALL, RET,
					LLI, LUI, MOV,
					LW, LH, LB, SW, SH, SB,
					ADD, SUB, MUL, DIV, MOD,
					NOT, NEG, INV, AND, OR, XOR, BSL, BSR,
					JMP, JEZ, JNZ, JEQ, JNE, JGT, JLT, JGE, JLE
			};
		}

		public void Execute(Program program) {
			this.program = program;
			IsRunning = true;
			
			// Zero out the registers.
			for (int i = 0; i < registers.Length; i++) {
				registers[i] = 0;
			}

			// Zero out the stack.
			for (UInt32 addr = STACK_START; addr < STACK_END; addr++) {
				memory[addr] = 0;
			}

			// Zero other registers.
			pc = 0;
			sp = STACK_END;

			Console.Clear();
			if (Options.HasFlag(Flags.Debug)) {
				PrintRegisters();
				PrintStack();
			}

			while (IsRunning) {
				if (Options.HasFlag(Flags.SingleStep)) {
					if (Options.HasFlag(Flags.Debug)) {
						Console.SetCursorPosition(0, Specification.NUM_REGISTERS + 1);
						Console.Write("Continue execution...");
					}
					Console.ReadKey(true);
				}

				UInt32 instruction = program.Instructions[pc / 4];
				ExecuteInstruction(instruction);
				pc += 4;
				if (pc >= program.Instructions.Length * 4) IsRunning = false;

				if (!Options.HasFlag(Flags.Debug)) {
					//PrintConsole();
				}
			}

			if (Options.HasFlag(Flags.Debug)) {
				Console.SetCursorPosition(0, Specification.NUM_REGISTERS + 1);
				Console.Write("Program complete...");
			}
			Console.ReadKey(true);
			Console.WriteLine();
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

			// Decompose instruction into its byte parameters
			byte[] bytes = BitConverter.GetBytes(instruction);
			if (BitConverter.IsLittleEndian) Array.Reverse(bytes);

			byte opcode = bytes[0];
			byte arg0 = bytes[1];
			byte arg1 = bytes[2];
			byte arg2 = bytes[3];

			if (Options.HasFlag(Flags.Debug)) {
				Console.Clear();
				Console.SetCursorPosition(0, Specification.NUM_REGISTERS);
			}

			if (opcode >= 0 && opcode < instructions.Length) {
				Debug("inst: ");
				if (!instructions[opcode](opcode, arg0, arg1, arg2)) {
					Debug("Illegal execution: 0x{0:x8}", instruction);
				}
			} else {
				Debug("Illegal instruction: 0x{0:x8}", instruction);
			}

			if (Options.HasFlag(Flags.Debug)) {
				PrintRegisters();
				PrintStack();
			}
		}

		void Debug(string format, params object[] args) {
			if (Options.HasFlag(Flags.Debug)) Console.Write(format, args);
		}

		void PrintConsole() {
			char[] line = new char[Console.WindowWidth];
			for (int y = 0; y < Console.WindowHeight; y++) {
				for (int x = 0; x < Console.WindowWidth; x++) {
					if (memory.TryGetValue((UInt32)(y * Console.WindowWidth + x), out byte value)) {

						//if (value != 0) line[x] = (char)value;
						//else line[x] = ' ';
					} else {
						//line[x] = ' ';
					}
				}
				Console.SetCursorPosition(0, y);
				Console.Write(line);
			}
		}

		void PrintRegisters() {
			Console.SetCursorPosition(0, 0);
			for (int i = 0; i < Specification.NUM_REGISTERS; i++) {
				Console.WriteLine("{0}: 0x{1:x8}", Specification.REGISTER_NAMES[i], registers[i]);
			}
		}

		void PrintStack() {
			Console.SetCursorPosition(32, 2);
			for (int y = 0, i = 0; y < STACK_SIZE / 16; y++) {
				Console.SetCursorPosition(26, 2 + y);
				Console.Write("0x{0:x2}:", y * 16);
				for (int x = 0; x < 4; x++, i += 4) {
					UInt32 value = GetMemory32((UInt32) (STACK_START + i));
					Console.SetCursorPosition(32 + x * 9, 2 + y);
					Console.Write("{0:x8}", value);
				}
			}
		}

		bool ValidRegister(byte reg) {
			return reg <= Specification.NUM_REGISTERS;
		}

		byte GetMemory8(UInt32 addr) {
			return memory[addr];
		}

		UInt16 GetMemory16(UInt32 addr) {
			return Specification.AssembleInteger16(memory[addr], memory[addr + 1]);
		}

		UInt32 GetMemory32(UInt32 addr) {
			return Specification.AssembleInteger32(memory[addr], memory[addr + 1], memory[addr + 2], memory[addr + 3]);
		}

		void SetMemory8(UInt32 addr, UInt32 value) {
			memory[addr] = (byte) (value & 0xFF);
			if (addr < Console.WindowWidth * Console.WindowHeight) {
				Console.SetCursorPosition((int)(addr % Console.WindowWidth), (int)(addr / Console.WindowWidth));
				Console.Write((char)value);
			}
		}

		void SetMemory16(UInt32 addr, UInt32 value) {
			byte[] bytes = Specification.DisassembleInteger16((UInt16) value);
			for	(UInt32 index = 0; index < bytes.Length; index++) {
				memory[addr + index] = bytes[index];
			}
		}

		void SetMemory32(UInt32 addr, UInt32 value) {
			byte[] bytes = Specification.DisassembleInteger32(value);
			for	(UInt32 index = 0; index < bytes.Length; index++) {
				memory[addr + index] = bytes[index];
			}
		}

#region Instructions

#region System Instructions
		bool NOP(byte opcode, byte arg0, byte arg1, byte arg2) {
			if (opcode != ((byte)Opcode.NOP) || arg0 != 0 || arg1 != 0 || arg2 != 0) return false;
			Debug("nop");
			return true;
		}

		bool HLT(byte opcode, byte arg0, byte arg1, byte arg2) {
			if (opcode != ((byte)Opcode.HLT) || arg0 != 0 || arg1 != 0 || arg2 != 0) return false;
			Debug("hlt");
			IsRunning = false;
			return true;
		}

		bool SYS(byte opcode, byte arg0, byte arg1, byte arg2) {
			if (opcode != ((byte)Opcode.SYS) || arg0 != 0 || arg1 != 0 || arg2 != 0) return false;
			Debug("sys");
			return true;
		}

		bool CALL(byte opcode, byte arg0, byte arg1, byte arg2) {
			if (opcode != ((byte)Opcode.CALL) || !ValidRegister(arg0) || arg1 != 0 || arg2 != 0) return false;
			Debug("call {0} (0x{1:x8})", Specification.REGISTER_NAMES[arg0], registers[arg0]);

			// Set return address to next instruction
			ra = pc + 4;
			// Jump to called address (subtracted by 4 because VM increments pc by 4)
			pc = registers[arg0] - 4;
			return true;
		}

		bool RET(byte opcode, byte arg0, byte arg1, byte arg2) {
			if (opcode != ((byte)Opcode.RET) || arg0 != 0 || arg1 != 0 || arg2 != 0) return false;
			Debug("ret (0x{0:x8})", ra);

			// Jump to return address (subtracted by 4 because VM increments pc by 4)
			pc = ra - 4;
			return true;
		}
#endregion

#region Load Instructions
		bool LLI(byte opcode, byte arg0, byte arg1, byte arg2) {
			if (opcode != ((byte)Opcode.LLI) || !ValidRegister(arg0)) return false;
			UInt16 imm = Specification.AssembleInteger16(arg1, arg2);
			Debug("lli {0}, 0x{1:x4}", Specification.REGISTER_NAMES[arg0], imm);
			registers[arg0] = imm;
			return true;
		}

		bool LUI(byte opcode, byte arg0, byte arg1, byte arg2) {
			if (opcode != ((byte)Opcode.LUI) || !ValidRegister(arg0)) return false;
			UInt16 imm = Specification.AssembleInteger16(arg1, arg2);
			Debug("lui {0}, 0x{1:x4}", Specification.REGISTER_NAMES[arg0], imm);
			registers[arg0] = (UInt32)((registers[arg0] & 0xFFFF) | (UInt32)(imm << 16));
			return true;
		}

		bool MOV(byte opcode, byte arg0, byte arg1, byte arg2) {
			if (opcode != ((byte)Opcode.MOV) || !ValidRegister(arg0) || !ValidRegister(arg1) || arg2 != 0) return false;
			Debug("mov {0}, {1} (0x{2:x8})", Specification.REGISTER_NAMES[arg0], Specification.REGISTER_NAMES[arg1], registers[arg1]);
			registers[arg0] = registers[arg1];
			return true;
		}
#endregion

#region Memory Instructions
		bool LW(byte opcode, byte arg0, byte arg1, byte arg2) {
			if (opcode != ((byte)Opcode.LW) || !ValidRegister(arg0) || !ValidRegister(arg1)) return false;
			sbyte offset = (sbyte) arg2;
			UInt32 addr = (UInt32) (registers[arg1] + offset);
			UInt32 value = GetMemory32(addr);
			Debug("ld {0}, {1}[{2}] (0x{3:x8}, @0x{4:x8})", Specification.REGISTER_NAMES[arg0], Specification.REGISTER_NAMES[arg1], offset, value, addr);
			registers[arg0] = value;
			return true;
		}

		bool LH(byte opcode, byte arg0, byte arg1, byte arg2) {
			if (opcode != ((byte)Opcode.LH) || !ValidRegister(arg0) || !ValidRegister(arg1)) return false;
			sbyte offset = (sbyte) arg2;
			UInt32 addr = (UInt32) (registers[arg1] + offset);
			UInt16 value = GetMemory16(addr);
			Debug("ld {0}, {1}[{2}] (0x{3:x4}, @0x{4:x8})", Specification.REGISTER_NAMES[arg0], Specification.REGISTER_NAMES[arg1], offset, value, addr);
			registers[arg0] = value;
			return true;
		}

		bool LB(byte opcode, byte arg0, byte arg1, byte arg2) {
			if (opcode != ((byte)Opcode.LB) || !ValidRegister(arg0) || !ValidRegister(arg1)) return false;
			sbyte offset = (sbyte) arg2;
			UInt32 addr = (UInt32) (registers[arg1] + offset);
			byte value = GetMemory8(addr);
			Debug("ld {0}, {1}[{2}] (0x{3:x2}, @0x{4:x8})", Specification.REGISTER_NAMES[arg0], Specification.REGISTER_NAMES[arg1], offset, value, addr);
			registers[arg0] = value;
			return true;
		}

		bool SW(byte opcode, byte arg0, byte arg1, byte arg2) {
			if (opcode != ((byte)Opcode.SW) || !ValidRegister(arg0) || !ValidRegister(arg1)) return false;
			sbyte offset = (sbyte) arg2;
			UInt32 addr = (UInt32) (registers[arg1] + offset);
			Debug("st {0} (0x{1:x8}), {2}[{3}] (@0x{4:x8})", Specification.REGISTER_NAMES[arg0], registers[arg0], Specification.REGISTER_NAMES[arg1], offset, addr);
			SetMemory32(addr, registers[arg0]);
			return true;
		}

		bool SH(byte opcode, byte arg0, byte arg1, byte arg2) {
			if (opcode != ((byte)Opcode.SH) || !ValidRegister(arg0) || !ValidRegister(arg1)) return false;
			sbyte offset = (sbyte) arg2;
			UInt32 addr = (UInt32) (registers[arg1] + offset);
			Debug("sh {0} (0x{1:x4}), {2}[{3}] (@0x{4:x8})", Specification.REGISTER_NAMES[arg0], registers[arg0] & 0xFFFF, Specification.REGISTER_NAMES[arg1], offset, addr);
			SetMemory16(addr, registers[arg0]);
			return true;
		}

		bool SB(byte opcode, byte arg0, byte arg1, byte arg2) {
			if (opcode != ((byte)Opcode.SB) || !ValidRegister(arg0) || !ValidRegister(arg1)) return false;
			sbyte offset = (sbyte) arg2;
			UInt32 addr = (UInt32) (registers[arg1] + offset);
			Debug("sb {0} (0x{1:x2}), {2}[{3}] (@0x{4:x8})", Specification.REGISTER_NAMES[arg0], registers[arg0] & 0xFF, Specification.REGISTER_NAMES[arg1], offset, addr);
			SetMemory8(addr, registers[arg0]);
			return true;
		}
#endregion

#region Arithmetic Instructions
		bool ADD(byte opcode, byte arg0, byte arg1, byte arg2) {
			if (opcode != ((byte)Opcode.ADD) || !ValidRegister(arg0) || !ValidRegister(arg1) || !ValidRegister(arg2)) return false;
			Debug("add {0}, {1} (0x{2:x8}), {3} (0x{4:x8})", Specification.REGISTER_NAMES[arg0], Specification.REGISTER_NAMES[arg1], registers[arg1], Specification.REGISTER_NAMES[arg2], registers[arg2]);
			registers[arg0] = registers[arg1] + registers[arg2];
			return true;
		}

		bool SUB(byte opcode, byte arg0, byte arg1, byte arg2) {
			if (opcode != ((byte)Opcode.SUB) || !ValidRegister(arg0) || !ValidRegister(arg1) || !ValidRegister(arg2)) return false;
			Debug("sub {0}, {1} (0x{2:x8}), {3} (0x{4:x8})", Specification.REGISTER_NAMES[arg0], Specification.REGISTER_NAMES[arg1], registers[arg1], Specification.REGISTER_NAMES[arg2], registers[arg2]);
			registers[arg0] = registers[arg1] - registers[arg2];
			return true;
		}

		bool MUL(byte opcode, byte arg0, byte arg1, byte arg2) {
			if (opcode != ((byte)Opcode.MUL) || !ValidRegister(arg0) || !ValidRegister(arg1) || !ValidRegister(arg2)) return false;
			Debug("mul {0}, {1} (0x{2:x8}), {3} (0x{4:x8})", Specification.REGISTER_NAMES[arg0], Specification.REGISTER_NAMES[arg1], registers[arg1], Specification.REGISTER_NAMES[arg2], registers[arg2]);
			registers[arg0] = registers[arg1] * registers[arg2];
			return true;
		}

		bool DIV(byte opcode, byte arg0, byte arg1, byte arg2) {
			if (opcode != ((byte)Opcode.DIV) || !ValidRegister(arg0) || !ValidRegister(arg1) || !ValidRegister(arg2)) return false;
			Debug("div {0}, {1} (0x{2:x8}), {3} (0x{4:x8})", Specification.REGISTER_NAMES[arg0], Specification.REGISTER_NAMES[arg1], registers[arg1], Specification.REGISTER_NAMES[arg2], registers[arg2]);
			registers[arg0] = registers[arg1] / registers[arg2];
			return true;
		}

		bool MOD(byte opcode, byte arg0, byte arg1, byte arg2) {
			if (opcode != ((byte)Opcode.MOD) || !ValidRegister(arg0) || !ValidRegister(arg1) || !ValidRegister(arg2)) return false;
			Debug("mod {0}, {1} (0x{2:x8}), {3} (0x{4:x8})", Specification.REGISTER_NAMES[arg0], Specification.REGISTER_NAMES[arg1], registers[arg1], Specification.REGISTER_NAMES[arg2], registers[arg2]);
			registers[arg0] = registers[arg1] % registers[arg2];
			return true;
		}
#endregion

#region Negation Instructions
		bool NOT(byte opcode, byte arg0, byte arg1, byte arg2) {
			if (opcode != ((byte)Opcode.NEG) || !ValidRegister(arg0) || !ValidRegister(arg1) || arg2 != 0) return false;
			Debug("not {0}, {1} ({2:x8})", Specification.REGISTER_NAMES[arg0], Specification.REGISTER_NAMES[arg1], registers[arg1]);
			registers[arg0] = registers[arg1] != 0 ? 0u : 1u;
			return true;
		}

		bool NEG(byte opcode, byte arg0, byte arg1, byte arg2) {
			if (opcode != ((byte)Opcode.NEG) || !ValidRegister(arg0) || !ValidRegister(arg1) || arg2 != 0) return false;
			Debug("neg {0}, {1} ({2:x8})", Specification.REGISTER_NAMES[arg0], Specification.REGISTER_NAMES[arg1], registers[arg1]);
			registers[arg0] = (registers[arg1] ^ 0xFFFFFFFF) + 1;
			return true;
		}

		bool INV(byte opcode, byte arg0, byte arg1, byte arg2) {
			if (opcode != ((byte)Opcode.INV) || !ValidRegister(arg0) || !ValidRegister(arg1) || arg2 != 0) return false;
			Debug("inv {0}, {1} ({2:x8})", Specification.REGISTER_NAMES[arg0], Specification.REGISTER_NAMES[arg1], registers[arg1]);
			registers[arg0] = registers[arg1] ^ 0xFFFFFFFF;
			return true;
		}
#endregion

#region Logical Instructions
		bool AND(byte opcode, byte arg0, byte arg1, byte arg2) {
			if (opcode != ((byte)Opcode.AND) || !ValidRegister(arg0) || !ValidRegister(arg1) || !ValidRegister(arg2)) return false;
			Debug("and {0}, {1} (0x{2:x8}), {3} (0x{4:x8})", Specification.REGISTER_NAMES[arg0], Specification.REGISTER_NAMES[arg1], registers[arg1], Specification.REGISTER_NAMES[arg2], registers[arg2]);
			registers[arg0] = registers[arg1] & registers[arg2];
			return true;
		}

		bool OR(byte opcode, byte arg0, byte arg1, byte arg2) {
			if (opcode != ((byte)Opcode.OR) || !ValidRegister(arg0) || !ValidRegister(arg1) || !ValidRegister(arg2)) return false;
			Debug("or {0}, {1} (0x{2:x8}), {3} (0x{4:x8})", Specification.REGISTER_NAMES[arg0], Specification.REGISTER_NAMES[arg1], registers[arg1], Specification.REGISTER_NAMES[arg2], registers[arg2]);
			registers[arg0] = registers[arg1] | registers[arg2];
			return true;
		}

		bool XOR(byte opcode, byte arg0, byte arg1, byte arg2) {
			if (opcode != ((byte)Opcode.XOR) || !ValidRegister(arg0) || !ValidRegister(arg1) || !ValidRegister(arg2)) return false;
			Debug("xor {0}, {1} (0x{2:x8}), {3} (0x{4:x8})", Specification.REGISTER_NAMES[arg0], Specification.REGISTER_NAMES[arg1], registers[arg1], Specification.REGISTER_NAMES[arg2], registers[arg2]);
			registers[arg0] = registers[arg1] ^ registers[arg2];
			return true;
		}

		bool BSL(byte opcode, byte arg0, byte arg1, byte arg2) {
			if (opcode != ((byte)Opcode.BSL) || !ValidRegister(arg0) || !ValidRegister(arg1) || !ValidRegister(arg2)) return false;
			Debug("bsl {0}, {1} (0x{2:x8}), {3} (0x{4:x8})", Specification.REGISTER_NAMES[arg0], Specification.REGISTER_NAMES[arg1], registers[arg1], Specification.REGISTER_NAMES[arg2], registers[arg2]);
			registers[arg0] = registers[arg1] << (Int32) registers[arg2];
			return true;
		}

		bool BSR(byte opcode, byte arg0, byte arg1, byte arg2) {
			if (opcode != ((byte)Opcode.BSR) || !ValidRegister(arg0) || !ValidRegister(arg1) || !ValidRegister(arg2)) return false;
			Debug("bsr {0}, {1} (0x{2:x8}), {3} (0x{4:x8})", Specification.REGISTER_NAMES[arg0], Specification.REGISTER_NAMES[arg1], registers[arg1], Specification.REGISTER_NAMES[arg2], registers[arg2]);
			registers[arg0] = registers[arg1] >> (Int32)registers[arg2];
			return true;
		}
#endregion

#region Jump Instructions
		bool JMP(byte opcode, byte arg0, byte arg1, byte arg2) {
			if (opcode != ((byte)Opcode.JMP) || !ValidRegister(arg0) || arg1 != 0 || arg2 != 0) return false;
			Debug("jmp {0} (0x{1:x8})", Specification.REGISTER_NAMES[arg0], registers[arg0]);
			pc = registers[arg0] - 4;
			return true;
		}

		bool JEZ(byte opcode, byte arg0, byte arg1, byte arg2) {
			if (opcode != ((byte)Opcode.JEZ) || !ValidRegister(arg0) || !ValidRegister(arg1) || arg2 != 0) return false;
			Debug("jz {0} (0x{1:x8}), {2} (3x{1:x8})", Specification.REGISTER_NAMES[arg0], registers[arg0], Specification.REGISTER_NAMES[arg1], registers[arg1]);
			if (registers[arg1] == 0) pc = registers[arg0] - 4;
			return true;
		}

		bool JNZ(byte opcode, byte arg0, byte arg1, byte arg2) {
			if (opcode != ((byte)Opcode.JNZ) || !ValidRegister(arg0) || !ValidRegister(arg1) || arg2 != 0) return false;
			Debug("jnz {0} (0x{1:x8}), {2} (0x{3:x8})", Specification.REGISTER_NAMES[arg0], registers[arg0], Specification.REGISTER_NAMES[arg1], registers[arg1]);
			if (registers[arg1] != 0) pc = registers[arg0] - 4;
			return true;
		}

		bool JEQ(byte opcode, byte arg0, byte arg1, byte arg2) {
			if (opcode != ((byte)Opcode.JEQ) || !ValidRegister(arg0) || !ValidRegister(arg1) || !ValidRegister(arg2)) return false;
			Debug("je {0} (0x{1:x8}), {2} (0x{3:x8}), {4} (0x{5:x8})", Specification.REGISTER_NAMES[arg0], registers[arg0], Specification.REGISTER_NAMES[arg1], registers[arg1], Specification.REGISTER_NAMES[arg2], registers[arg2]);
			if (registers[arg1] == registers[arg2]) pc = registers[arg0] - 4;
			return true;
		}

		bool JNE(byte opcode, byte arg0, byte arg1, byte arg2) {
			if (opcode != ((byte)Opcode.JNE) || !ValidRegister(arg0) || !ValidRegister(arg1) || !ValidRegister(arg2)) return false;
			Debug("jne {0} (0x{1:x8}), {2} (0x{3:x8}), {4} (0x{5:x8})", Specification.REGISTER_NAMES[arg0], registers[arg0], Specification.REGISTER_NAMES[arg1], registers[arg1], Specification.REGISTER_NAMES[arg2], registers[arg2]);
			if (registers[arg1] != registers[arg2]) pc = registers[arg0] - 4;
			return true;
		}

		bool JGT(byte opcode, byte arg0, byte arg1, byte arg2) {
			if (opcode != ((byte)Opcode.JGT) || !ValidRegister(arg0) || !ValidRegister(arg1) || !ValidRegister(arg2)) return false;
			Debug("jgt {0} (0x{1:x8}), {2} (0x{3:x8}), {4} (0x{5:x8})", Specification.REGISTER_NAMES[arg0], registers[arg0], Specification.REGISTER_NAMES[arg1], registers[arg1], Specification.REGISTER_NAMES[arg2], registers[arg2]);
			if (registers[arg1] > registers[arg2]) pc = registers[arg0] - 4;
			return true;
		}

		bool JLT(byte opcode, byte arg0, byte arg1, byte arg2) {
			if (opcode != ((byte)Opcode.JLT) || !ValidRegister(arg0) || !ValidRegister(arg1) || !ValidRegister(arg2)) return false;
			Debug("jlt {0} (0x{1:x8}), {2} (0x{3:x8}), {4} (0x{5:x8})", Specification.REGISTER_NAMES[arg0], registers[arg0], Specification.REGISTER_NAMES[arg1], registers[arg1], Specification.REGISTER_NAMES[arg2], registers[arg2]);
			if (registers[arg1] < registers[arg2]) pc = registers[arg0] - 4;
			return true;
		}

		bool JGE(byte opcode, byte arg0, byte arg1, byte arg2) {
			if (opcode != ((byte)Opcode.JGE) || !ValidRegister(arg0) || !ValidRegister(arg1) || !ValidRegister(arg2)) return false;
			Debug("jge {0} (0x{1:x8}), {2} (0x{3:x8}), {4} (0x{5:x8})", Specification.REGISTER_NAMES[arg0], registers[arg0], Specification.REGISTER_NAMES[arg1], registers[arg1], Specification.REGISTER_NAMES[arg2], registers[arg2]);
			if (registers[arg1] >= registers[arg2]) pc = registers[arg0] - 4;
			return true;
		}

		bool JLE(byte opcode, byte arg0, byte arg1, byte arg2) {
			if (opcode != ((byte)Opcode.JLE) || !ValidRegister(arg0) || !ValidRegister(arg1) || !ValidRegister(arg2)) return false;
			Debug("jle {0} (0x{1:x8}), {2} (0x{3:x8}), {4} (0x{5:x8})", Specification.REGISTER_NAMES[arg0], registers[arg0], Specification.REGISTER_NAMES[arg1], registers[arg1], Specification.REGISTER_NAMES[arg2], registers[arg2]);
			if (registers[arg1] <= registers[arg2]) pc = registers[arg0] - 4;
			return true;
		}
#endregion

#endregion
	}
}
