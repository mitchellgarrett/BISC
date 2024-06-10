using System;
using System.IO;

namespace FTG.Studios.BISC.Asm
{

	class Application
	{

		static void Main(string[] args)
		{
			if (args.Length != 1)
			{
				PrintHelp();
				return;
			}

			string file_name = args[0];

			AssemblyTree program = null;
			try {
				program = Assembler.Assemble(file_name + ".asm", File.ReadAllText(file_name + ".asm"));
			} catch (SyntaxErrorException exception) {
				Console.Error.WriteLine(exception.Message);
				Environment.Exit(1);
			}

			/*BEEF.ObjectFile beef = program.ToObjectFile();
			BEEF.ObjectFile.Serialize(beef, file_name + ".exe");

			Console.WriteLine(beef);*/
		}

		static void PrintHelp()
		{
			Console.WriteLine("Usage: bisc-asm file");
		}
	}
}
