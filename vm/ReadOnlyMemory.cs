using System;

namespace FTG.Studios.BISC.VM {

    /// <summary>
    /// BISC memory element that does not clear on reset and cannot be written to normally.
    /// </summary>
    public class ReadOnlyMemory : NonVolatileMemory {

        public ReadOnlyMemory(UInt32 len) : base(len) { 
			MetaName = "ROM ".AssembleUInt32();
		}

        /// <summary>
        /// Write an array of bytes to the read-only memory.
        /// </summary>
        public bool Flash(UInt32 address, byte[] data) {
            if (address >= AddressLength) return false;
            for (int i = 0; i < data.Length; i++) {
                // If this is the first write to this address, populate it with zeros first.
                // This sets the other fields in case we are only writing part of an array, and protects the read function.
                if (!memory.ContainsKey((UInt32)((address + i) >> 2))) {
                    memory.Add((UInt32)((address + i) >> 2), new byte[] { 0, 0, 0, 0 });
                }

                // Shift the dictionary address by 2 to prevent multiple, overlapping entries.
                memory[(UInt32)(address + i) >> 2] // Get the address of the byte array that the byte should be written into.
                  [((address & 0x3) + i) % 4] // Get an index for the byte array between 0-3.
                  = data[i]; // Assign the data.
            }
            return true;
        }

        #region IMemoryModule
        /// <summary>
        /// Writes do not work for read-only memory.
        /// </summary>
        public override bool Write(UInt32 address, byte[] data) {
            return false;
        }
        #endregion

    }
}
