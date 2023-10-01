using System;

namespace FTG.Studios.BISC.VM {

	/// <summary>
	/// 80 * 24 characters
    /// Cursor Enabled : 0x0000 - 0x0003
	/// Cursor X       : 0x0004 - 0x0007
	/// Cursor Y       : 0x0008 - 0x000B
	/// Read Character : 0x000C - 0x000F
	/// Characters     : 0x0010 - 0x100F
	/// </summary>
    public class Terminal : Memory {

        public int Width { get; private set; }
        public int Height { get; private set; }

        readonly UInt32 CursorEnableAddress;
        readonly UInt32 CursorXAddress;
        readonly UInt32 CursorYAddress;
        readonly UInt32 ReadCharAddress;
		readonly UInt32 CharacterDataAddress;

        byte cursor_enabled, cursor_x, cursor_y;
        readonly byte[,] memory;

        public Terminal(UInt32 addr, int width, int height) : base() {
            this.Width = width;
            this.Height = height;
            AddressStart = addr;
			CursorEnableAddress = AddressStart;
            CursorXAddress = CursorEnableAddress + 4;
            CursorYAddress = CursorXAddress + 4;
            ReadCharAddress = CursorYAddress + 4;
			CharacterDataAddress = ReadCharAddress + 4;
            AddressLength = CharacterDataAddress + (UInt32)(Width * Height) - AddressStart;
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
				
				UInt32 relative_address = address + (UInt32)i - CharacterDataAddress;
                if (relative_address >= memory.Length) data[i] = 0;
                else {
                    int row = (int)((relative_address) / Width);
                    int col = (int)((relative_address) % Width);
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
				UInt32 relative_address = address + (UInt32)i - CharacterDataAddress;
                if (relative_address < memory.Length) {
                    int row = (int)((relative_address) / Width);
                    int col = (int)((relative_address) % Width);
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
