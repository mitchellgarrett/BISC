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
			AssemblerResult result = Assembler.Assemble(file_name + ".asm", File.ReadAllText(file_name + ".asm"));
			BEEF.ObjectFile beef = result.Assemble();
			BEEF.ObjectFile.Serialize(beef, file_name + ".exe");

			Console.WriteLine(beef);
		}

		static void PrintHelp()
		{
			Console.WriteLine("Usage: bisc-asm file");
		}
	}
}
