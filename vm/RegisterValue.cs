using System;
using System.Runtime.InteropServices;

namespace FTG.Studios.BISC.VM {
	
	[StructLayout(LayoutKind.Explicit)]
	public struct RegisterValue {
		[FieldOffset(0)] public UInt32 UValue;
		[FieldOffset(0)] public int    IValue;
		[FieldOffset(0)] public float  FValue;
	}
}