using System;
using System.Diagnostics;
using System.IO;

namespace FTG.Studios.BISC.VM {

	class Application {

		enum Flags { None = 0x0, SingleStep = 0x1, Debug = 0x2 };
		static VirtualMachine vm;

		static void Main(string[] args) {
			//Console.Title = "BISC Virtual Machine";

			if (args.Length <= 0) {
				PrintHelp();
				return;
			}

			string file_name = args[0];
			UInt32[] program = Asm.Assembler.Assemble(File.ReadAllText(file_name + ".asm"));
			BISC.Program.Write(file_name + ".bin", new BISC.Program(program));

			Flags options = Flags.None;
			//options = Flags.Debug;
			//options = Flags.SingleStep | Flags.Debug;

			vm = new VirtualMachine();
			vm.Reset();

			Stopwatch sw = new Stopwatch();
			UInt32 inst_count = 0;
			if (options.HasFlag(Flags.Debug)) {
				Console.Clear();
				PrintRegisters();
				PrintStack();
            } else {
				sw.Start();
            }

			while (vm.IsRunning) {
				if (options.HasFlag(Flags.SingleStep)) {
					if (options.HasFlag(Flags.Debug)) {
						Console.SetCursorPosition(0, Specification.NUM_REGISTERS - 1);
						Console.Write("Continue execution...");
					}
					Console.ReadKey(true);
				}

				UInt32 instruction = program[vm.GetRegister((int)Register.PC) / 4];
				bool success = vm.Execute(instruction);
				inst_count++;
				if (vm.GetRegister((int)Register.PC) >= program.Length * 4) vm.Halt();

				if (options.HasFlag(Flags.Debug)) {
					Console.Clear();
					PrintRegisters();
					PrintStack();
					Console.SetCursorPosition(0, Specification.NUM_REGISTERS - 2);
					Console.Write("inst: ");
					if (success) PrintInstruction(instruction);
					else PrintInvalidInstruction(instruction);
				}
			}

			if (options.HasFlag(Flags.Debug)) {
				Console.SetCursorPosition(0, Specification.NUM_REGISTERS - 1);
				Console.Write("Program complete...");
				Console.ReadKey(true);
			} else {
				sw.Stop();
				Console.WriteLine("Program complete...");
				Console.WriteLine($"Execution time: {sw.Elapsed.TotalSeconds}s");
				Console.WriteLine($"Instruction count: {inst_count} ({Math.Round(inst_count / sw.Elapsed.TotalMilliseconds, 2)}/ms)");
				Console.WriteLine($"Cycle time: {Math.Round(inst_count / sw.Elapsed.TotalSeconds / 1000, 2)}kHz");
			}
		}

		static void PrintHelp() {
			Console.WriteLine("Usage: bisc-vm file");
		}

		static void PrintRegisters() {
			Console.SetCursorPosition(0, 0);
			for (int i = 0; i < Specification.NUM_REGISTERS; i++) {
				if (((Register)i).IsValid()) Console.WriteLine("{0}: 0x{1:x8}", Specification.REGISTER_NAMES[i], vm.GetRegister(i));
			}
		}

		static void PrintStack() {
			Console.SetCursorPosition(32, 2);
			for (int y = 0, i = 0; y < VirtualMachine.STACK_SIZE / 16; y++) {
				Console.SetCursorPosition(26, 2 + y);
				Console.Write("0x{0:x2}:", y * 16);
				for (int x = 0; x < 4; x++, i += 4) {
					UInt32 value = vm.GetMemory32((UInt32)(VirtualMachine.STACK_START + i));
					Console.SetCursorPosition(32 + x * 9, 2 + y);
					Console.Write("{0:x8}", value);
				}
			}
		}

		static void PrintInstruction(UInt32 instruction) {
			byte[] bytes = Specification.DisassembleInteger32(instruction);

			byte opcode = bytes[0];
			string output = ((Opcode)opcode).ToString().ToLower() + ' ';

			ArgumentType[] arg_types = Specification.instruction_format_definitions[(int)Specification.instruction_formats[opcode]];
			for (int i = 0; i < arg_types.Length; i++) {
				switch (arg_types[i]) {
					case ArgumentType.Register:
						output += string.Format("{0} (0x{1:x8})", Specification.REGISTER_NAMES[bytes[i + 1]], vm.GetRegister(bytes[i + 1]));
						break;

					case ArgumentType.Memory:
						sbyte offset = (sbyte)bytes[i + 2];
						UInt32 addr = (UInt32)(vm.GetRegister(bytes[i + 1]) + offset);
						UInt32 value = vm.GetMemory32(addr);
						output += string.Format("{0}[{1}] (0x{2:x8}, @0x{3:x8})", Specification.REGISTER_NAMES[bytes[i + 1]], offset, value, addr);
						break;

					case ArgumentType.Immediate16:
						UInt16 imm = Specification.AssembleInteger16(bytes[i + 1], bytes[i + 2]);
						output += string.Format("0x{0:x4}", imm);
						break;
                }
				if (arg_types[i] != ArgumentType.None && i < arg_types.Length - 1) output += ", ";
            }

			Console.Write(output);
		}

		static void PrintInvalidInstruction(UInt32 instruction) {
			Console.Write("Illegal instruction: 0x{0:x8}", instruction);
		}
	}
}
