using FTG.Studios.BISC.VM;
using NUnit.Framework;
using System;

namespace FTG.Studios.BISC.Test {

    [TestFixture]
    public class VirtualMachineTest {

        VirtualMachine vm;
        VolatileMemory memory;
        UInt32 address_start, address_end, address_length;

        UInt32 AssembleInstruction(byte a, byte b, byte c, byte d) {
            return Specification.AssembleInteger32(a, b, c, d);
        }

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
                if (((Register)reg).IsValid()) {
                    if (reg == (int)Register.SP) {
                        Assert.AreEqual(VirtualMachine.STACK_END, vm.GetRegister(reg));
                    } else {
                        Assert.AreEqual(0, vm.GetRegister(reg));
                    }
                } else {
                    Assert.AreEqual(0xffffffff, vm.GetRegister(reg));
                }
            }
        }

        [Test]
        public void SetRegister() {
            UInt32 expected = 0xbabecafe;
            for (int reg = 0; reg < Specification.NUM_REGISTERS; reg++) {
                if (((Register)reg).IsValid()) {
                    vm.SetRegister(reg, expected);
                    Assert.AreEqual(expected, vm.GetRegister(reg));
                }
            }
        }

        [Test]
        public void GetMemory8() {

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
            byte[] expected_bytes = Specification.DisassembleInteger16(expected);
            memory.Write(address_start, expected_bytes);

            UInt32 value = vm.GetMemory16(address_start);
            Assert.AreEqual(expected, (UInt16)(value & 0xffff));
        }

        [Test]
        public void SetMemory32() {
            UInt32 expected = 0xbabecafe;
            byte[] expected_bytes = Specification.DisassembleInteger32(expected);
            memory.Write(address_start, expected_bytes);

            UInt32 value = vm.GetMemory32(address_start);
            Assert.AreEqual(expected, value);
        }

        [Test]
        public void Execute() {
            // NOP
            UInt32 inst = AssembleInstruction((byte)Opcode.NOP, 0, 0, 0);
            UInt32 pc = vm.GetRegister(Register.PC);
            Assert.IsTrue(vm.Execute(inst));

            // PC should be incremented by 4
            Assert.AreEqual(pc + 4, vm.GetRegister(Register.PC));

            // Invalid instruction
            inst = 0xffffffff;
            pc = vm.GetRegister(Register.PC);
            Assert.IsFalse(vm.Execute(inst));

            // PC should be the same
            Assert.AreEqual(pc, vm.GetRegister(Register.PC));
        }
        #endregion

        #region System Instructions
        [Test]
        public void NOP() {
            UInt32 inst = AssembleInstruction((byte)Opcode.NOP, 0, 0, 0);
            UInt32[] expected_state = GetRegisterState();
            Assert.IsTrue(vm.Execute(inst));
            UInt32[] actual_state = GetRegisterState();

            // PC should be incremented by 4
            expected_state[(int)Register.PC] += 4;

            // Check registers match previous register state
            Assert.AreEqual(expected_state, actual_state);

            Assert.IsTrue(vm.IsRunning);
        }

        [Test]
        public void HLT() {
            UInt32 inst = AssembleInstruction((byte)Opcode.HLT, 0, 0, 0);
            UInt32[] expected_state = GetRegisterState();
            Assert.IsTrue(vm.Execute(inst));
            UInt32[] actual_state = GetRegisterState();

            // Check registers match previous register state, PC does not increment
            Assert.AreEqual(expected_state, actual_state);

            Assert.IsFalse(vm.IsRunning);
        }

        [Test]
        public void SYS() {
            UInt32 inst = AssembleInstruction((byte)Opcode.SYS, 0, 0, 0);
            UInt32[] expected_state = GetRegisterState();
            Assert.IsTrue(vm.Execute(inst));
            UInt32[] actual_state = GetRegisterState();

            // PC should be incremented by 4
            expected_state[(int)Register.PC] += 4;

            // Check registers match previous register state
            Assert.AreEqual(expected_state, actual_state);
        }

        [Test]
        public void CALL() {
            UInt32 call_addr = 0xbabecafe;
            Register call_reg = Register.R0;

            vm.SetRegister(call_reg, call_addr);

            UInt32 inst = AssembleInstruction((byte)Opcode.CALL, (byte)call_reg, 0, 0);
            UInt32[] expected_state = GetRegisterState();
            Assert.IsTrue(vm.Execute(inst));
            UInt32[] actual_state = GetRegisterState();

            // RA should be PC + 4
            expected_state[(int)Register.RA] = expected_state[(int)Register.PC] + 4;

            // PC should be the call address
            expected_state[(int)Register.PC] = call_addr;

            // Check registers match previous register state
            Assert.AreEqual(expected_state, actual_state);
        }

        [Test]
        public void RET() {
            UInt32 ret_addr = 0xbabecafe;

            vm.SetRegister(Register.RA, ret_addr);

            UInt32 inst = AssembleInstruction((byte)Opcode.RET, 0, 0, 0);
            UInt32[] expected_state = GetRegisterState();
            Assert.IsTrue(vm.Execute(inst));
            UInt32[] actual_state = GetRegisterState();

            // PC should be RA
            expected_state[(int)Register.PC] = expected_state[(int)Register.RA];

            // Check registers match previous register state
            Assert.AreEqual(expected_state, actual_state);
        }
        #endregion

        #region Load Instructions
        [Test]
        public void LLI() {
            UInt16 imm = 0xcafe;
            byte[] immb = Specification.DisassembleInteger16(imm);
            Register reg = Register.R0;

            UInt32 inst = AssembleInstruction((byte)Opcode.LLI, (byte)reg, immb[0], immb[1]);
            UInt32[] expected_state = GetRegisterState();
            Assert.IsTrue(vm.Execute(inst));
            UInt32[] actual_state = GetRegisterState();

            // PC should be incremented by 4
            expected_state[(int)Register.PC] += 4;

            // Reg should be value of immediate
            expected_state[(int)reg] = imm;

            // Check registers match previous register state
            Assert.AreEqual(expected_state, actual_state);
        }

        [Test]
        public void LUI() {
            UInt16 imm = 0xbabe;
            byte[] immb = Specification.DisassembleInteger16(imm);
            Register reg = Register.R0;

            UInt32 inst = AssembleInstruction((byte)Opcode.LUI, (byte)reg, immb[0], immb[1]);
            UInt32[] expected_state = GetRegisterState();
            Assert.IsTrue(vm.Execute(inst));
            UInt32[] actual_state = GetRegisterState();

            // PC should be incremented by 4
            expected_state[(int)Register.PC] += 4;

            // Reg should be value of immediate
            expected_state[(int)reg] = (UInt32)(imm << 16);

            // Check registers match previous register state
            Assert.AreEqual(expected_state, actual_state);
        }

        [Test]
        public void MOV() {
            Register reg0 = Register.R0;
            Register reg1 = Register.R1;

            UInt32 expected = 0xbabecafe;
            vm.SetRegister(reg1, expected);

            UInt32 inst = AssembleInstruction((byte)Opcode.MOV, (byte)reg0, (byte)reg1, 0);
            UInt32[] expected_state = GetRegisterState();
            Assert.IsTrue(vm.Execute(inst));
            UInt32[] actual_state = GetRegisterState();

            // PC should be incremented by 4
            expected_state[(int)Register.PC] += 4;

            // Reg0 should be value of moved reg1
            expected_state[(int)reg0] = expected_state[(int)reg1];
            Assert.AreEqual(expected, expected_state[(int)reg0]);

            // Check registers match previous register state
            Assert.AreEqual(expected_state, actual_state);
        }
        #endregion

        #region Memory Instructions
        [Test]
        public void LDW() {
            Register src = Register.R0;
            Register dst = Register.R1;
            byte offset = 127;

            vm.SetRegister(src, address_start);

            UInt32 expected = 0xbabecafe;
            UInt32 addr = (UInt32)(vm.GetRegister(src) + (sbyte)offset);
            vm.SetMemory32(addr, expected);

            UInt32 inst = AssembleInstruction((byte)Opcode.LDW, (byte)dst, (byte)src, offset);
            UInt32[] expected_state = GetRegisterState();
            Assert.IsTrue(vm.Execute(inst));
            UInt32[] actual_state = GetRegisterState();

            // PC should be incremented by 4
            expected_state[(int)Register.PC] += 4;

            // Dst should have expected value
            expected_state[(int)dst] = expected;

            // Check registers match previous register state
            Assert.AreEqual(expected_state, actual_state);
        }

        [Test]
        public void LDH() {
            Register src = Register.R0;
            Register dst = Register.R1;
            byte offset = 127;

            vm.SetRegister(src, address_start);

            UInt32 value = 0xbabecafe;
            byte[] bytes = Specification.DisassembleInteger32(value);
            UInt32 expected = Specification.AssembleInteger16(bytes[0], bytes[1]);
            UInt32 addr = (UInt32)(vm.GetRegister(src) + (sbyte)offset);
            vm.SetMemory32(addr, value);

            UInt32 inst = AssembleInstruction((byte)Opcode.LDH, (byte)dst, (byte)src, offset);
            UInt32[] expected_state = GetRegisterState();
            Assert.IsTrue(vm.Execute(inst));
            UInt32[] actual_state = GetRegisterState();

            // PC should be incremented by 4
            expected_state[(int)Register.PC] += 4;

            // Dst should have expected value
            expected_state[(int)dst] = expected;

            // Check registers match previous register state
            Assert.AreEqual(expected_state, actual_state);
        }

        [Test]
        public void LDB() {
            Register src = Register.R0;
            Register dst = Register.R1;
            byte offset = 127;

            vm.SetRegister(src, address_start);

            UInt32 value = 0xbabecafe;
            byte[] bytes = Specification.DisassembleInteger32(value);
            UInt32 expected = bytes[0];
            UInt32 addr = (UInt32)(vm.GetRegister(src) + (sbyte)offset);
            vm.SetMemory32(addr, value);

            UInt32 inst = AssembleInstruction((byte)Opcode.LDB, (byte)dst, (byte)src, offset);
            UInt32[] expected_state = GetRegisterState();
            Assert.IsTrue(vm.Execute(inst));
            UInt32[] actual_state = GetRegisterState();

            // PC should be incremented by 4
            expected_state[(int)Register.PC] += 4;

            // Dst should have expected value
            expected_state[(int)dst] = expected;

            // Check registers match previous register state
            Assert.AreEqual(expected_state, actual_state);
        }

        [Test]
        public void STW() {
            Register src = Register.R0;
            Register dst = Register.R1;
            byte offset = 127;

            UInt32 expected = 0xbabecafe;

            vm.SetRegister(dst, address_start);
            vm.SetRegister(src, expected);

            UInt32 inst = AssembleInstruction((byte)Opcode.STW, (byte)src, (byte)dst, offset);
            UInt32[] expected_state = GetRegisterState();
            Assert.IsTrue(vm.Execute(inst));
            UInt32[] actual_state = GetRegisterState();

            // PC should be incremented by 4
            expected_state[(int)Register.PC] += 4;

            // Check registers match previous register state
            Assert.AreEqual(expected_state, actual_state);

            // Value at memory address should match src
            UInt32 addr = (UInt32)(vm.GetRegister(dst) + (sbyte)offset);
            Assert.AreEqual(expected, vm.GetMemory32(addr));
        }

        [Test]
        public void STH() {
            Register src = Register.R0;
            Register dst = Register.R1;
            byte offset = 127;

            UInt32 value = 0xbabecafe;
            byte[] bytes = Specification.DisassembleInteger32(value);
            UInt32 expected = Specification.AssembleInteger16(bytes[2], bytes[3]);

            vm.SetRegister(dst, address_start);
            vm.SetRegister(src, expected);

            UInt32 inst = AssembleInstruction((byte)Opcode.STH, (byte)src, (byte)dst, offset);
            UInt32[] expected_state = GetRegisterState();
            Assert.IsTrue(vm.Execute(inst));
            UInt32[] actual_state = GetRegisterState();

            // PC should be incremented by 4
            expected_state[(int)Register.PC] += 4;

            // Check registers match previous register state
            Assert.AreEqual(expected_state, actual_state);

            // Value at memory address should match src
            UInt32 addr = (UInt32)(vm.GetRegister(dst) + (sbyte)offset);
            Assert.AreEqual(expected, vm.GetMemory32(addr));
        }

        [Test]
        public void STB() {
            Register src = Register.R0;
            Register dst = Register.R1;
            byte offset = 127;

            UInt32 value = 0xbabecafe;
            byte[] bytes = Specification.DisassembleInteger32(value);
            UInt32 expected = bytes[3];

            vm.SetRegister(dst, address_start);
            vm.SetRegister(src, expected);

            UInt32 inst = AssembleInstruction((byte)Opcode.STB, (byte)src, (byte)dst, offset);
            UInt32[] expected_state = GetRegisterState();
            Assert.IsTrue(vm.Execute(inst));
            UInt32[] actual_state = GetRegisterState();

            // PC should be incremented by 4
            expected_state[(int)Register.PC] += 4;

            // Check registers match previous register state
            Assert.AreEqual(expected_state, actual_state);

            // Value at memory address should match src
            UInt32 addr = (UInt32)(vm.GetRegister(dst) + (sbyte)offset);
            Assert.AreEqual(expected, vm.GetMemory32(addr));
        }
        #endregion

        #region Arithmetic Instructions
        [Test]
        public void ADD() {
            Register arg0 = Register.R0;
            Register arg1 = Register.R1;
            Register dst = Register.R2;

            UInt32 val0 = 0xbabe;
            UInt32 val1 = 0xcafe;
            UInt32 expected = val0 + val1;

            vm.SetRegister(arg0, val0);
            vm.SetRegister(arg1, val1);

            UInt32 inst = AssembleInstruction((byte)Opcode.ADD, (byte)dst, (byte)arg0, (byte)arg1);
            UInt32[] expected_state = GetRegisterState();
            Assert.IsTrue(vm.Execute(inst));
            UInt32[] actual_state = GetRegisterState();

            // PC should be incremented by 4
            expected_state[(int)Register.PC] += 4;

            // Dst should be expected value
            expected_state[(int)dst] = expected;

            // Check registers match previous register state
            Assert.AreEqual(expected_state, actual_state);
        }

        [Test]
        public void SUB() {
            Register arg0 = Register.R0;
            Register arg1 = Register.R1;
            Register dst = Register.R2;

            UInt32 val0 = 0xbabe;
            UInt32 val1 = 0xcafe;
            UInt32 expected = val0 - val1;

            vm.SetRegister(arg0, val0);
            vm.SetRegister(arg1, val1);

            UInt32 inst = AssembleInstruction((byte)Opcode.SUB, (byte)dst, (byte)arg0, (byte)arg1);
            UInt32[] expected_state = GetRegisterState();
            Assert.IsTrue(vm.Execute(inst));
            UInt32[] actual_state = GetRegisterState();

            // PC should be incremented by 4
            expected_state[(int)Register.PC] += 4;

            // Dst should be expected value
            expected_state[(int)dst] = expected;

            // Check registers match previous register state
            Assert.AreEqual(expected_state, actual_state);
        }

        [Test]
        public void MUL() {
            Register arg0 = Register.R0;
            Register arg1 = Register.R1;
            Register dst = Register.R2;

            UInt32 val0 = 0xbabe;
            UInt32 val1 = 0xcafe;
            UInt32 expected = val0 * val1;

            vm.SetRegister(arg0, val0);
            vm.SetRegister(arg1, val1);

            UInt32 inst = AssembleInstruction((byte)Opcode.MUL, (byte)dst, (byte)arg0, (byte)arg1);
            UInt32[] expected_state = GetRegisterState();
            Assert.IsTrue(vm.Execute(inst));
            UInt32[] actual_state = GetRegisterState();

            // PC should be incremented by 4
            expected_state[(int)Register.PC] += 4;

            // Dst should be expected value
            expected_state[(int)dst] = expected;

            // Check registers match previous register state
            Assert.AreEqual(expected_state, actual_state);
        }

        [Test]
        public void DIV() {
            Register arg0 = Register.R0;
            Register arg1 = Register.R1;
            Register dst = Register.R2;

            UInt32 val0 = 0xbabe;
            UInt32 val1 = 0xcafe;
            UInt32 expected = val0 / val1;

            vm.SetRegister(arg0, val0);
            vm.SetRegister(arg1, val1);

            UInt32 inst = AssembleInstruction((byte)Opcode.DIV, (byte)dst, (byte)arg0, (byte)arg1);
            UInt32[] expected_state = GetRegisterState();
            Assert.IsTrue(vm.Execute(inst));
            UInt32[] actual_state = GetRegisterState();

            // PC should be incremented by 4
            expected_state[(int)Register.PC] += 4;

            // Dst should be expected value
            expected_state[(int)dst] = expected;

            // Check registers match previous register state
            Assert.AreEqual(expected_state, actual_state);
        }

        [Test]
        public void MOD() {
            Register arg0 = Register.R0;
            Register arg1 = Register.R1;
            Register dst = Register.R2;

            UInt32 val0 = 0xbabe;
            UInt32 val1 = 0xcafe;
            UInt32 expected = val0 % val1;

            vm.SetRegister(arg0, val0);
            vm.SetRegister(arg1, val1);

            UInt32 inst = AssembleInstruction((byte)Opcode.MOD, (byte)dst, (byte)arg0, (byte)arg1);
            UInt32[] expected_state = GetRegisterState();
            Assert.IsTrue(vm.Execute(inst));
            UInt32[] actual_state = GetRegisterState();

            // PC should be incremented by 4
            expected_state[(int)Register.PC] += 4;

            // Dst should be expected value
            expected_state[(int)dst] = expected;

            // Check registers match previous register state
            Assert.AreEqual(expected_state, actual_state);
        }
        #endregion

        #region Negation Instructions
        [Test]
        public void NOT() {
            Register src = Register.R0;
            Register dst = Register.R1;

            UInt32 value = 0xbabecafe;
            UInt32 expected = value != 0u ? 0u : 1u;

            vm.SetRegister(src, value);

            UInt32 inst = AssembleInstruction((byte)Opcode.NOT, (byte)dst, (byte)src, 0);
            UInt32[] expected_state = GetRegisterState();
            Assert.IsTrue(vm.Execute(inst));
            UInt32[] actual_state = GetRegisterState();

            // PC should be incremented by 4
            expected_state[(int)Register.PC] += 4;

            // Dst should be expected value
            expected_state[(int)dst] = expected;

            // Check registers match previous register state
            Assert.AreEqual(expected_state, actual_state);
        }

        [Test]
        public void NEG() {
            Register src = Register.R0;
            Register dst = Register.R1;

            UInt32 value = 0xbabecafe;
            UInt32 expected = (value ^ 0xFFFFFFFF) + 1;

            vm.SetRegister(src, value);

            UInt32 inst = AssembleInstruction((byte)Opcode.NEG, (byte)dst, (byte)src, 0);
            UInt32[] expected_state = GetRegisterState();
            Assert.IsTrue(vm.Execute(inst));
            UInt32[] actual_state = GetRegisterState();

            // PC should be incremented by 4
            expected_state[(int)Register.PC] += 4;

            // Dst should be expected value
            expected_state[(int)dst] = expected;

            // Check registers match previous register state
            Assert.AreEqual(expected_state, actual_state);
        }

        [Test]
        public void INV() {
            Register src = Register.R0;
            Register dst = Register.R1;

            vm.SetRegister(dst, address_start);

            UInt32 value = 0xbabecafe;
            UInt32 expected = (value ^ 0xFFFFFFFF);

            vm.SetRegister(src, value);
            Assert.AreEqual(value, vm.GetRegister(src));

            UInt32 inst = AssembleInstruction((byte)Opcode.INV, (byte)dst, (byte)src, 0);
            UInt32[] expected_state = GetRegisterState();
            Assert.IsTrue(vm.Execute(inst));
            UInt32[] actual_state = GetRegisterState();

            // PC should be incremented by 4
            expected_state[(int)Register.PC] += 4;

            // Dst should be expected value
            expected_state[(int)dst] = expected;

            // Check registers match previous register state
            Assert.AreEqual(expected_state, actual_state);
        }
        #endregion
    }
}