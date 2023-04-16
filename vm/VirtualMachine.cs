using System;

namespace FTG.Studios.BISC.VM {

    /// <summary>
    /// BISC virtual machine.
    /// </summary>
    public class VirtualMachine {

        delegate bool InstructionHandler(byte opcode, byte arg0, byte arg1, byte arg2);
        InstructionHandler[] instructions;

        UInt32[] registers;

        UInt32 pc { get => registers[(int)Register.PC]; set { registers[(int)Register.PC] = value; } }
        UInt32 sp { get => registers[(int)Register.SP]; set { registers[(int)Register.SP] = value; } }
        UInt32 fp { get => registers[(int)Register.FP]; set { registers[(int)Register.FP] = value; } }
        UInt32 ra { get => registers[(int)Register.RA]; set { registers[(int)Register.RA] = value; } }
        UInt32 rv { get => registers[(int)Register.RV]; set { registers[(int)Register.RV] = value; } }
        UInt32 ri { get => registers[(int)Register.RI]; set { registers[(int)Register.RI] = value; } }

        public const UInt32 STACK_SIZE = 256;
        public const UInt32 STACK_END = STACK_SIZE;
        public const UInt32 STACK_START = STACK_END - STACK_SIZE;

        readonly Memory memory;

        public bool IsRunning { get; private set; }

        public VirtualMachine(Memory memory) {
            this.memory = memory;
            Initialize();
        }

        void Initialize() {
            registers = new UInt32[Specification.NUM_REGISTERS];
            instructions = new InstructionHandler[] {
                HLT, NOP, SYS, CALL, RET,
                LLI, LUI, MOV,
                LDW, LDH, LDB, STW, STH, STB,
                ADD, SUB, MUL, DIV, MOD,
                NOT, NEG, INV, AND, OR, XOR, BSL, BSR,
                JMP, JEZ, JNZ, JEQ, JNE, JGT, JLT, JGE, JLE
            };
            Reset();
        }

        public UInt32 GetRegister(int reg) {
            if (((Register)reg).IsValid()) return registers[reg];
            return 0xFFFFFFFF;
        }

        public UInt32 GetRegister(Register reg) {
            if (reg.IsValid()) return registers[(int)reg];
            return 0xFFFFFFFF;
        }

        public void SetRegister(int reg, UInt32 val) {
            if (((Register)reg).IsValid()) registers[reg] = val;
        }

        public void SetRegister(Register reg, UInt32 val) {
            if (reg.IsValid()) registers[(int)reg] = val;
        }

        bool IsValidRegister(byte reg) {
            return reg <= Specification.NUM_REGISTERS && reg != (byte)Register.INVALID0 && reg != (byte)Register.INVALID1;
        }

        public byte GetMemory8(UInt32 addr) {
            byte[] data = { 0 };
            memory.Read(addr, ref data);

            return data[0];
        }

        public UInt16 GetMemory16(UInt32 addr) {
            byte[] data = { 0, 0 };
            memory.Read(addr, ref data);

            return Specification.AssembleInteger16(data[0], data[1]);
        }

        public UInt32 GetMemory32(UInt32 addr) {
            byte[] data = { 0, 0, 0, 0 };
            memory.Read(addr, ref data);

            return Specification.AssembleInteger32(data[0], data[1], data[2], data[3]);
        }

        public void SetMemory8(UInt32 addr, UInt32 value) {
            byte[] data = { (byte)(value & 0xFF) };
            memory.Write(addr, data);
        }

        public void SetMemory16(UInt32 addr, UInt32 value) {
            byte[] data = Specification.DisassembleInteger16((UInt16)value);
            memory.Write(addr, data);
        }

        public void SetMemory32(UInt32 addr, UInt32 value) {
            byte[] data = Specification.DisassembleInteger32(value);
            memory.Write(addr, data);
        }

        public void Reset() {
            // Zero out the registers.
            for (int i = 0; i < registers.Length; i++) {
                registers[i] = 0;
            }

            memory.Reset();

            pc = 0;
            sp = STACK_END;
            IsRunning = true;
        }

        public void Halt() {
            IsRunning = false;
        }

