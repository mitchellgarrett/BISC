using System;

namespace FTG.Studios.BISC.VM {

    // 80 * 24 characters
    // Memory map: 0x0000 - 0x1000
    public class Terminal : Memory {

        public int Width { get; private set; }
        public int Height { get; private set; }

        readonly UInt32 CursorEnableAddress;
        readonly UInt32 CursorXAddress;
        readonly UInt32 CursorYAddress;
        readonly UInt32 ReadCharAddress;

        byte cursor_enabled, cursor_x, cursor_y;
        readonly byte[,] memory;

        public Terminal(UInt32 addr, int width, int height) : base() {
            this.Width = width;
            this.Height = height;
            AddressStart = addr;
            AddressLength = ReadCharAddress;
            CursorEnableAddress = AddressStart + (UInt32)(Width * Height);
            CursorXAddress = CursorEnableAddress + 1;
            CursorYAddress = CursorXAddress + 1;
            ReadCharAddress = CursorYAddress + 1;
            Console.SetWindowSize(Width, Height);
            memory = new byte[Height, Width];
        }

        public override void Reset() {
            for (int y = 0; y < Height; y++) {
                for (int x = 0; x < Width; x++) {
                    memory[y, x] = 0;
                }
            }
            Refresh();
        }

        public override bool Read(UInt32 address, ref byte[] data) {
            for (int i = 0; i < data.Length; i++) {
                if (address + i == CursorEnableAddress) {
                    data[i] = cursor_enabled;
                    continue;
                }

                if (address + i == CursorXAddress) {
                    data[i] = cursor_x;
                    continue;
                }

                if (address + i == CursorYAddress) {
                    data[i] = cursor_y;
                    continue;
                }

                if (address + i == ReadCharAddress) {
                    data[i] = (byte)Console.ReadKey(true).KeyChar;
                    continue;
                }

                if (address + i >= memory.Length) data[i] = 0;
                else {
                    int row = (int)((address + i) / Width);
                    int col = (int)((address + i) % Width);
                    data[i] = memory[row, col];
                }
            }
            return true;
        }

        public override bool Write(UInt32 address, byte[] data) {
            System.Diagnostics.Debug.WriteLine($"Writing to address: 0x{address:x8}");
            if (address == CursorEnableAddress) {
                cursor_enabled = data[0];
                Console.CursorVisible = cursor_enabled == 1;
                return true;
            }

            if (address == CursorXAddress) {
                cursor_x = data[0];
                Console.CursorLeft = cursor_x;
                return true;
            }

            if (address == CursorYAddress) {
                cursor_y = data[0];
                Console.CursorTop = cursor_y;
                return true;
            }

            if (address == ReadCharAddress) {
                return true;
            }

            for (int i = 0; i < data.Length; i++) {
                if (address + i < memory.Length) {
                    int row = (int)((address + i) / Width);
                    int col = (int)((address + i) % Width);
                    memory[row, col] = data[i];
                    Refresh(row, col);
                } else break;
            }
            return true;
        }

        void Refresh() {
            Console.Clear();
            char[] line = new char[Width];
            for (int y = 0; y < Height; y++) {
                for (int x = 0; x < Width; x++) {
                    line[x] = (char)memory[y, x];
                }
                Console.Write(line);
            }
        }

        void Refresh(int row, int col) {
            Console.SetCursorPosition(col, row);
            Console.Write((char)memory[row, col]);
        }
    }
}
