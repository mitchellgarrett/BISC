using System;

namespace FTG.Studios.BISC.VM {

    /// <summary>
    /// BISC memory element.
    /// </summary>
    public abstract class Memory {

        /// <summary>
        /// Starting address for this memory element.
        /// </summary>
        public UInt32 AddressStart { get; protected set; }

        /// <summary>
        /// Ending address for this memory element;
        /// </summary>
        public UInt32 AddressEnd { get { return AddressStart + AddressLength; } }

        /// <summary>
        /// Length of address range.
        /// </summary>
        public UInt32 AddressLength { get; protected set; }

        /// <summary>
        /// Populate with the desired reset functionality.
        /// </summary>
        public abstract void Reset();

        /// <summary>
        /// Return an array of bytes based on the address.
        /// </summary>
        /// <param name="address">32-bit starting address for the read.</param>
        /// <param name="data">An array of bytes read from the Memory Element of the same size as the referenced array.</param>
        public abstract bool Read(UInt32 address, ref byte[] data);

        /// <summary>
        /// Write an array of bytes of arbitrary length.
        /// </summary>
        /// <param name="address">32-bit starting address for the read.</param>
        /// <param name="data">An array of arbitrary length to be written to the Memory Element.</param>
        public abstract bool Write(UInt32 address, byte[] data);
    }
}
