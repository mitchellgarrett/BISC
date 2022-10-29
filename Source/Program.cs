using System;

namespace FTG.Studios.BISC {

    class Program {

        static void Main(string[] args) {

            UInt32[] instructions = new UInt32[] { 0x00000000, 0x02001234, 0x03040500 };
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