        /// <summary>
        /// Executes a single 32-bit BISC instruction.
        /// </summary>
        /// <param name="instruction">32-bit instruction.</param>
        public bool Execute(UInt32 instruction) {
            //Console.WriteLine("Executing instruction: 0x{0:x8}", instruction);

            // Decompose instruction into its byte parameters
            byte[] bytes = Specification.DisassembleInteger32(instruction);

            byte opcode = bytes[0];
            byte arg0 = bytes[1];
            byte arg1 = bytes[2];
            byte arg2 = bytes[3];
            
            if (opcode >= 0 && opcode < instructions.Length) {
                if (!instructions[opcode](opcode, arg0, arg1, arg2)) {
                    // Set debug register to illegeal execution
                    Console.Error.WriteLine($"Illegal execution: 0x{instruction:x8}");
                    return false;
                }
            } else {
                // Set debug register to illegal instruction
                Console.Error.WriteLine($"Illegal instruction: 0x{instruction:x8}");
                return false;
            }

            return true;
        }

        #region Instructions

        #region System Instructions
        bool NOP(byte opcode, byte arg0, byte arg1, byte arg2) {
            if (opcode != ((byte)Opcode.NOP) || arg0 != 0 || arg1 != 0 || arg2 != 0) return false;

            pc += 4;
            return true;
        }

        bool HLT(byte opcode, byte arg0, byte arg1, byte arg2) {
            if (opcode != ((byte)Opcode.HLT) || arg0 != 0 || arg1 != 0 || arg2 != 0) return false;

            Halt();
            return true;
        }

        bool SYS(byte opcode, byte arg0, byte arg1, byte arg2) {
            if (opcode != ((byte)Opcode.SYS) || arg0 != 0 || arg1 != 0 || arg2 != 0) return false;

            pc += 4;
            return true;
        }

        bool CALL(byte opcode, byte arg0, byte arg1, byte arg2) {
            if (opcode != ((byte)Opcode.CALL) || !IsValidRegister(arg0) || arg1 != 0 || arg2 != 0) return false;

            // Set return address to next instruction
            ra = pc + 4;
            // Jump to called address
            pc = registers[arg0];
            return true;
        }

        bool RET(byte opcode, byte arg0, byte arg1, byte arg2) {
            if (opcode != ((byte)Opcode.RET) || arg0 != 0 || arg1 != 0 || arg2 != 0) return false;

            // Jump to return address
            pc = ra;
            return true;
        }
        #endregion

        #region Load Instructions
        bool LLI(byte opcode, byte arg0, byte arg1, byte arg2) {
            if (opcode != ((byte)Opcode.LLI) || !IsValidRegister(arg0)) return false;

            UInt16 imm = Specification.AssembleInteger16(arg1, arg2);
            registers[arg0] = imm;
            pc += 4;
            return true;
        }

        bool LUI(byte opcode, byte arg0, byte arg1, byte arg2) {
            if (opcode != ((byte)Opcode.LUI) || !IsValidRegister(arg0)) return false;

            UInt16 imm = Specification.AssembleInteger16(arg1, arg2);
            registers[arg0] = (UInt32)((registers[arg0] & 0xFFFF) | (UInt32)(imm << 16));
            pc += 4;
            return true;
        }

        bool MOV(byte opcode, byte arg0, byte arg1, byte arg2) {
            if (opcode != ((byte)Opcode.MOV) || !IsValidRegister(arg0) || !IsValidRegister(arg1) || arg2 != 0) return false;

            registers[arg0] = registers[arg1];
            pc += 4;
            return true;
        }
        #endregion

        #region Memory Instructions
        bool LDW(byte opcode, byte arg0, byte arg1, byte arg2) {
            if (opcode != ((byte)Opcode.LDW) || !IsValidRegister(arg0) || !IsValidRegister(arg1)) return false;

            sbyte offset = (sbyte)arg2;
            UInt32 addr = (UInt32)(registers[arg1] + offset);
            UInt32 value = GetMemory32(addr);
            registers[arg0] = value;
            pc += 4;
            return true;
        }

        bool LDH(byte opcode, byte arg0, byte arg1, byte arg2) {
            if (opcode != ((byte)Opcode.LDH) || !IsValidRegister(arg0) || !IsValidRegister(arg1)) return false;

            sbyte offset = (sbyte)arg2;
            UInt32 addr = (UInt32)(registers[arg1] + offset);
            UInt16 value = GetMemory16(addr);
            registers[arg0] = value;
            pc += 4;
            return true;
        }

        bool LDB(byte opcode, byte arg0, byte arg1, byte arg2) {
            if (opcode != ((byte)Opcode.LDB) || !IsValidRegister(arg0) || !IsValidRegister(arg1)) return false;

            sbyte offset = (sbyte)arg2;
            UInt32 addr = (UInt32)(registers[arg1] + offset);
            byte value = GetMemory8(addr);
            registers[arg0] = value;
            pc += 4;
            return true;
        }

