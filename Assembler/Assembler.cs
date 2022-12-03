using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FTG.Studios.BISC {

    /// <summary>
    /// BISC Assembler.
    /// </summary>
    public static class Assembler {

        static Dictionary<string, byte> opcodes;
		static Dictionary<string, PseudoOpcode> pseudo_opcodes;
        static Dictionary<string, UInt32> symbols;

        /// <summary>
        /// Assembles BISC source code into binary opcodes.
        /// </summary>
        /// <param name="source">Source code.</param>
        /// <returns>An executable program.</returns>
        public static Program Assemble(string source) {
            if (opcodes == null) {
                opcodes = new Dictionary<string, byte>();
                foreach (Opcode opcode in Enum.GetValues(typeof(Opcode))) {
                    opcodes[opcode.ToString()] = (byte)opcode;
                }
            }
			
			if (pseudo_opcodes == null) {
                pseudo_opcodes = new Dictionary<string, PseudoOpcode>();
                foreach (PseudoOpcode opcode in Enum.GetValues(typeof(PseudoOpcode))) {
                    pseudo_opcodes[opcode.ToString()] = opcode;
                }
            }
			
			//source = Preprocessor.Preprocess(source);
            List<UInt32> instructions = new List<UInt32>();
            using (StringReader reader = new StringReader(source)) {
                string line;
                while ((line = reader.ReadLine()) != null) {
                    int comment_index = line.IndexOf(Specification.COMMENT);
                    if (comment_index >= 0) line = line.Substring(0, comment_index);
                    if (string.IsNullOrEmpty(line)) continue;
					string[] pseudo = ResolvePseudoInstruction(line);
					if (pseudo != null) {
						for	(int i = 0; i < pseudo.Length; i++) {
							instructions.Add(ParseInstruction(pseudo[i]));
						}
					} else {
                    	instructions.Add(ParseInstruction(line));
					}
                }
            }

            Program program = new Program(instructions.ToArray());
            return program;
        }

        static string[] ResolvePseudoInstruction(string line) {
			string[] parameters = line.Split(' ', '\t', ',').Where(s => !string.IsNullOrEmpty(s)).ToArray();
			if (!pseudo_opcodes.TryGetValue(parameters[0].ToUpper(), out PseudoOpcode opcode)) return null;
			string[] instructions = Specification.pseudo_instructions[(int) opcode];
			for (int i = 0; i < instructions.Length; i++) {
				instructions[i] = string.Format(instructions[i].ToLower(), parameters);
			}
			return instructions;
        }

        /// <summary>
        /// Parses a line of BISC code into a single binary instruction.
        /// </summary>
        /// <param name="line">Line to parse.</param>
        /// <returns>A single BISC instruction in binary form.</returns>
        static UInt32 ParseInstruction(string line) {

            Console.WriteLine(line);
            string[] parameters = line.Split(' ', '\t', ',').Where(s => !string.IsNullOrEmpty(s)).ToArray();
            UInt32 instruction = 0x00000000;

            byte opcode = GetOpcode(parameters[0]);
            instruction |= (UInt32) opcode << 24;

            ArgumentType[] arg_types = Specification.argument_types[opcode];
            for (int i = 0; i < arg_types.Length; i++) {
                switch (arg_types[i]) {
                    case ArgumentType.Register:
                        instruction |= (UInt32) GetRegister(parameters[i + 1]) << (2 - i) * 8;
                        break;
                    case ArgumentType.Address:
                        instruction |= (UInt32) GetAddress(parameters[i + 1]) << (1 - i) * 8;
                        break;
                    case ArgumentType.IntegerImmediate:
                        instruction |= (UInt32) GetIntegerImmediate(parameters[i + 1]) << (1 - i) * 8;
                        break;
                    default:
                        break;
                }
            }

            return instruction;
        }

        /// <summary>
        /// Assembles a 4-byte array into a 32-bit unsigned integer.
        /// </summary>
        /// <param name="bytes">Byte array to convert into intger. Must be 4 bytes.</param>
        /// <returns>A single 32-bit unsigned integer.</returns>
        static UInt32 AssembleInstruction(byte[] bytes) {
            if (BitConverter.IsLittleEndian) Array.Reverse(bytes);
            return BitConverter.ToUInt32(bytes, 0);
        }

        /// <summary>
        /// Parses a string instruction into a binary opcode.
        /// </summary>
        /// <param name="mneumonic">String to parse.</param>
        /// <returns>Opcode in binary form.</returns>
        static byte GetOpcode(string mneumonic) {
            if (opcodes.TryGetValue(mneumonic.ToUpper(), out byte opcode)) return opcode;
            InvalidInstruction(mneumonic);
            return 0xFF;
        }

        /// <summary>
        /// Parses a register name into its index into the register file.
        /// </summary>
        /// <param name="mneumonic">Register name.</param>
        /// <returns>Register index.</returns>
        static byte GetRegister(string mneumonic) {
            for (byte i = 0; i < Specification.register_names.Length; i++) {
                if (mneumonic == Specification.register_names[i]) return i;
            }
            return 0xFF;
        }

        static byte GetAddress(string mneumonic) {
            return 0x00;
        }

        /// <summary>
        /// Parses a string into an unsigned 16-bit integer.
        /// </summary>
        /// <param name="mneumonic">String to parse. Can be a signed or unsigned integer, hexadecimal value prefixed with 0x, binary value prefixed with 0b, or single ASCII character wrapped in single quotes.</param>
        /// <returns>Unsigned 16-bit integer.</returns>
        static UInt16 GetIntegerImmediate(string mneumonic) {

            // Check for ASCII character
            if (mneumonic[0] == '\'') {
                if (mneumonic.Length != 3 || mneumonic[2] != '\'') {
                    //InvalidValue(mneumonic);
                    return 0xFFF;
                }
                return mneumonic[1];
            }

            // Check if value has prefix
            if (mneumonic.Length >= 3 && mneumonic[0] == '0') {
                // Check for hexadecimal value
                if (mneumonic[1] == 'x' || mneumonic[1] == 'X') return Convert.ToUInt16(mneumonic.Substring(2), 16);
                // Check for binary value
                if (mneumonic[1] == 'b' || mneumonic[1] == 'B') return Convert.ToUInt16(mneumonic.Substring(2), 2);
            }

            // Check for negative value
            bool isNegative = false;
            if (mneumonic[0] == '-') {
                isNegative = true;
                mneumonic = mneumonic.Substring(1);
            }
            if (UInt16.TryParse(mneumonic, out UInt16 imm)) {
                if (isNegative) {
                    imm--;
                    imm |= 0xFFFF;
                }
                return imm;
            }

            //InvalidValue(mneumonic);
            return 0xFFFF;
        }

        static byte GetSignedImmediate(string mneumonic) {
            return 0xFF;
        }

        static byte GetDecimalImmediate(string mneumonic) {
            return 0xFF;
        }

        static void InvalidValue(string mneumonic) {
            Console.Error.WriteLine($"Invalid value: {mneumonic}");
        }

        static void InvalidInstruction(string mneumonic) {
            Console.Error.WriteLine($"Invalid instruction: {mneumonic}");
        }
    }
}
