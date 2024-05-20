using FTG.Studios.BISC.VM;
using NUnit.Framework;
using System;

namespace FTG.Studios.BISC.Test {

	[TestFixture]
	public class VirtualMachineTest {

		VirtualMachine vm;
		VolatileMemory memory;
		UInt32 address_start, address_end, address_length;

		UInt32[] GetRegisterState() {
			UInt32[] regs = new UInt32[Specification.NUM_REGISTERS];
			for (int r = 0; r < regs.Length; r++) {
				regs[r] = vm.GetRegister(r);
			}
			return regs;
		}

		void CompareRegisterStates(UInt32[] expected, UInt32[] actual) {
			Assert.AreEqual(expected.Length, actual.Length);
			for (int i = 0; i < expected.Length; i++) {
				Assert.AreEqual(expected[i], actual[i]);
			}
		}

		[SetUp]
		public void SetUp() {
			address_start = 0x1000;
			address_length = 0x1000;
			address_end = address_start + address_length;
			memory = new VolatileMemory(address_start, address_length);
			vm = new VirtualMachine(memory);
			Assert.NotNull(vm);
		}

		#region Virtual Machine Functions
		[Test]
		public void Reset() {
			vm.Reset();
			Assert.IsTrue(vm.IsRunning);
		}

		[Test]
		public void Halt() {
			vm.Reset();
			vm.Halt();
			Assert.IsFalse(vm.IsRunning);
			Assert.AreEqual(0, vm.GetRegister(Register.PC));
		}

		[Test]
		public void GetRegister() {
			for (int reg = 0; reg < Specification.NUM_REGISTERS; reg++) {
				Assert.AreEqual(0, vm.GetRegister(reg));
			}
		}

		[Test]
		public void SetRegister() {
			UInt32 expected = 0xbabecafe;
			for (int reg = 0; reg < Specification.NUM_REGISTERS; reg++) {
					vm.SetRegister(reg, expected);
					Assert.AreEqual(expected, vm.GetRegister(reg));
			}
		}

		[Test]
		public void SetMemory8() {
			byte expected = 0xfe;
			memory.Write(address_start, new byte[] { expected });

			UInt32 value = vm.GetMemory8(address_start);
			Assert.AreEqual(expected, (byte)(value & 0xff));
		}

		[Test]
		public void SetMemory16() {
			UInt16 expected = 0xcafe;
			byte[] expected_bytes = expected.DisassembleUInt16();
			memory.Write(address_start, expected_bytes);

			UInt32 value = vm.GetMemory16(address_start);
			Assert.AreEqual(expected, (UInt16)(value & 0xffff));
		}

		[Test]
		public void SetMemory32() {
			UInt32 expected = 0xbabecafe;
			byte[] expected_bytes = expected.DisassembleUInt32();
			memory.Write(address_start, expected_bytes);

			UInt32 value = vm.GetMemory32(address_start);
			Assert.AreEqual(expected, value);
		}

		[Test]
		public void Execute_ValidInstruction() {
			// NOP
			UInt32 instruction = ((byte)Opcode.NOP, (byte)0, (byte)0, (byte)0).AssembleUInt32();
			UInt32 pc = vm.GetRegister(Register.PC);
			Assert.IsTrue(vm.ExecuteInstruction(instruction));

			// PC should be incremented by 4
			Assert.AreEqual(pc + 4, vm.GetRegister(Register.PC));
		}

		[Test]
		public void Execute_InvalidInstruction() {
			// Invalid instruction
			UInt32 instruction = 0xffffffff;
			UInt32 pc = vm.GetRegister(Register.PC);

			Assert.Throws<IllegalExecutionException>(() => vm.ExecuteInstruction(instruction));
		}
		#endregion

		#region System Instructions
		[Test]
		public void NOP() {
			UInt32 instruction = ((byte)Opcode.NOP, (byte)0, (byte)0, (byte)0).AssembleUInt32();
			UInt32[] expected_state = GetRegisterState();
			Assert.IsTrue(vm.ExecuteInstruction(instruction));
			UInt32[] actual_state = GetRegisterState();

			// PC should be incremented by 4
			expected_state[(int)Register.PC] += 4;

			// Check registers match previous register state
			Assert.AreEqual(expected_state, actual_state);

			Assert.IsTrue(vm.IsRunning);
		}

