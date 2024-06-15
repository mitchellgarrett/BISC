using System;

namespace FTG.Studios.BISC.VM {

    /// <summary>
    /// BISC memory element.
    /// </summary>
    public abstract class MemoryModule {

        public UInt32 MetaName    { get; protected set; }
        public UInt32 MetaID      { get; protected set; }

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
        /// <param name="data">An array of bytes read from the Memory Module of the same size as the referenced array.</param>
        public abstract bool Read(UInt32 address, ref byte[] data);

        /// <summary>
        /// Write an array of bytes of arbitrary length.
        /// </summary>
        /// <param name="address">32-bit starting address for the read.</param>
        /// <param name="data">An array of arbitrary length to be written to the Memory Module.</param>
        public abstract bool Write(UInt32 address, byte[] data);
    }
}
