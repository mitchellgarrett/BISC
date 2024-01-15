using System;

namespace FTG.Studios.BISC.VM {

    /// <summary>
    /// BISC memory element.
    /// </summary>
    public class NonVolatileMemory : VolatileMemory {

        public NonVolatileMemory(UInt32 len) : base(len) { MetaName = Specification.AssembleInteger32FromString("NMEM"); }

        // Non-volatile memory will not lose its contents on reset.
        public override void Reset() { }

    }
}
