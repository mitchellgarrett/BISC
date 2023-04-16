using System;

namespace FTG.Studios.BISC.VM {

    /// <summary>
    /// BISC memory element.
    /// </summary>
    public class NonVolatileMemory : VolatileMemory {

        public NonVolatileMemory(UInt32 addr, UInt32 len) : base(addr, len) { }

        // Non-volatile memory will not lose its contents on reset.
        public override void Reset() { }

    }
}