		[Test]
		public void HLT() {
			UInt32 instruction = ((byte)Opcode.HLT, (byte)0, (byte)0, (byte)0).AssembleUInt32();
			UInt32[] expected_state = GetRegisterState();
			Assert.IsTrue(vm.ExecuteInstruction(instruction));
			UInt32[] actual_state = GetRegisterState();

			// Check registers match previous register state, PC does not increment
			Assert.AreEqual(expected_state, actual_state);

			Assert.IsFalse(vm.IsRunning);
		}

		[Test]
		public void SYS() {
			UInt32 instruction = ((byte)Opcode.SYS, (byte)0, (byte)0, (byte)0).AssembleUInt32();
			UInt32[] expected_state = GetRegisterState();
			Assert.IsTrue(vm.ExecuteInstruction(instruction));
			UInt32[] actual_state = GetRegisterState();

			// PC should be incremented by 4
			expected_state[(int)Register.PC] += 4;

			// Check registers match previous register state
			Assert.AreEqual(expected_state, actual_state);
		}

		[Test]
		public void CALL() {
			UInt32 call_address = 0xbabecafe;
			Register call_register = Register.R0;

			vm.SetRegister(call_register, call_address);

			UInt32 instruction = ((byte)Opcode.CALL, (byte)call_register, (byte)0, (byte)0).AssembleUInt32();
			UInt32[] expected_state = GetRegisterState();
			Assert.IsTrue(vm.ExecuteInstruction(instruction));
			UInt32[] actual_state = GetRegisterState();

			// RA should be PC + 4
			expected_state[(int)Register.RA] = expected_state[(int)Register.PC] + 4;

			// PC should be the call address
			expected_state[(int)Register.PC] = call_address;

			// Check registers match previous register state
			Assert.AreEqual(expected_state, actual_state);
		}

		[Test]
		public void RET() {
			UInt32 ret_address = 0xbabecafe;

			vm.SetRegister(Register.RA, ret_address);

			UInt32 instruction = ((byte)Opcode.RET, (byte)0, (byte)0, (byte)0).AssembleUInt32();
			UInt32[] expected_state = GetRegisterState();
			Assert.IsTrue(vm.ExecuteInstruction(instruction));
			UInt32[] actual_state = GetRegisterState();

			// PC should be RA
			expected_state[(int)Register.PC] = ret_address;

			// Check registers match previous register state
			Assert.AreEqual(expected_state, actual_state);
		}
		#endregion

		#region Load Instructions
		[Test]
		public void LLI() {
			UInt16 immediate = 0xcafe;
			byte[] immediate_bytes = immediate.DisassembleUInt16();
			Register register = Register.R0;

			UInt32 instruction = ((byte)Opcode.LLI, (byte)register, immediate_bytes[0], immediate_bytes[1]).AssembleUInt32();
			UInt32[] expected_state = GetRegisterState();
			Assert.IsTrue(vm.ExecuteInstruction(instruction));
			UInt32[] actual_state = GetRegisterState();

			// PC should be incremented by 4
			expected_state[(int)Register.PC] += 4;

			// Reg should be value of immediate
			expected_state[(int)register] = immediate;

			// Check registers match previous register state
			Assert.AreEqual(expected_state, actual_state);
		}

		[Test]
		public void LUI() {
			UInt16 immediate = 0xbabe;
			byte[] immediate_bytes = immediate.DisassembleUInt16();
			Register register = Register.R0;

			UInt32 instruction = ((byte)Opcode.LUI, (byte)register, immediate_bytes[0], immediate_bytes[1]).AssembleUInt32();
			UInt32[] expected_state = GetRegisterState();
			Assert.IsTrue(vm.ExecuteInstruction(instruction));
			UInt32[] actual_state = GetRegisterState();

			// PC should be incremented by 4
			expected_state[(int)Register.PC] += 4;

			// Reg should be value of immediate
			expected_state[(int)register] = (UInt32)(immediate << 16);

			// Check registers match previous register state
			Assert.AreEqual(expected_state, actual_state);
		}

