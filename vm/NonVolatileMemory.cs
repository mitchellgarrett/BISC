using System;

namespace FTG.Studios.BISC.VM {

    /// <summary>
    /// BISC memory element that does not clear on reset.
    /// </summary>
    public class NonVolatileMemory : VolatileMemory {

        public NonVolatileMemory(UInt32 len) : base(len) { 
			MetaName = "NMEM".AssembleUInt32();
		}

        #region IMemoryModule
        // Read-only memory will not lose its contents on reset.
        public override void Reset() { }
        #endregion

    }
}
