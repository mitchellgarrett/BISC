using System;
using System.IO;

namespace FTG.Studios.BISC.Asm
{

	class Application
	{

		static void Main(string[] args)
		{
			if (args.Length <= 0)
			{
				PrintHelp();
				return;
			}

			string file_name = args[0];
			UInt32[] program = Assembler.Assemble(File.ReadAllText(file_name + ".asm"));
			BEEF.ObjectFile beef = ToBEEF(program);
			BEEF.ObjectFile.Serialize(beef, file_name + ".exe");
			Console.WriteLine(beef);

			/*Console.WriteLine($"Assembled file {file_name}.asm into {program.Length} instructions");
            for (UInt32 addr = 0; addr < program.Length; addr++) {
                Console.WriteLine("{0:x}: {1:x08}", addr * 4, program[addr]);
            }*/

			BISC.Program.Write(file_name + ".bin", new BISC.Program(program));
		}

		static void PrintHelp()
		{
			Console.WriteLine("Usage: bisc-asm file");
		}

		static BEEF.ObjectFile ToBEEF(UInt32[] machine_code)
		{
			BEEF.ObjectFile obj = new BEEF.ObjectFile();
			obj.FileHeader = new BEEF.FileHeader()
			{
				MagicNumber = BEEF.FileHeader.MAGIC_NUMBER,
				Architecture = 0xb,
				Endianness = BEEF.Endianness.Little,
				EntryPoint = 0,
				SectionTableOffset = 14,
				SectionCount = 1
			};

			obj.SectionHeaders = new BEEF.SectionHeader[1];
			obj.SectionHeaders[0] = new BEEF.SectionHeader()
			{
				Type = BEEF.SectionType.Program,
				Flags = BEEF.SectionFlag.Readable | BEEF.SectionFlag.Writable | BEEF.SectionFlag.Code | BEEF.SectionFlag.Executable,
				Offset = 14 + 32,
				Address = 0,
				Size = (UInt32)machine_code.Length * 4,
				Name = ".text"
			};

			obj.SectionData = new byte[1][];
			obj.SectionData[0] = new byte[machine_code.Length * 4];
			for (int i = 0; i < machine_code.Length; i++)
			{
				byte[] bytes = Specification.DisassembleInteger32(machine_code[i]);
				obj.SectionData[0][i * 4 + 0] = bytes[0];
				obj.SectionData[0][i * 4 + 1] = bytes[1];
				obj.SectionData[0][i * 4 + 2] = bytes[2];
				obj.SectionData[0][i * 4 + 3] = bytes[3];
			}

			return obj;
		}
	}
}
