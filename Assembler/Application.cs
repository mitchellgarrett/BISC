using System;
using System.IO;

namespace FTG.Studios.BISC {

    class Application {

        static void Main(string[] args) {
            Console.Title = "BISC Assembler";

            UInt32[] instructions = Assembler.Assemble(File.ReadAllText("Programs/instructions.asm"));
            foreach (var inst in instructions) {
                Console.WriteLine("{0:x8}", inst);
            }

            Assembler.WriteInstructions("Programs/instructions.bin", instructions);
        }
    }
}
