using System;
using System.Collections.Generic;

namespace FTG.Studios.BISC.VM {

    public class MemoryManager : Memory {

        readonly List<Memory> memory;

        public MemoryManager() {
            memory = new List<Memory>();
            AddressStart = AddressLength = 0;
        }

        public bool AddDevice(Memory device) {
            System.Diagnostics.Debug.WriteLine($"Adding device {device} (start=0x{device.AddressStart:x8}, end=0x{device.AddressEnd:x8}, length=0x{device.AddressLength:x8}) to MMU");
            if (device.AddressStart 
                < AddressStart) AddressStart = device.AddressStart;
            if (device.AddressEnd 
                > AddressEnd) AddressLength = device.AddressEnd - AddressStart;
            memory.Add(device);
            return true;
        }

        public override void Reset() {
            foreach (Memory device in memory) {
                device.Reset();
            }
        }

        public override bool Read(UInt32 address, ref byte[] data) {
            foreach (Memory device in memory) {
                if (address >= device.AddressStart && address <= device.AddressEnd) {
                    return device.Read(address, ref data);
                }
            }
            return false;
        }

        public override bool Write(UInt32 address, byte[] data) {
            foreach (Memory device in memory) {
                if (address >= device.AddressStart && address <= device.AddressEnd) {
                    return device.Write(address, data);
                }
            }
            return false;
        }
    }
}