		[Test]
		public void MOV() {
			Register destination_register = Register.R0;
			Register source_rergister = Register.R1;

			UInt32 expected = 0xbabecafe;
			vm.SetRegister(source_rergister, expected);

			UInt32 instruction = ((byte)Opcode.MOV, (byte)destination_register, (byte)source_rergister, (byte)0).AssembleUInt32();
			UInt32[] expected_state = GetRegisterState();
			Assert.IsTrue(vm.ExecuteInstruction(instruction));
			UInt32[] actual_state = GetRegisterState();

			// PC should be incremented by 4
			expected_state[(int)Register.PC] += 4;

			// Reg0 should be value of moved reg1
			expected_state[(int)destination_register] = expected_state[(int)source_rergister];
			Assert.AreEqual(expected, expected_state[(int)destination_register]);

			// Check registers match previous register state
			Assert.AreEqual(expected_state, actual_state);
		}
		#endregion

		#region Memory Instructions
		[Test]
		public void LDW() {
			Register source_register = Register.R0;
			Register destination_register = Register.R1;
			byte offset = 127;

			vm.SetRegister(source_register, address_start);

			UInt32 expected = 0xbabecafe;
			UInt32 address = (UInt32)(vm.GetRegister(source_register) + (sbyte)offset);
			vm.SetMemory32(address, expected);

			UInt32 instruction = ((byte)Opcode.LDW, (byte)destination_register, (byte)source_register, offset).AssembleUInt32();
			UInt32[] expected_state = GetRegisterState();
			Assert.IsTrue(vm.ExecuteInstruction(instruction));
			UInt32[] actual_state = GetRegisterState();

			// PC should be incremented by 4
			expected_state[(int)Register.PC] += 4;

			// Dst should have expected value
			expected_state[(int)destination_register] = expected;

			// Check registers match previous register state
			Assert.AreEqual(expected_state, actual_state);
		}

		[Test]
		public void LDH() {
			Register source_register = Register.R0;
			Register destination_register = Register.R1;
			byte offset = 127;

			vm.SetRegister(source_register, address_start);

			UInt32 value = 0xbabecafe;
			byte[] value_bytes = value.DisassembleUInt32();
			UInt32 expected = (value_bytes[0], value_bytes[1]).AssembleUInt16();
			UInt32 address = (UInt32)(vm.GetRegister(source_register) + (sbyte)offset);
			vm.SetMemory32(address, value);

			UInt32 instruction = ((byte)Opcode.LDH, (byte)destination_register, (byte)source_register, offset).AssembleUInt32();
			UInt32[] expected_state = GetRegisterState();
			Assert.IsTrue(vm.ExecuteInstruction(instruction));
			UInt32[] actual_state = GetRegisterState();

			// PC should be incremented by 4
			expected_state[(int)Register.PC] += 4;

			// Dst should have expected value
			expected_state[(int)destination_register] = expected;

			// Check registers match previous register state
			Assert.AreEqual(expected_state, actual_state);
		}

		[Test]
		public void LDB() {
			Register source_register = Register.R0;
			Register destination_register = Register.R1;
			byte offset = 127;

			vm.SetRegister(source_register, address_start);

			UInt32 value = 0xbabecafe;
			byte[] value_bytes = value.DisassembleUInt32();
			UInt32 expected = value_bytes[0];
			UInt32 address = (UInt32)(vm.GetRegister(source_register) + (sbyte)offset);
			vm.SetMemory32(address, value);

			UInt32 instruction = ((byte)Opcode.LDB, (byte)destination_register, (byte)source_register, offset).AssembleUInt32();
			UInt32[] expected_state = GetRegisterState();
			Assert.IsTrue(vm.ExecuteInstruction(instruction));
			UInt32[] actual_state = GetRegisterState();

			// PC should be incremented by 4
			expected_state[(int)Register.PC] += 4;

			// Dst should have expected value
			expected_state[(int)destination_register] = expected;

			// Check registers match previous register state
			Assert.AreEqual(expected_state, actual_state);
		}

		[Test]
		public void STW() {
			Register source_register = Register.R0;
			Register destination_register = Register.R1;
			byte offset = 127;

			UInt32 expected = 0xbabecafe;

			vm.SetRegister(destination_register, address_start);
			vm.SetRegister(source_register, expected);

			UInt32 instruction = ((byte)Opcode.STW, (byte)source_register, (byte)destination_register, offset).AssembleUInt32();
			UInt32[] expected_state = GetRegisterState();
			Assert.IsTrue(vm.ExecuteInstruction(instruction));
			UInt32[] actual_state = GetRegisterState();

			// PC should be incremented by 4
			expected_state[(int)Register.PC] += 4;

			// Check registers match previous register state
			Assert.AreEqual(expected_state, actual_state);

			// Value at memory address should match src
			UInt32 address = (UInt32)(vm.GetRegister(destination_register) + (sbyte)offset);
			Assert.AreEqual(expected, vm.GetMemory32(address));
		}

