using System;
using System.IO;

namespace FTG.Studios.BISC {

    class Application {

        static void Main(string[] args) {
            //Console.Title = "BISC Virtual Machine";
			
			if (args.Length <= 0) {
				PrintHelp();
				return;
			}
			
            string file_name = args[0];
            Program program = Assembler.Assemble(File.ReadAllText(file_name + ".asm"));
            Program.Write(file_name + ".bin", program);
            VirtualMachine vm = new VirtualMachine();
			vm.SingleStep = false;
            vm.Execute(program);
        }
		
		static void PrintHelp() {
			Console.WriteLine("Usage: bisc-vm file");
		}
    }
}
