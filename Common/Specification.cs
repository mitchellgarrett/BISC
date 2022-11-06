namespace FTG.Studios.BISC {

    public static class Specification {

        public const char COMMENT = ';';

        public static readonly string[] register_names = {
            "pc", "sp", "ra", "rv", "r0", "r1", "r2", "r3", "r4", "r5", "r6", "r7", "f0", "f1", "f2", "f3", "f4", "f5", "f6", "f7"
        };

        public readonly static ArgumentType[][] argument_types = {
            new ArgumentType[] { ArgumentType.None, ArgumentType.None, ArgumentType.None },             // NOP
            new ArgumentType[] { ArgumentType.None, ArgumentType.None, ArgumentType.None },             // HLT
            new ArgumentType[] { ArgumentType.None, ArgumentType.IntegerImmediate },                    // SYS
            new ArgumentType[] { ArgumentType.Address, ArgumentType.None },                             // CALL
            new ArgumentType[] { ArgumentType.None, ArgumentType.None, ArgumentType.None },             // RET

            new ArgumentType[] { ArgumentType.Register, ArgumentType.IntegerImmediate },                // LLI
            new ArgumentType[] { ArgumentType.Register, ArgumentType.IntegerImmediate },                // LUI
            new ArgumentType[] { ArgumentType.Register, ArgumentType.Register, ArgumentType.None },     // MOV

            new ArgumentType[] { ArgumentType.Register, ArgumentType.Register, ArgumentType.Register }, // ADD
            new ArgumentType[] { ArgumentType.Register, ArgumentType.Register, ArgumentType.Register }, // SUB
            new ArgumentType[] { ArgumentType.Register, ArgumentType.Register, ArgumentType.Register }, // MUL
            new ArgumentType[] { ArgumentType.Register, ArgumentType.Register, ArgumentType.Register }, // DIV
            new ArgumentType[] { ArgumentType.Register, ArgumentType.Register, ArgumentType.Register }, // MOD

            new ArgumentType[] { ArgumentType.Register, ArgumentType.Register, ArgumentType.None },     // NOT
            new ArgumentType[] { ArgumentType.Register, ArgumentType.Register, ArgumentType.None },     // NEG
            new ArgumentType[] { ArgumentType.Register, ArgumentType.Register, ArgumentType.None },     // INV

            new ArgumentType[] { ArgumentType.Register, ArgumentType.Register, ArgumentType.Register }, // AND
            new ArgumentType[] { ArgumentType.Register, ArgumentType.Register, ArgumentType.Register }, // OR
            new ArgumentType[] { ArgumentType.Register, ArgumentType.Register, ArgumentType.Register }, // XOR
            new ArgumentType[] { ArgumentType.Register, ArgumentType.Register, ArgumentType.Register }, // BSL
            new ArgumentType[] { ArgumentType.Register, ArgumentType.Register, ArgumentType.Register }, // BSR

            new ArgumentType[] { ArgumentType.Register, ArgumentType.None, ArgumentType.None },         // JMP
            new ArgumentType[] { ArgumentType.Register, ArgumentType.Register, ArgumentType.None },     // JZ
            new ArgumentType[] { ArgumentType.Register, ArgumentType.Register, ArgumentType.None },     // JNZ
            new ArgumentType[] { ArgumentType.Register, ArgumentType.Register, ArgumentType.Register }, // JE
            new ArgumentType[] { ArgumentType.Register, ArgumentType.Register, ArgumentType.Register }, // JNE
            new ArgumentType[] { ArgumentType.Register, ArgumentType.Register, ArgumentType.Register }, // JGT
            new ArgumentType[] { ArgumentType.Register, ArgumentType.Register, ArgumentType.Register }, // JLT
            new ArgumentType[] { ArgumentType.Register, ArgumentType.Register, ArgumentType.Register }, // JGE
            new ArgumentType[] { ArgumentType.Register, ArgumentType.Register, ArgumentType.Register }, // JLE
        };
    }
}