        bool STW(byte opcode, byte arg0, byte arg1, byte arg2) {
            if (opcode != ((byte)Opcode.STW) || !IsValidRegister(arg0) || !IsValidRegister(arg1)) return false;

            sbyte offset = (sbyte)arg2;
            UInt32 addr = (UInt32)(registers[arg1] + offset);
            SetMemory32(addr, registers[arg0]);
            pc += 4;
            return true;
        }

        bool STH(byte opcode, byte arg0, byte arg1, byte arg2) {
            if (opcode != ((byte)Opcode.STH) || !IsValidRegister(arg0) || !IsValidRegister(arg1)) return false;

            sbyte offset = (sbyte)arg2;
            UInt32 addr = (UInt32)(registers[arg1] + offset);
            SetMemory16(addr, registers[arg0]);
            pc += 4;
            return true;
        }

        bool STB(byte opcode, byte arg0, byte arg1, byte arg2) {
            if (opcode != ((byte)Opcode.STB) || !IsValidRegister(arg0) || !IsValidRegister(arg1)) return false;

            sbyte offset = (sbyte)arg2;
            UInt32 addr = (UInt32)(registers[arg1] + offset);
            SetMemory8(addr, registers[arg0]);
            pc += 4;
            return true;
        }
        #endregion

        #region Arithmetic Instructions
        bool ADD(byte opcode, byte arg0, byte arg1, byte arg2) {
            if (opcode != ((byte)Opcode.ADD) || !IsValidRegister(arg0) || !IsValidRegister(arg1) || !IsValidRegister(arg2)) return false;

            registers[arg0] = registers[arg1] + registers[arg2];
            pc += 4;
            return true;
        }

        bool SUB(byte opcode, byte arg0, byte arg1, byte arg2) {
            if (opcode != ((byte)Opcode.SUB) || !IsValidRegister(arg0) || !IsValidRegister(arg1) || !IsValidRegister(arg2)) return false;

            registers[arg0] = registers[arg1] - registers[arg2];
            pc += 4;
            return true;
        }

        bool MUL(byte opcode, byte arg0, byte arg1, byte arg2) {
            if (opcode != ((byte)Opcode.MUL) || !IsValidRegister(arg0) || !IsValidRegister(arg1) || !IsValidRegister(arg2)) return false;

            registers[arg0] = registers[arg1] * registers[arg2];
            pc += 4;
            return true;
        }

        bool DIV(byte opcode, byte arg0, byte arg1, byte arg2) {
            if (opcode != ((byte)Opcode.DIV) || !IsValidRegister(arg0) || !IsValidRegister(arg1) || !IsValidRegister(arg2)) return false;

            registers[arg0] = registers[arg1] / registers[arg2];
            pc += 4;
            return true;
        }

        bool MOD(byte opcode, byte arg0, byte arg1, byte arg2) {
            if (opcode != ((byte)Opcode.MOD) || !IsValidRegister(arg0) || !IsValidRegister(arg1) || !IsValidRegister(arg2)) return false;

            registers[arg0] = registers[arg1] % registers[arg2];
            pc += 4;
            return true;
        }
        #endregion

        #region Negation Instructions
        bool NOT(byte opcode, byte arg0, byte arg1, byte arg2) {
            if (opcode != ((byte)Opcode.NOT) || !IsValidRegister(arg0) || !IsValidRegister(arg1) || arg2 != 0) return false;

            registers[arg0] = registers[arg1] != 0 ? 0u : 1u;
            pc += 4;
            return true;
        }

        bool NEG(byte opcode, byte arg0, byte arg1, byte arg2) {
            if (opcode != ((byte)Opcode.NEG) || !IsValidRegister(arg0) || !IsValidRegister(arg1) || arg2 != 0) return false;

            registers[arg0] = (registers[arg1] ^ 0xFFFFFFFF) + 1;
            pc += 4;
            return true;
        }

        bool INV(byte opcode, byte arg0, byte arg1, byte arg2) {
            if (opcode != ((byte)Opcode.INV) || !IsValidRegister(arg0) || !IsValidRegister(arg1) || arg2 != 0) return false;

            registers[arg0] = registers[arg1] ^ 0xFFFFFFFF;
            pc += 4;
            return true;
        }
        #endregion

        #region Logical Instructions
        bool AND(byte opcode, byte arg0, byte arg1, byte arg2) {
            if (opcode != ((byte)Opcode.AND) || !IsValidRegister(arg0) || !IsValidRegister(arg1) || !IsValidRegister(arg2)) return false;

            registers[arg0] = registers[arg1] & registers[arg2];
            pc += 4;
            return true;
        }

