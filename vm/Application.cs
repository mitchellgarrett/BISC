using System;
using System.Diagnostics;

namespace FTG.Studios.BISC.VM
{

	class Application
	{

		enum Flags { None = 0x0, SingleStep = 0x1, Debug = 0x2, Stat = 0x4 };
		static VirtualMachine vm;

		static void Main(string[] args)
		{
			//Console.Title = "BISC Virtual Machine";

			if (args.Length <= 0)
			{
				PrintHelp();
				return;
			}

			string file_name = args[0];

			Flags options = Flags.None;
			//options = Flags.Stat;
			//options = Flags.Debug;
			//options = Flags.SingleStep;
			//options = Flags.SingleStep | Flags.Debug;

			MemoryManager mmu = new MemoryManager();

			VolatileMemory ram = new VolatileMemory(0, 0x4000);
			mmu.AddDevice(ram);

			Terminal terminal = new Terminal(0x4000, 80, 24);
			mmu.AddDevice(terminal);

			RandomNumberGenerator rng = new RandomNumberGenerator(0x5000);
			mmu.AddDevice(rng);

			vm = new VirtualMachine(mmu);

			// Load BEEF executable into memory
			BEEF.ObjectFile beef = BEEF.ObjectFile.Deserialize(file_name + ".exe");
			LoadProgram(beef);

			Stopwatch sw = new Stopwatch();
			UInt32 inst_count = 0;
			if (options.HasFlag(Flags.Debug))
			{
				Console.Clear();
				PrintRegisters();
				PrintStack();
			}
			else
			{
				sw.Start();
			}

			while (vm.IsRunning)
			{
				if (options.HasFlag(Flags.SingleStep))
				{
					if (options.HasFlag(Flags.Debug))
					{
						Console.SetCursorPosition(0, Specification.NUM_REGISTERS - 1);
						Console.Write("Continue execution...");
					}
					Console.ReadKey(true);
				}

				UInt32 instruction = vm.GetMemory32(vm.GetRegister(Register.PC));
				bool success = vm.ExecuteNext();
				inst_count++;

				/*
				UInt32 instruction = program[vm.GetRegister((int)Register.PC) / 4];
				bool success = vm.ExecuteInstruction(instruction);
				inst_count++;
				if (vm.GetRegister((int)Register.PC) >= program.Length * 4) vm.Halt();
				*/

				if (options.HasFlag(Flags.Debug))
				{
					Console.Clear();
					PrintRegisters();
					PrintStack();
					Console.SetCursorPosition(0, Specification.NUM_REGISTERS - 2);
					Console.Write("inst: ");
					if (success) PrintInstruction(instruction);
					else PrintInvalidInstruction(instruction);
				}
			}

			if (options.HasFlag(Flags.Debug))
			{
				Console.SetCursorPosition(0, Specification.NUM_REGISTERS - 1);
				Console.Write("Program complete...");
				Console.ReadKey(true);
			}
			else if (options.HasFlag(Flags.Stat))
			{
				sw.Stop();
				Console.WriteLine("Program complete...");
				Console.WriteLine($"Execution time: {sw.Elapsed.TotalSeconds}s");
				Console.WriteLine($"Instruction count: {inst_count} ({Math.Round(inst_count / sw.Elapsed.TotalMilliseconds, 2)}/ms)");
				Console.WriteLine($"Cycle time: {Math.Round(inst_count / sw.Elapsed.TotalSeconds / 1000, 2)}kHz");
			}
			else
			{
				Console.SetCursorPosition(0, Console.WindowHeight - 1);
			}
		}

		static bool LoadProgram(BEEF.ObjectFile program)
		{
			UInt32 entry_point = program.FileHeader.EntryPoint;
			vm.SetRegister(Register.PC, entry_point);
			for (int s = 0; s < program.FileHeader.SectionCount; s++)
			{
				BEEF.SectionHeader header = program.SectionHeaders[s];
				UInt32 section_address = header.Address;
				for (UInt32 b = 0; b < program.SectionData[s].Length; b++)
				{
					byte value = program.SectionData[s][b];
					vm.SetMemory8(section_address + b, value);
				}
			}

			return false;
		}

		static void PrintHelp()
		{
			Console.WriteLine("Usage: bisc-vm file");
		}

		static void PrintRegisters()
		{
			Console.SetCursorPosition(0, 0);
			for (int i = 0; i < Specification.NUM_REGISTERS; i++)
			{
				Console.WriteLine("{0}: 0x{1:x8}", Specification.REGISTER_NAMES[i], vm.GetRegister(i));
			}
		}

		static void PrintStack()
		{
			Console.SetCursorPosition(32, 2);
			for (int y = 0, i = 0; y < 256 / 16; y++)
			{
				Console.SetCursorPosition(26, 2 + y);
				Console.Write("0x{0:x2}:", y * 16);
				for (int x = 0; x < 4; x++, i += 4)
				{
					UInt32 value = vm.GetMemory32((UInt32)i);
					Console.SetCursorPosition(32 + x * 9, 2 + y);
					Console.Write("{0:x8}", value);
				}
			}
		}

		static void PrintInstruction(UInt32 instruction)
		{
			byte[] bytes = Specification.DisassembleInteger32(instruction);

			byte opcode = bytes[0];
			string output = ((Opcode)opcode).ToString().ToLower() + ' ';

			ArgumentType[] arg_types = Specification.instruction_format_definitions[(int)Specification.instruction_formats[opcode]];
			for (int i = 0; i < arg_types.Length; i++)
			{
				switch (arg_types[i])
				{
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

		static void PrintInvalidInstruction(UInt32 instruction)
		{
			Console.Write("Illegal instruction: 0x{0:x8}", instruction);
		}
	}
}
