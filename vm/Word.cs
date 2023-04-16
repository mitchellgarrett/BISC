using System;
using System.Runtime.InteropServices;

namespace FTG.Studios.BISC.VM {

    [StructLayout(LayoutKind.Explicit, Size = 4)]
    public struct Word {

        [FieldOffset(0)] public UInt32 w;

        [FieldOffset(0)] public UInt16 s0;
        [FieldOffset(2)] public UInt16 s1;

        [FieldOffset(0)] public byte b0;
        [FieldOffset(1)] public byte b1;
        [FieldOffset(2)] public byte b2;
        [FieldOffset(3)] public byte b3;

        public byte this[int index] {
            get {
                switch (index) {
                    case 0: return b0;
                    case 1: return b1;
                    case 2: return b2;
                    case 3: return b3;
                    default: throw new IndexOutOfRangeException();
                }
            }
            set {
                switch (index) {
                    case 0: b0 = value; break;
                    case 1: b1 = value; break;
                    case 2: b2 = value; break;
                    case 3: b3 = value; break;
                    default: throw new IndexOutOfRangeException();
                }
            }
        }
    }
}
