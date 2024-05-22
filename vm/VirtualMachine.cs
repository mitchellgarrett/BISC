using System;
using System.Linq;

namespace FTG.Studios.BISC.VM
{

	/// <summary>
	/// BISC virtual machine.
	/// </summary>
	public class VirtualMachine
	{

		delegate bool InstructionHandler(byte arg0, byte arg1, byte arg2);
		InstructionHandler[] instructions;

		RegisterValue[] registers;

		UInt32 pc { get => registers[(int)Register.PC].UValue; set { registers[(int)Register.PC].UValue = value; } }
		UInt32 ra { get => registers[(int)Register.RA].UValue; set { registers[(int)Register.RA].UValue = value; } }
		UInt32 rv { get => registers[(int)Register.RV].UValue; set { registers[(int)Register.RV].UValue = value; } }

		readonly Memory memory;

		public bool IsRunning { get; private set; }

		public VirtualMachine(Memory memory)
		{
			this.memory = memory;
			Initialize();
			Reset();
		}

		void Initialize()
		{
			registers = new RegisterValue[Specification.NUM_REGISTERS];
			instructions = new InstructionHandler[] {
				HLT, NOP, SYS, CALL, RET,
				LLI, LUI, MOV,
				LDW, LDH, LDB, STW, STH, STB,
				ADD, SUB, MUL, MULH, MULHU, DIV, DIVU, MOD, MODU,
				NOT, NEG, INV, AND, OR, XOR, BSL, BSR,
				JMP, JEZ, JNZ, JEQ, JNE,
				JGT, JLT, JGE, JLE,
				JGTU, JLTU, JGEU, JLEU
			};
		}

		public UInt32 GetRegister(int reg)
		{
			if (reg >= 0 && reg < Specification.NUM_REGISTERS) return registers[reg].UValue;
			return 0xFFFFFFFF;
		}

		public UInt32 GetRegister(Register reg)
		{
			return registers[(int)reg].UValue;
		}

		public void SetRegister(int reg, UInt32 val)
		{
			if (reg >= 0 && reg < Specification.NUM_REGISTERS) registers[reg].UValue = val;
		}

		public void SetRegister(Register reg, UInt32 val)
		{
			registers[(int)reg].UValue = val;
		}

		bool IsValidRegister(byte reg)
		{
			return reg <= Specification.NUM_REGISTERS;
		}

		public byte GetMemory8(UInt32 address)
		{
			byte[] data = { 0 };
			memory.Read(address, ref data);

			return data[0];
		}

		public UInt16 GetMemory16(UInt32 address)
		{
			byte[] data = { 0, 0 };
			memory.Read(address, ref data);

			return data.AssembleUInt16();
		}

		public UInt32 GetMemory32(UInt32 address)
		{
			byte[] data = { 0, 0, 0, 0 };
			memory.Read(address, ref data);

			return data.AssembleUInt32();
		}

		public void SetMemory8(UInt32 address, UInt32 value)
		{
			byte[] data = { (byte)(value & 0xFF) };
			memory.Write(address, data);
		}

		public void SetMemory16(UInt32 address, UInt32 value)
		{
			byte[] data = value.DisassembleUInt16();
			memory.Write(address, data);
		}

		public void SetMemory32(UInt32 address, UInt32 value)
		{
			byte[] data = value.DisassembleUInt32();
			memory.Write(address, data);
		}

		public void SetMemoryRange(UInt32 address, byte[] data)
		{
			memory.Write(address, data);
		}

		public void SetMemoryRange(UInt32 address, UInt32[] data)
		{
			byte[] bytes = data.SelectMany(BitConverter.GetBytes).ToArray();
			memory.Write(address, bytes);
		}

		public void Reset()
		{
			// Zero out the registers.
			for (int i = 0; i < registers.Length; i++)
			{
				registers[i].UValue = 0;
			}

			memory.Reset();

			pc = 0;
			IsRunning = true;
		}

		public void Halt()
		{
			IsRunning = false;
		}

		/// <summary>
		/// Executes a single 32-bit BISC instruction at the address stored in PC.
		/// </summary>
		/// <returns>True if the executed instruction is valid, false otherwise.</returns>
		public bool ExecuteNext()
		{
			return ExecuteAt(pc);
		}