		[Test]
		public void STH() {
			Register source_register = Register.R0;
			Register destination_register = Register.R1;
			byte offset = 127;

			UInt32 value = 0xbabecafe;
			byte[] value_bytes = value.DisassembleUInt32();
			UInt32 expected = (value_bytes[0], value_bytes[1]).AssembleUInt16();

			vm.SetRegister(destination_register, address_start);
			vm.SetRegister(source_register, value);

			UInt32 instruction = ((byte)Opcode.STH, (byte)source_register, (byte)destination_register, offset).AssembleUInt32();
			UInt32[] expected_state = GetRegisterState();
			Assert.IsTrue(vm.ExecuteInstruction(instruction));
			UInt32[] actual_state = GetRegisterState();

			// PC should be incremented by 4
			expected_state[(int)Register.PC] += 4;

			// Check registers match previous register state
			Assert.AreEqual(expected_state, actual_state);

			// Value at memory address should match src
			UInt32 address = (UInt32)(vm.GetRegister(destination_register) + (sbyte)offset);
			Assert.AreEqual(expected, vm.GetMemory32(address));
		}

		[Test]
		public void STB() {
			Register source_register = Register.R0;
			Register destination_register = Register.R1;
			byte offset = 127;

			UInt32 value = 0xbabecafe;
			byte[] value_bytes = value.DisassembleUInt32();
			UInt32 expected = value_bytes[0];

			vm.SetRegister(destination_register, address_start);
			vm.SetRegister(source_register, expected);

			UInt32 instruction = ((byte)Opcode.STB, (byte)source_register, (byte)destination_register, offset).AssembleUInt32();
			UInt32[] expected_state = GetRegisterState();
			Assert.IsTrue(vm.ExecuteInstruction(instruction));
			UInt32[] actual_state = GetRegisterState();

			// PC should be incremented by 4
			expected_state[(int)Register.PC] += 4;

			// Check registers match previous register state
			Assert.AreEqual(expected_state, actual_state);

			// Value at memory address should match src
			UInt32 addr = (UInt32)(vm.GetRegister(destination_register) + (sbyte)offset);
			Assert.AreEqual(expected, vm.GetMemory32(addr));
		}
		#endregion

		#region Arithmetic Instructions
		[Test]
		public void ADD() {
			Register source_register_a = Register.R0;
			Register source_register_b = Register.R1;
			Register destination_register = Register.R2;

			UInt32 value_a = 0xbabe;
			UInt32 value_b = 0xcafe;
			UInt32 expected = value_a + value_b;

			vm.SetRegister(source_register_a, value_a);
			vm.SetRegister(source_register_b, value_b);

			UInt32 instruction = ((byte)Opcode.ADD, (byte)destination_register, (byte)source_register_a, (byte)source_register_b).AssembleUInt32();
			UInt32[] expected_state = GetRegisterState();
			Assert.IsTrue(vm.ExecuteInstruction(instruction));
			UInt32[] actual_state = GetRegisterState();

			// PC should be incremented by 4
			expected_state[(int)Register.PC] += 4;

			// Dst should be expected value
			expected_state[(int)destination_register] = expected;

			// Check registers match previous register state
			Assert.AreEqual(expected_state, actual_state);
		}

		[Test]
		public void SUB() {
			Register source_register_a = Register.R0;
			Register source_register_b = Register.R1;
			Register destination_register = Register.R2;

			UInt32 value_a = 0xbabe;
			UInt32 value_b = 0xcafe;
			UInt32 expected = value_a - value_b;

			vm.SetRegister(source_register_a, value_a);
			vm.SetRegister(source_register_b, value_b);

			UInt32 instruction = ((byte)Opcode.SUB, (byte)destination_register, (byte)source_register_a, (byte)source_register_b).AssembleUInt32();
			UInt32[] expected_state = GetRegisterState();
			Assert.IsTrue(vm.ExecuteInstruction(instruction));
			UInt32[] actual_state = GetRegisterState();

			// PC should be incremented by 4
			expected_state[(int)Register.PC] += 4;

			// Dst should be expected value
			expected_state[(int)destination_register] = expected;

			// Check registers match previous register state
			Assert.AreEqual(expected_state, actual_state);
		}

