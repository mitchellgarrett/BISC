using System;
using System.IO;

namespace FTG.Studios.BISC {

    public class Program {

        public UInt32 Offset;
        public UInt32[] Instructions;

        public Program(UInt32[] instructions) {
            Offset = 0;
            this.Instructions = instructions;
        }

        /// <summary>
        /// Writes program to specified file in binary form.
        /// </summary>
        /// <param name="path">File to write to.</param>
        /// <param name="program">Program to write.</param>
        public static void Write(string path, Program program) {
            byte[] bytes = new byte[program.Instructions.Length * 4];
            Buffer.BlockCopy(program.Instructions, 0, bytes, 0, bytes.Length);
            if (BitConverter.IsLittleEndian) {
                byte b0, b1, b2, b3;
                for (int i = 0; i < bytes.Length; i += 4) {
                    b0 = bytes[i + 0];
                    b1 = bytes[i + 1];
                    b2 = bytes[i + 2];
                    b3 = bytes[i + 3];
                    bytes[i + 0] = b3;
                    bytes[i + 1] = b2;
                    bytes[i + 2] = b1;
                    bytes[i + 3] = b0;
                }
            }
            File.WriteAllBytes(path, bytes);
        }

        /// <summary>
        /// Reads program from a binary file.
        /// </summary>
        /// <param name="path">File to read from.</param>
        /// <returns>An executable program.</returns>
        public static Program Read(string path) {
            byte[] bytes = File.ReadAllBytes(path);
            if (BitConverter.IsLittleEndian) {
                byte b0, b1, b2, b3;
                for (int i = 0; i < bytes.Length; i += 4) {
                    b0 = bytes[i + 0];
                    b1 = bytes[i + 1];
                    b2 = bytes[i + 2];
                    b3 = bytes[i + 3];
                    bytes[i + 0] = b3;
                    bytes[i + 1] = b2;
                    bytes[i + 2] = b1;
                    bytes[i + 3] = b0;
                }
            }

            UInt32[] instructions = new UInt32[bytes.Length / 4];
            Buffer.BlockCopy(bytes, 0, instructions, 0, bytes.Length);

            Program program = new Program(instructions);
            return program;
        }
    }
}