		/// <summary>
		/// Executes a single 32-bit BISC instruction at the given address.
		/// </summary>
		/// <param name="address">The memory address the instruction to execute is located.</param>
		/// <returns>True if the instruction at the given address is valid, false otherwise.</returns>
		public bool ExecuteAt(UInt32 address)
		{
			byte[] bytes = new byte[4];
			memory.Read(address, ref bytes);
			return ExecuteInstruction(bytes[0], bytes[1], bytes[2], bytes[3]);
		}

		/// <summary>
		/// Executes 32-bit BISC instructions starting at the given address.
		/// </summary>
		/// <param name="address">The memory addrerss to start execution from.</param>
		/// <returns>The value of the RV register at the end of execution.</returns>
		public UInt32 ExecuteFrom(UInt32 address)
		{
			pc = address;
			while (IsRunning && ExecuteNext()) ;
			return rv;
		}

		/// <summary>
		/// Executes a single 32-bit BISC instruction.
		/// </summary>
		/// <param name="instruction">32-bit instruction.</param>
		/// <returns>True if the given instruction is valid, false otherwise.</returns>
		public bool ExecuteInstruction(UInt32 instruction)
		{
			// Decompose instruction into its byte parameters
			byte[] bytes = instruction.DisassembleUInt32();
			return ExecuteInstruction(bytes[0], bytes[1], bytes[2], bytes[3]);
		}

		/// <summary>
		/// Executes a single 32-bit BISC instruction.
		/// </summary>
		/// <param name="opcode">Opcode of the instruction to execute.</param>
		/// <param name="arg0">First instruction argument.</param>
		/// <param name="arg1">Second instruction argument.</param>
		/// <param name="arg2">Third instruction argument.</param>
		/// <returns>True if the given instruction is valid, false otherwise.</returns>
		public bool ExecuteInstruction(byte opcode, byte arg0, byte arg1, byte arg2)
		{
			if (opcode >= instructions.Length || !instructions[opcode](arg0, arg1, arg2))
			{
				// TODO: Set debug register to illegal execution
				throw new IllegalExecutionException($"Illegal execution: 0x{opcode:x2}{arg0:x2}{arg1:x2}{arg2:x2} at 0x{pc:x8}");
			}

			return true;
		}

		#region Instructions

		#region System Instructions
		bool NOP(byte arg0, byte arg1, byte arg2)
		{
			if (arg0 != 0 || arg1 != 0 || arg2 != 0) return false;

			pc += 4;
			return true;
		}

		bool HLT(byte arg0, byte arg1, byte arg2)
		{
			if (arg0 != 0 || arg1 != 0 || arg2 != 0) return false;

			Halt();
			return true;
		}

		bool SYS(byte arg0, byte arg1, byte arg2)
		{
			if (arg0 != 0 || arg1 != 0 || arg2 != 0) return false;

			// TODO: Handle syscall

			pc += 4;
			return true;
		}

		bool CALL(byte arg0, byte arg1, byte arg2)
		{
			if (!IsValidRegister(arg0) || arg1 != 0 || arg2 != 0) return false;

			// Set return address to next instruction
			ra = pc + 4;
			// Jump to called address
			pc = registers[arg0].UValue;
			return true;
		}

		bool RET(byte arg0, byte arg1, byte arg2)
		{
			if (arg0 != 0 || arg1 != 0 || arg2 != 0) return false;

			// Jump to return address
			pc = ra;
			return true;
		}
		#endregion

		#region Load Instructions
		bool LLI(byte arg0, byte arg1, byte arg2)
		{
			if (!IsValidRegister(arg0)) return false;

			UInt16 imm = (arg1, arg2).AssembleUInt16();
			registers[arg0].UValue = imm;
			pc += 4;
			return true;
		}

		bool LUI(byte arg0, byte arg1, byte arg2)
		{
			if (!IsValidRegister(arg0)) return false;

			UInt16 imm = (arg1, arg2).AssembleUInt16();
			registers[arg0].UValue = (UInt32)((registers[arg0].UValue & 0xFFFF) | (UInt32)(imm << 16));
			pc += 4;
			return true;
		}