		[Test]
		public void MUL() {
			Register source_register_a = Register.R0;
			Register source_register_b = Register.R1;
			Register destination_rregister = Register.R2;

			UInt32 value_a = 0xbabecafe;
			UInt32 value_b = 0xcafebabe;
			UInt32 expected = 0x0D1B3800;

			vm.SetRegister(source_register_a, value_a);
			vm.SetRegister(source_register_b, value_b);

			UInt32 instruction = ((byte)Opcode.MUL, (byte)destination_rregister, (byte)source_register_a, (byte)source_register_b).AssembleUInt32();
			UInt32[] expected_state = GetRegisterState();
			Assert.IsTrue(vm.ExecuteInstruction(instruction));
			UInt32[] actual_state = GetRegisterState();

			// PC should be incremented by 4
			expected_state[(int)Register.PC] += 4;

			// Dst should be expected value
			expected_state[(int)destination_rregister] = expected;

			// Check registers match previous register state
			Assert.AreEqual(expected_state, actual_state);
		}

		[Test]
		public void MULH() {
			Register source_register_a = Register.R0;
			Register source_register_b = Register.R1;
			Register destination_rregister = Register.R2;

			UInt32 value_a = 0xbabecafe;
			UInt32 value_b = 0xcafebabe;
			UInt32 expected = 0x94145DB3;

			vm.SetRegister(source_register_a, value_a);
			vm.SetRegister(source_register_b, value_b);

			UInt32 instruction = ((byte)Opcode.MULH, (byte)destination_rregister, (byte)source_register_a, (byte)source_register_b).AssembleUInt32();
			UInt32[] expected_state = GetRegisterState();
			Assert.IsTrue(vm.ExecuteInstruction(instruction));
			UInt32[] actual_state = GetRegisterState();

			// PC should be incremented by 4
			expected_state[(int)Register.PC] += 4;

			// Dst should be expected value
			expected_state[(int)destination_rregister] = expected;

			// Check registers match previous register state
			Assert.AreEqual(expected_state, actual_state);
		}

		[Test]
		public void MULHU() {
			Register source_register_a = Register.R0;
			Register source_register_b = Register.R1;
			Register destination_rregister = Register.R2;

			UInt32 value_a = 0xbabecafe;
			UInt32 value_b = 0xcafebabe;
			UInt32 expected = 0x94145DB3;

			vm.SetRegister(source_register_a, value_a);
			vm.SetRegister(source_register_b, value_b);

			UInt32 instruction = ((byte)Opcode.MULHU, (byte)destination_rregister, (byte)source_register_a, (byte)source_register_b).AssembleUInt32();
			UInt32[] expected_state = GetRegisterState();
			Assert.IsTrue(vm.ExecuteInstruction(instruction));
			UInt32[] actual_state = GetRegisterState();

			// PC should be incremented by 4
			expected_state[(int)Register.PC] += 4;

			// Dst should be expected value
			expected_state[(int)destination_rregister] = expected;

			// Check registers match previous register state
			Assert.AreEqual(expected_state, actual_state);
		}

		[Test]
		public void DIV() {
			Register source_register_a = Register.R0;
			Register source_register_b = Register.R1;
			Register destination_register = Register.R2;

			int value_a = 0xbabe;
			int value_b = 0xcafe;
			int expected = value_a / value_b;

			vm.SetRegister(source_register_a, (UInt32)value_a);
			vm.SetRegister(source_register_b, (UInt32)value_b);

			UInt32 instruction = ((byte)Opcode.DIV, (byte)destination_register, (byte)source_register_a, (byte)source_register_b).AssembleUInt32();
			UInt32[] expected_state = GetRegisterState();
			Assert.IsTrue(vm.ExecuteInstruction(instruction));
			UInt32[] actual_state = GetRegisterState();

			// PC should be incremented by 4
			expected_state[(int)Register.PC] += 4;

			// Dst should be expected value
			expected_state[(int)destination_register] = (UInt32)expected;

			// Check registers match previous register state
			Assert.AreEqual(expected_state, actual_state);
		}

