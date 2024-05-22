using System;
using System.Runtime.InteropServices;

namespace FTG.Studios.BISC.VM {
	
	[StructLayout(LayoutKind.Explicit)]
	public struct RegisterValue {
		[FieldOffset(0)] UInt32 Unsigned;
		[FieldOffset(0)] int Integer;
		[FieldOffset(0)] float Float;
	}
}