		bool MOV(byte arg0, byte arg1, byte arg2)
		{
			if (!IsValidRegister(arg0) || !IsValidRegister(arg1) || arg2 != 0) return false;

			registers[arg0].UValue = registers[arg1].UValue;
			pc += 4;
			return true;
		}
		#endregion

		#region Memory Instructions
		bool LDW(byte arg0, byte arg1, byte arg2)
		{
			if (!IsValidRegister(arg0) || !IsValidRegister(arg1)) return false;

			sbyte offset = (sbyte)arg2;
			UInt32 addr = (UInt32)(registers[arg1].UValue + offset);
			UInt32 value = GetMemory32(addr);
			registers[arg0].UValue = value;
			pc += 4;
			return true;
		}

		bool LDH(byte arg0, byte arg1, byte arg2)
		{
			if (!IsValidRegister(arg0) || !IsValidRegister(arg1)) return false;

			sbyte offset = (sbyte)arg2;
			UInt32 addr = (UInt32)(registers[arg1].UValue + offset);
			UInt16 value = GetMemory16(addr);
			registers[arg0].UValue = value;
			pc += 4;
			return true;
		}

		bool LDB(byte arg0, byte arg1, byte arg2)
		{
			if (!IsValidRegister(arg0) || !IsValidRegister(arg1)) return false;

			sbyte offset = (sbyte)arg2;
			UInt32 addr = (UInt32)(registers[arg1].UValue + offset);
			byte value = GetMemory8(addr);
			registers[arg0].UValue = value;
			pc += 4;
			return true;
		}

		bool STW(byte arg0, byte arg1, byte arg2)
		{
			if (!IsValidRegister(arg0) || !IsValidRegister(arg1)) return false;

			sbyte offset = (sbyte)arg2;
			UInt32 addr = (UInt32)(registers[arg1].UValue + offset);
			SetMemory32(addr, registers[arg0].UValue);
			pc += 4;
			return true;
		}

		bool STH(byte arg0, byte arg1, byte arg2)
		{
			if (!IsValidRegister(arg0) || !IsValidRegister(arg1)) return false;

			sbyte offset = (sbyte)arg2;
			UInt32 addr = (UInt32)(registers[arg1].UValue + offset);
			SetMemory16(addr, registers[arg0].UValue);
			pc += 4;
			return true;
		}

		bool STB(byte arg0, byte arg1, byte arg2)
		{
			if (!IsValidRegister(arg0) || !IsValidRegister(arg1)) return false;

			sbyte offset = (sbyte)arg2;
			UInt32 addr = (UInt32)(registers[arg1].UValue + offset);
			SetMemory8(addr, registers[arg0].UValue);
			pc += 4;
			return true;
		}
		#endregion

		#region Arithmetic Instructions
		bool ADD(byte arg0, byte arg1, byte arg2)
		{
			if (!IsValidRegister(arg0) || !IsValidRegister(arg1) || !IsValidRegister(arg2)) return false;

			registers[arg0].UValue = registers[arg1].UValue + registers[arg2].UValue;
			pc += 4;
			return true;
		}

		bool SUB(byte arg0, byte arg1, byte arg2)
		{
			if (!IsValidRegister(arg0) || !IsValidRegister(arg1) || !IsValidRegister(arg2)) return false;

			registers[arg0].UValue = registers[arg1].UValue - registers[arg2].UValue;
			pc += 4;
			return true;
		}

		bool MUL(byte arg0, byte arg1, byte arg2)
		{
			if (!IsValidRegister(arg0) || !IsValidRegister(arg1) || !IsValidRegister(arg2)) return false;

			registers[arg0].UValue = registers[arg1].UValue * registers[arg2].UValue;
			pc += 4;
			return true;
		}

		bool MULH(byte arg0, byte arg1, byte arg2) {
			if (!IsValidRegister(arg0) || !IsValidRegister(arg1) || !IsValidRegister(arg2)) return false;

			registers[arg0].IValue = (int)(((long)registers[arg1].IValue * (long)registers[arg2].IValue) >> 32);
			pc += 4;
			return true;
		}

		bool MULHU(byte arg0, byte arg1, byte arg2) {
			if (!IsValidRegister(arg0) || !IsValidRegister(arg1) || !IsValidRegister(arg2)) return false;

			registers[arg0].UValue = (registers[arg1].UValue >> 16) * (registers[arg2].UValue >> 16);
			pc += 4;
			return true;
		}