		[Test]
		public void DIVU() {
			Register source_register_a = Register.R0;
			Register source_register_b = Register.R1;
			Register destination_register = Register.R2;

			int value_a = 0xbabe;
			int value_b = 0xcafe;
			int expected = value_a / value_b;

			vm.SetRegister(source_register_a, (UInt32)value_a);
			vm.SetRegister(source_register_b, (UInt32)value_b);

			UInt32 instruction = ((byte)Opcode.DIVU, (byte)destination_register, (byte)source_register_a, (byte)source_register_b).AssembleUInt32();
			UInt32[] expected_state = GetRegisterState();
			Assert.IsTrue(vm.ExecuteInstruction(instruction));
			UInt32[] actual_state = GetRegisterState();

			// PC should be incremented by 4
			expected_state[(int)Register.PC] += 4;

			// Dst should be expected value
			expected_state[(int)destination_register] = (UInt32)expected;

			// Check registers match previous register state
			Assert.AreEqual(expected_state, actual_state);
		}

		[Test]
		public void MOD() {
			Register source_register_a = Register.R0;
			Register source_register_b = Register.R1;
			Register destination_register = Register.R2;

			int value_a = 0xbabe;
			int value_b = 0xcafe;
			int expected = value_a % value_b;

			vm.SetRegister(source_register_a, (UInt32)value_a);
			vm.SetRegister(source_register_b, (UInt32)value_b);

			UInt32 instruction = ((byte)Opcode.MOD, (byte)destination_register, (byte)source_register_a, (byte)source_register_b).AssembleUInt32();
			UInt32[] expected_state = GetRegisterState();
			Assert.IsTrue(vm.ExecuteInstruction(instruction));
			UInt32[] actual_state = GetRegisterState();

			// PC should be incremented by 4
			expected_state[(int)Register.PC] += 4;

			// Dst should be expected value
			expected_state[(int)destination_register] = (UInt32)expected;

			// Check registers match previous register state
			Assert.AreEqual(expected_state, actual_state);
		}

		[Test]
		public void MODU() {
			Register source_register_a = Register.R0;
			Register source_register_b = Register.R1;
			Register destination_register = Register.R2;

			int value_a = 0xbabe;
			int value_b = 0xcafe;
			int expected = value_a % value_b;

			vm.SetRegister(source_register_a, (UInt32)value_a);
			vm.SetRegister(source_register_b, (UInt32)value_b);

			UInt32 instruction = ((byte)Opcode.MODU, (byte)destination_register, (byte)source_register_a, (byte)source_register_b).AssembleUInt32();
			UInt32[] expected_state = GetRegisterState();
			Assert.IsTrue(vm.ExecuteInstruction(instruction));
			UInt32[] actual_state = GetRegisterState();

			// PC should be incremented by 4
			expected_state[(int)Register.PC] += 4;

			// Dst should be expected value
			expected_state[(int)destination_register] = (UInt32)expected;

			// Check registers match previous register state
			Assert.AreEqual(expected_state, actual_state);
		}
		#endregion

		#region Negation Instructions
		[Test]
		public void NOT_0_Returns_1() {
			Register source_register = Register.R0;
			Register destination_register = Register.R1;

			UInt32 value = 0;
			UInt32 expected = 1;

			vm.SetRegister(source_register, value);

			UInt32 instruction = ((byte)Opcode.NOT, (byte)destination_register, (byte)source_register, (byte)0).AssembleUInt32();
			UInt32[] expected_state = GetRegisterState();
			Assert.IsTrue(vm.ExecuteInstruction(instruction));
			UInt32[] actual_state = GetRegisterState();

			// PC should be incremented by 4
			expected_state[(int)Register.PC] += 4;

			// Dst should be expected value
			expected_state[(int)destination_register] = expected;

			// Check registers match previous register state
			Assert.AreEqual(expected_state, actual_state);
		}

		[Test]
		public void NOT_1_Returns_0() {
			Register source_register = Register.R0;
			Register destination_register = Register.R1;

			UInt32 value = 1;
			UInt32 expected = 0;

			vm.SetRegister(source_register, value);

			UInt32 instruction = ((byte)Opcode.NOT, (byte)destination_register, (byte)source_register, (byte)0).AssembleUInt32();
			UInt32[] expected_state = GetRegisterState();
			Assert.IsTrue(vm.ExecuteInstruction(instruction));
			UInt32[] actual_state = GetRegisterState();

			// PC should be incremented by 4
			expected_state[(int)Register.PC] += 4;

			// Dst should be expected value
			expected_state[(int)destination_register] = expected;

			// Check registers match previous register state
			Assert.AreEqual(expected_state, actual_state);
		}

