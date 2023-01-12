using System;
using System.Collections.Generic;

namespace FTG.Studios.BISC {

  /// <summary>
  /// BISC memory element.
  /// </summary>
  public interface IMemory {

    /// <summary>
    /// Populate with the desired reset functionality.
    /// </summary>
    void Reset();

    /// <summary>
    /// Return an array of bytes based on the address.
    /// </summary>
    /// <param name="address">32-bit starting address for the read.</param>
    /// <param name="data">An array of bytes read from the Memory Element of the same size as the referenced array.</param>
    bool Read(UInt32 address, ref byte[] data);

    /// <summary>
    /// Write an array of bytes of arbitrary length.
    /// </summary>
    /// <param name="address">32-bit starting address for the read.</param>
    /// <param name="data">An array of arbitrary length to be written to the Memory Element.</param>
    bool Write(UInt32 address, byte[] data);
  }

  /// <summary>
  /// BISC memory element.
  /// </summary>
  public class BasicVolatileMemory : IMemory {

    Dictionary<UInt32, byte[]> memory;

    public BasicVolatileMemory() {
      memory = new Dictionary<UInt32, byte[]>();
    }

#region IMemory
    /// <summary>
    /// Clear the internal memory of the BasicVolatileMemory.
    /// </summary>
    public void Reset() { memory.Clear(); }

    /// <summary>
    /// Read an array of bytes from the BasicVolatileMemory.
    /// </summary>
    public bool Read(UInt32 address, ref byte[] data) {
      for(int i = 0; i < data.Length; i++) {
        // If the dictionary is missing our entry, return zero.
        if(!memory.ContainsKey((UInt32)((address + i) >> 2))) return false;

        // Shift the dictionary address by 2 to prevent multiple, overlapping entries.
        data[i] = memory[(UInt32)(address + i) >> 2] // Get the address of the byte array you want to read.
                        [((address & 0x3) + i) % 4]; // Get the address of the byte you want to read.
      }
      return true;
    }

    /// <summary>
    /// Write an array of bytes to the BasicVolatileMemory.
    /// </summary>
    public bool Write(UInt32 address, byte[] data) { 
      for(int i = 0; i < data.Length; i++) {
        // If this is the first write to this address, populate it with zeros first.
        // This sets the other fields in case we are only writing part of an array, and protects the read function.
        if(!memory.ContainsKey((UInt32)((address + i) >> 2))) {
          memory.Add((UInt32)((address + i) >> 2), new byte[] {0, 0, 0, 0});
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
