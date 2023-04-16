using System;
using System.Collections.Generic;

namespace FTG.Studios.BISC.VM {

    /// <summary>
    /// BISC memory element.
    /// </summary>
    public class VolatileMemory : Memory {

        readonly Dictionary<UInt32, byte[]> memory;

        public VolatileMemory(UInt32 addr, UInt32 len) {
            AddressStart = addr;
            AddressLength = len;
            memory = new Dictionary<UInt32, byte[]>();
        }

        #region IMemory
        /// <summary>
        /// Clear the internal memory of the BasicVolatileMemory.
        /// </summary>
        public override void Reset() { memory.Clear(); }

        /// <summary>
        /// Read an array of bytes from the BasicVolatileMemory.
        /// </summary>
        public override bool Read(UInt32 address, ref byte[] data) {
            if (address < AddressStart || address >= AddressEnd) return false;
            for (int i = 0; i < data.Length; i++) {
                if (memory.TryGetValue((UInt32)(address + i) >> 2, out byte[] value)) {
                    // Shift the dictionary address by 2 to prevent multiple, overlapping entries.
                    data[i] = value[((address & 0x3) + i) % 4]; // Get the address of the byte you want to read.
                } else {
                    data[i] = 0;
                }
            }
            return true;
        }

        /// <summary>
        /// Write an array of bytes to the BasicVolatileMemory.
        /// </summary>
        public override bool Write(UInt32 address, byte[] data) {
            if (address < AddressStart || address >= AddressEnd) return false;
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
        #endregion

    }
}