		[Test]
		public void NOT_Positive_Returns_0() {
			Register source_register = Register.R0;
			Register destination_register = Register.R1;

			UInt32 value = 53487;
			UInt32 expected = 0;

			vm.SetRegister(source_register, value);

			UInt32 instruction = ((byte)Opcode.NOT, (byte)destination_register, (byte)source_register, (byte)0).AssembleUInt32();
			UInt32[] expected_state = GetRegisterState();
			Assert.IsTrue(vm.ExecuteInstruction(instruction));
			UInt32[] actual_state = GetRegisterState();

			// PC should be incremented by 4
			expected_state[(int)Register.PC] += 4;

			// Dst should be expected value
			expected_state[(int)destination_register] = expected;

			// Check registers match previous register state
			Assert.AreEqual(expected_state, actual_state);
		}

		[Test]
		public void NOT_Negative_Returns_0() {
			Register source_register = Register.R0;
			Register destination_register = Register.R1;

			UInt32 value = 0xFFFFFFFF;
			UInt32 expected = 0;

			vm.SetRegister(source_register, value);

			UInt32 instruction = ((byte)Opcode.NOT, (byte)destination_register, (byte)source_register, (byte)0).AssembleUInt32();
			UInt32[] expected_state = GetRegisterState();
			Assert.IsTrue(vm.ExecuteInstruction(instruction));
			UInt32[] actual_state = GetRegisterState();

			// PC should be incremented by 4
			expected_state[(int)Register.PC] += 4;

			// Dst should be expected value
			expected_state[(int)destination_register] = expected;

			// Check registers match previous register state
			Assert.AreEqual(expected_state, actual_state);
		}

		[Test]
		public void NEG_0_Returns_0() {
			Register source_register = Register.R0;
			Register destination_register = Register.R1;

			UInt32 value = 0;
			UInt32 expected = 0;

			vm.SetRegister(source_register, value);

			UInt32 instruction = ((byte)Opcode.NEG, (byte)destination_register, (byte)source_register, (byte)0).AssembleUInt32();
			UInt32[] expected_state = GetRegisterState();
			Assert.IsTrue(vm.ExecuteInstruction(instruction));
			UInt32[] actual_state = GetRegisterState();

			// PC should be incremented by 4
			expected_state[(int)Register.PC] += 4;

			// Dst should be expected value
			expected_state[(int)destination_register] = expected;

			// Check registers match previous register state
			Assert.AreEqual(expected_state, actual_state);
		}

		[Test]
		public void NEG_1_Returns_M1() {
			Register source_register = Register.R0;
			Register destination_register = Register.R1;

			UInt32 value = 1;
			UInt32 expected = 0xFFFFFFFF;

			vm.SetRegister(source_register, value);

			UInt32 instruction = ((byte)Opcode.NEG, (byte)destination_register, (byte)source_register, (byte)0).AssembleUInt32();
			UInt32[] expected_state = GetRegisterState();
			Assert.IsTrue(vm.ExecuteInstruction(instruction));
			UInt32[] actual_state = GetRegisterState();

			// PC should be incremented by 4
			expected_state[(int)Register.PC] += 4;

			// Dst should be expected value
			expected_state[(int)destination_register] = expected;

			// Check registers match previous register state
			Assert.AreEqual(expected_state, actual_state);
		}

		[Test]
		public void NEG_M1_Returns_1() {
			Register source_register = Register.R0;
			Register destination_register = Register.R1;

			UInt32 value = 0xFFFFFFFF;
			UInt32 expected = 1;

			vm.SetRegister(source_register, value);

			UInt32 instruction = ((byte)Opcode.NEG, (byte)destination_register, (byte)source_register, (byte)0).AssembleUInt32();
			UInt32[] expected_state = GetRegisterState();
			Assert.IsTrue(vm.ExecuteInstruction(instruction));
			UInt32[] actual_state = GetRegisterState();

			// PC should be incremented by 4
			expected_state[(int)Register.PC] += 4;

			// Dst should be expected value
			expected_state[(int)destination_register] = expected;

			// Check registers match previous register state
			Assert.AreEqual(expected_state, actual_state);
		}

