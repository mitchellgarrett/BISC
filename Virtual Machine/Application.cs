using System;

namespace FTG.Studios.BISC {

    class Application {

        static void Main(string[] args) {
            Console.Title = "BISC Virtual Machine";

            UInt32[] instructions = Assembler.ReadInstructions("Programs/instructions.bin");
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
