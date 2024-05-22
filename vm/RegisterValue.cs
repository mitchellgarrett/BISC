using System;
using System.Runtime.InteropServices;

namespace FTG.Studios.BISC.VM {
	
	/// <summary>
	/// Union to store a single 32-bit register value. 
	/// Can be accessed as a signed, unsigned, or floating point value.
	/// </summary>
	[StructLayout(LayoutKind.Explicit)]
	public struct RegisterValue {
		[FieldOffset(0)] public UInt32 UValue;
		[FieldOffset(0)] public int    IValue;
		[FieldOffset(0)] public float  FValue;
	}
}