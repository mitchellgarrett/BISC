using System;
using System.Runtime.InteropServices;

namespace FTG.Studios.BISC.VM {

    [StructLayout(LayoutKind.Explicit, Size = 4)]
	/// <summary>
	/// A 32-bit word in little endian.
	/// </summary>
    public struct Word {
        [FieldOffset(0)] public byte Byte0;
        [FieldOffset(1)] public byte Byte1;
        [FieldOffset(2)] public byte Byte2;
        [FieldOffset(3)] public byte Byte3;

        public byte this[int index] {
            get {
                switch (index) {
                    case 0: return Byte0;
                    case 1: return Byte1;
                    case 2: return Byte2;
                    case 3: return Byte3;
                    default: throw new IndexOutOfRangeException();
                }
            }
            set {
                switch (index) {
                    case 0: Byte0 = value; break;
                    case 1: Byte1 = value; break;
                    case 2: Byte2 = value; break;
                    case 3: Byte3 = value; break;
                    default: throw new IndexOutOfRangeException();
                }
            }
        }
    }
}
