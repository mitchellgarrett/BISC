using System;
using System.IO;

namespace FTG.Studios.BISC {

    class Application {

        static void Main(string[] args) {
            Console.Title = "BISC Assembler";

            string file_name = "Programs/test";
            Program program = Assembler.Assemble(File.ReadAllText(file_name + ".asm"));
            for (UInt32 addr = 0; addr < program.Instructions.Length; addr++) {
                Console.WriteLine("{0:x}: {1:x08}", program.Offset + addr * 4, program.Instructions[addr]);
            }

            Program.Write(file_name + ".bin", program);
        }
    }
}