		bool DIV(byte arg0, byte arg1, byte arg2) {
			if (!IsValidRegister(arg0) || !IsValidRegister(arg1) || !IsValidRegister(arg2)) return false;

			registers[arg0].IValue = registers[arg1].IValue / registers[arg2].IValue;
			pc += 4;
			return true;
		}

		bool DIVU(byte arg0, byte arg1, byte arg2) {
			if (!IsValidRegister(arg0) || !IsValidRegister(arg1) || !IsValidRegister(arg2)) return false;

			registers[arg0].UValue = registers[arg1].UValue / registers[arg2].UValue;
			pc += 4;
			return true;
		}

		bool MOD(byte arg0, byte arg1, byte arg2)
		{
			if (!IsValidRegister(arg0) || !IsValidRegister(arg1) || !IsValidRegister(arg2)) return false;

			registers[arg0].IValue = registers[arg1].IValue % registers[arg2].IValue;
			pc += 4;
			return true;
		}

		bool MODU(byte arg0, byte arg1, byte arg2) {
			if (!IsValidRegister(arg0) || !IsValidRegister(arg1) || !IsValidRegister(arg2)) return false;

			registers[arg0].UValue = registers[arg1].UValue % registers[arg2].UValue;
			pc += 4;
			return true;
		}
		#endregion

		#region Negation Instructions
		bool NOT(byte arg0, byte arg1, byte arg2)
		{
			if (!IsValidRegister(arg0) || !IsValidRegister(arg1) || arg2 != 0) return false;

			registers[arg0].UValue = registers[arg1].UValue != 0 ? 0u : 1u;
			pc += 4;
			return true;
		}

		bool NEG(byte arg0, byte arg1, byte arg2)
		{
			if (!IsValidRegister(arg0) || !IsValidRegister(arg1) || arg2 != 0) return false;

			registers[arg0].IValue = registers[arg1].IValue * -1;
			pc += 4;
			return true;
		}

		bool INV(byte arg0, byte arg1, byte arg2)
		{
			if (!IsValidRegister(arg0) || !IsValidRegister(arg1) || arg2 != 0) return false;

			registers[arg0].UValue = registers[arg1].UValue ^ 0xFFFFFFFF;
			pc += 4;
			return true;
		}
		#endregion

		#region Bitwise Instructions
		bool AND(byte arg0, byte arg1, byte arg2)
		{
			if (!IsValidRegister(arg0) || !IsValidRegister(arg1) || !IsValidRegister(arg2)) return false;

			registers[arg0].UValue = registers[arg1].UValue & registers[arg2].UValue;
			pc += 4;
			return true;
		}

		bool OR(byte arg0, byte arg1, byte arg2)
		{
			if (!IsValidRegister(arg0) || !IsValidRegister(arg1) || !IsValidRegister(arg2)) return false;

			registers[arg0].UValue = registers[arg1].UValue | registers[arg2].UValue;
			pc += 4;
			return true;
		}

		bool XOR(byte arg0, byte arg1, byte arg2)
		{
			if (!IsValidRegister(arg0) || !IsValidRegister(arg1) || !IsValidRegister(arg2)) return false;

			registers[arg0].UValue = registers[arg1].UValue ^ registers[arg2].UValue;
			pc += 4;
			return true;
		}

		bool BSL(byte arg0, byte arg1, byte arg2)
		{
			if (!IsValidRegister(arg0) || !IsValidRegister(arg1) || !IsValidRegister(arg2)) return false;

			registers[arg0].UValue = registers[arg1].UValue << registers[arg2].IValue;
			pc += 4;
			return true;
		}

		bool BSR(byte arg0, byte arg1, byte arg2)
		{
			if (!IsValidRegister(arg0) || !IsValidRegister(arg1) || !IsValidRegister(arg2)) return false;

			registers[arg0].UValue = registers[arg1].UValue >> registers[arg2].IValue;
			pc += 4;
			return true;
		}
		#endregion

