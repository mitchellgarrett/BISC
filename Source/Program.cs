using System;
using System.IO;

namespace FTG.Studios.BISC {

    class Program {

        static void Main(string[] args) {

            UInt32[] instructions = Assembler.Assemble(File.ReadAllText("Programs/instructions.asm"));
            foreach (var inst in instructions) {
                Console.WriteLine("{0:x8}", inst);
            }

            Assembler.WriteInstructions("Programs/instructions.bin", instructions);
            return;
            instructions = Assembler.ReadInstructions("Programs/instructions.bin");

            VirtualMachine vm = new VirtualMachine();

            vm.PrintRegisters();
            for (int i = 0; i < instructions.Length; i++) {
                Console.SetCursorPosition(0, 21);
                Console.Write("Continue execution...");
                Console.ReadKey(false);
                vm.ExecuteInstruction(instructions[i]);
            }
            Console.SetCursorPosition(0, 21);
            Console.Write("Program complete...");
            Console.ReadKey(false);
        }
    }
}