        bool OR(byte opcode, byte arg0, byte arg1, byte arg2) {
            if (opcode != ((byte)Opcode.OR) || !IsValidRegister(arg0) || !IsValidRegister(arg1) || !IsValidRegister(arg2)) return false;

            registers[arg0] = registers[arg1] | registers[arg2];
            pc += 4;
            return true;
        }

        bool XOR(byte opcode, byte arg0, byte arg1, byte arg2) {
            if (opcode != ((byte)Opcode.XOR) || !IsValidRegister(arg0) || !IsValidRegister(arg1) || !IsValidRegister(arg2)) return false;

            registers[arg0] = registers[arg1] ^ registers[arg2];
            pc += 4;
            return true;
        }

        bool BSL(byte opcode, byte arg0, byte arg1, byte arg2) {
            if (opcode != ((byte)Opcode.BSL) || !IsValidRegister(arg0) || !IsValidRegister(arg1) || !IsValidRegister(arg2)) return false;

            registers[arg0] = registers[arg1] << (Int32)registers[arg2];
            pc += 4;
            return true;
        }

        bool BSR(byte opcode, byte arg0, byte arg1, byte arg2) {
            if (opcode != ((byte)Opcode.BSR) || !IsValidRegister(arg0) || !IsValidRegister(arg1) || !IsValidRegister(arg2)) return false;

            registers[arg0] = registers[arg1] >> (Int32)registers[arg2];
            pc += 4;
            return true;
        }
        #endregion

        #region Jump Instructions
        bool JMP(byte opcode, byte arg0, byte arg1, byte arg2) {
            if (opcode != ((byte)Opcode.JMP) || !IsValidRegister(arg0) || arg1 != 0 || arg2 != 0) return false;

            pc = registers[arg0];
            return true;
        }

        bool JEZ(byte opcode, byte arg0, byte arg1, byte arg2) {
            if (opcode != ((byte)Opcode.JEZ) || !IsValidRegister(arg0) || !IsValidRegister(arg1) || arg2 != 0) return false;

            if (registers[arg1] == 0) pc = registers[arg0];
            else pc += 4;
            return true;
        }

        bool JNZ(byte opcode, byte arg0, byte arg1, byte arg2) {
            if (opcode != ((byte)Opcode.JNZ) || !IsValidRegister(arg0) || !IsValidRegister(arg1) || arg2 != 0) return false;

            if (registers[arg1] != 0) pc = registers[arg0];
            else pc += 4;
            return true;
        }

        bool JEQ(byte opcode, byte arg0, byte arg1, byte arg2) {
            if (opcode != ((byte)Opcode.JEQ) || !IsValidRegister(arg0) || !IsValidRegister(arg1) || !IsValidRegister(arg2)) return false;

            if (registers[arg1] == registers[arg2]) pc = registers[arg0];
            else pc += 4;
            return true;
        }

        bool JNE(byte opcode, byte arg0, byte arg1, byte arg2) {
            if (opcode != ((byte)Opcode.JNE) || !IsValidRegister(arg0) || !IsValidRegister(arg1) || !IsValidRegister(arg2)) return false;

            if (registers[arg1] != registers[arg2]) pc = registers[arg0];
            else pc += 4;
            return true;
        }

        bool JGT(byte opcode, byte arg0, byte arg1, byte arg2) {
            if (opcode != ((byte)Opcode.JGT) || !IsValidRegister(arg0) || !IsValidRegister(arg1) || !IsValidRegister(arg2)) return false;

            if (registers[arg1] > registers[arg2]) pc = registers[arg0];
            else pc += 4;
            return true;
        }

        bool JLT(byte opcode, byte arg0, byte arg1, byte arg2) {
            if (opcode != ((byte)Opcode.JLT) || !IsValidRegister(arg0) || !IsValidRegister(arg1) || !IsValidRegister(arg2)) return false;

            if (registers[arg1] < registers[arg2]) pc = registers[arg0];
            else pc += 4;
            return true;
        }

        bool JGE(byte opcode, byte arg0, byte arg1, byte arg2) {
            if (opcode != ((byte)Opcode.JGE) || !IsValidRegister(arg0) || !IsValidRegister(arg1) || !IsValidRegister(arg2)) return false;

            if (registers[arg1] >= registers[arg2]) pc = registers[arg0];
            else pc += 4;
            return true;
        }

        bool JLE(byte opcode, byte arg0, byte arg1, byte arg2) {
            if (opcode != ((byte)Opcode.JLE) || !IsValidRegister(arg0) || !IsValidRegister(arg1) || !IsValidRegister(arg2)) return false;

            if (registers[arg1] <= registers[arg2]) pc = registers[arg0];
            else pc += 4;
            return true;
        }
        #endregion

        #endregion
    }
}
