using System;
using System.IO;

namespace FTG.Studios.BISC.Assembler {

    class Application {

        static void Main(string[] args) {
            //Console.Title = "BISC Assembler";
			
			if (args.Length <= 0) {
				PrintHelp();
				return;
			}
			
            string file_name = args[0];
            UInt32[] program = Assembler.Assemble(File.ReadAllText(file_name + ".asm"));
            
            for (UInt32 addr = 0; addr < program.Length; addr++) {
                Console.WriteLine("{0:x}: {1:x08}", addr * 4, program[addr]);
            }

            BISC.Program.Write(file_name + ".bin", new BISC.Program(program));
        }
		
		static void PrintHelp() {
			Console.WriteLine("Usage: bisc-asm file");
		}
    }
}