		[Test]
		public void NEG_Positive_Returns_Negative() {
			Register source_register = Register.R0;
			Register destination_register = Register.R1;

			UInt32 value = 47895;
			UInt32 expected = (value ^ 0xFFFFFFFF) + 1;

			vm.SetRegister(source_register, value);

			UInt32 instruction = ((byte)Opcode.NEG, (byte)destination_register, (byte)source_register, (byte)0).AssembleUInt32();
			UInt32[] expected_state = GetRegisterState();
			Assert.IsTrue(vm.ExecuteInstruction(instruction));
			UInt32[] actual_state = GetRegisterState();

			// PC should be incremented by 4
			expected_state[(int)Register.PC] += 4;

			// Dst should be expected value
			expected_state[(int)destination_register] = expected;

			// Check registers match previous register state
			Assert.AreEqual(expected_state, actual_state);
		}

		[Test]
		public void NEG_Negative_Returns_Positive() {
			Register source_register = Register.R0;
			Register destination_register = Register.R1;

			UInt32 value = 0xFF479283;
			UInt32 expected = (value ^ 0xFFFFFFFF) + 1;

			vm.SetRegister(source_register, value);

			UInt32 instruction = ((byte)Opcode.NEG, (byte)destination_register, (byte)source_register, (byte)0).AssembleUInt32();
			UInt32[] expected_state = GetRegisterState();
			Assert.IsTrue(vm.ExecuteInstruction(instruction));
			UInt32[] actual_state = GetRegisterState();

			// PC should be incremented by 4
			expected_state[(int)Register.PC] += 4;

			// Dst should be expected value
			expected_state[(int)destination_register] = expected;

			// Check registers match previous register state
			Assert.AreEqual(expected_state, actual_state);
		}

		[Test]
		public void INV_0_Returns_0xFFFFFFFF() {
			Register source_register = Register.R0;
			Register destination_register = Register.R1;

			vm.SetRegister(destination_register, address_start);

			UInt32 value = 0;
			UInt32 expected = 0xFFFFFFFF;

			vm.SetRegister(source_register, value);
			Assert.AreEqual(value, vm.GetRegister(source_register));

			UInt32 instruction = ((byte)Opcode.INV, (byte)destination_register, (byte)source_register, (byte)0).AssembleUInt32();
			UInt32[] expected_state = GetRegisterState();
			Assert.IsTrue(vm.ExecuteInstruction(instruction));
			UInt32[] actual_state = GetRegisterState();

			// PC should be incremented by 4
			expected_state[(int)Register.PC] += 4;

			// Dst should be expected value
			expected_state[(int)destination_register] = expected;

			// Check registers match previous register state
			Assert.AreEqual(expected_state, actual_state);
		}

		[Test]
		public void INV_0xFFFFFFFF_Returns_0() {
			Register source_register = Register.R0;
			Register destination_register = Register.R1;

			vm.SetRegister(destination_register, address_start);

			UInt32 value = 0xFFFFFFFF;
			UInt32 expected = 0;

			vm.SetRegister(source_register, value);
			Assert.AreEqual(value, vm.GetRegister(source_register));

			UInt32 instruction = ((byte)Opcode.INV, (byte)destination_register, (byte)source_register, (byte)0).AssembleUInt32();
			UInt32[] expected_state = GetRegisterState();
			Assert.IsTrue(vm.ExecuteInstruction(instruction));
			UInt32[] actual_state = GetRegisterState();

			// PC should be incremented by 4
			expected_state[(int)Register.PC] += 4;

			// Dst should be expected value
			expected_state[(int)destination_register] = expected;

			// Check registers match previous register state
			Assert.AreEqual(expected_state, actual_state);
		}

		[Test]
		public void INV_NonZero() {
			Register source_register = Register.R0;
			Register destination_register = Register.R1;

			vm.SetRegister(destination_register, address_start);

			UInt32 value = 0x46791084;
			UInt32 expected = (value ^ 0xFFFFFFFF);

			vm.SetRegister(source_register, value);
			Assert.AreEqual(value, vm.GetRegister(source_register));

			UInt32 instruction = ((byte)Opcode.INV, (byte)destination_register, (byte)source_register, (byte)0).AssembleUInt32();
			UInt32[] expected_state = GetRegisterState();
			Assert.IsTrue(vm.ExecuteInstruction(instruction));
			UInt32[] actual_state = GetRegisterState();

			// PC should be incremented by 4
			expected_state[(int)Register.PC] += 4;

			// Dst should be expected value
			expected_state[(int)destination_register] = expected;

			// Check registers match previous register state
			Assert.AreEqual(expected_state, actual_state);
		}
		#endregion
	}
}