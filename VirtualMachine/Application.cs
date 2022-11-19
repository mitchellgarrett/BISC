using System;
using System.IO;

namespace FTG.Studios.BISC {

    class Application {

        static void Main(string[] args) {
            Console.Title = "BISC Virtual Machine";

            string file_name = "Programs/fibonacci";
            Program program = Assembler.Assemble(File.ReadAllText(file_name + ".asm"));
            Program.Write(file_name + ".bin", program);
            VirtualMachine vm = new VirtualMachine();

            vm.Execute(program);
        }
    }
}