		#region Jump Instructions
		bool JMP(byte arg0, byte arg1, byte arg2)
		{
			if (!IsValidRegister(arg0) || arg1 != 0 || arg2 != 0) return false;

			pc = registers[arg0].UValue;
			return true;
		}

		bool JEZ(byte arg0, byte arg1, byte arg2)
		{
			if (!IsValidRegister(arg0) || !IsValidRegister(arg1) || arg2 != 0) return false;

			if (registers[arg1].UValue == 0) pc = registers[arg0].UValue;
			else pc += 4;
			return true;
		}

		bool JNZ(byte arg0, byte arg1, byte arg2)
		{
			if (!IsValidRegister(arg0) || !IsValidRegister(arg1) || arg2 != 0) return false;

			if (registers[arg1].UValue != 0) pc = registers[arg0].UValue;
			else pc += 4;
			return true;
		}

		bool JEQ(byte arg0, byte arg1, byte arg2)
		{
			if (!IsValidRegister(arg0) || !IsValidRegister(arg1) || !IsValidRegister(arg2)) return false;

			if (registers[arg1].UValue == registers[arg2].UValue) pc = registers[arg0].UValue;
			else pc += 4;
			return true;
		}

		bool JNE(byte arg0, byte arg1, byte arg2)
		{
			if (!IsValidRegister(arg0) || !IsValidRegister(arg1) || !IsValidRegister(arg2)) return false;

			if (registers[arg1].UValue != registers[arg2].UValue) pc = registers[arg0].UValue;
			else pc += 4;
			return true;
		}

		bool JGT(byte arg0, byte arg1, byte arg2)
		{
			if (!IsValidRegister(arg0) || !IsValidRegister(arg1) || !IsValidRegister(arg2)) return false;

			if (registers[arg1].IValue > registers[arg2].IValue) pc = registers[arg0].UValue;
			else pc += 4;
			return true;
		}

		bool JLT(byte arg0, byte arg1, byte arg2)
		{
			if (!IsValidRegister(arg0) || !IsValidRegister(arg1) || !IsValidRegister(arg2)) return false;

			if (registers[arg1].IValue < registers[arg2].IValue) pc = registers[arg0].UValue;
			else pc += 4;
			return true;
		}

		bool JGE(byte arg0, byte arg1, byte arg2)
		{
			if (!IsValidRegister(arg0) || !IsValidRegister(arg1) || !IsValidRegister(arg2)) return false;

			if (registers[arg1].IValue >= registers[arg2].IValue) pc = registers[arg0].UValue;
			else pc += 4;
			return true;
		}

		bool JLE(byte arg0, byte arg1, byte arg2)
		{
			if (!IsValidRegister(arg0) || !IsValidRegister(arg1) || !IsValidRegister(arg2)) return false;

			if (registers[arg1].IValue <= registers[arg2].IValue) pc = registers[arg0].UValue;
			else pc += 4;
			return true;
		}

		bool JGTU(byte arg0, byte arg1, byte arg2)
		{
			if (!IsValidRegister(arg0) || !IsValidRegister(arg1) || !IsValidRegister(arg2)) return false;

			if (registers[arg1].UValue > registers[arg2].UValue) pc = registers[arg0].UValue;
			else pc += 4;
			return true;
		}

		bool JLTU(byte arg0, byte arg1, byte arg2)
		{
			if (!IsValidRegister(arg0) || !IsValidRegister(arg1) || !IsValidRegister(arg2)) return false;

			if (registers[arg1].UValue < registers[arg2].UValue) pc = registers[arg0].UValue;
			else pc += 4;
			return true;
		}

		bool JGEU(byte arg0, byte arg1, byte arg2)
		{
			if (!IsValidRegister(arg0) || !IsValidRegister(arg1) || !IsValidRegister(arg2)) return false;

			if (registers[arg1].UValue >= registers[arg2].UValue) pc = registers[arg0].UValue;
			else pc += 4;
			return true;
		}

		bool JLEU(byte arg0, byte arg1, byte arg2)
		{
			if (!IsValidRegister(arg0) || !IsValidRegister(arg1) || !IsValidRegister(arg2)) return false;

			if (registers[arg1].UValue <= registers[arg2].UValue) pc = registers[arg0].UValue;
			else pc += 4;
			return true;
		}
		#endregion

		#endregion
	}
}
