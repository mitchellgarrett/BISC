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
			AssemblerResult result = Assembler.Assemble(File.ReadAllText(file_name + ".asm"));
			BEEF.ObjectFile beef = ToBEEF(result);
			BEEF.ObjectFile.Serialize(beef, file_name + ".exe");
			Console.WriteLine(beef);
		}

		static void PrintHelp()
		{
			Console.WriteLine("Usage: bisc-asm file");
		}

		static BEEF.ObjectFile ToBEEF(AssemblerResult program)
		{
			byte[] machine_code = program.Assemble();

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
				Size = (UInt32)machine_code.Length,
				Name = ".text"
			};

			obj.SectionData = new byte[1][];
			obj.SectionData[0] = machine_code;

			return obj;
		}
	}
}
