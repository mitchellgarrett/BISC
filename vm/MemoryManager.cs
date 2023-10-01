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
            foreach (Memory other in memory) {
				if (device.AddressStart >= other.AddressStart && device.AddressStart <= other.AddressEnd || device.AddressEnd <= other.AddressEnd && device.AddressEnd >= other.AddressStart) {
					Console.Error.WriteLine($"Unable to add device {device} (start=0x{device.AddressStart:x8}, end=0x{device.AddressEnd:x8}, length=0x{device.AddressLength:x8}) to MMU!");
					return false;
				}
			}
			if (device.AddressStart 
                < AddressStart) AddressStart = device.AddressStart;
            if (device.AddressEnd 
                > AddressEnd) AddressLength = device.AddressEnd - AddressStart;
				            
            memory.Add(device);
			System.Diagnostics.Debug.WriteLine($"Added device {device} (start=0x{device.AddressStart:x8}, end=0x{device.AddressEnd:x8}, length=0x{device.AddressLength:x8}) to MMU");
			Console.WriteLine($"Added device {device} (start=0x{device.AddressStart:x8}, end=0x{device.AddressEnd:x8}, length=0x{device.AddressLength:x8}) to MMU");
            return true;
        }

        public override void Reset() {
            foreach (Memory device in memory) {
                device.Reset();
            }
        }

        public override bool Read(UInt32 address, ref byte[] data) {
			System.Diagnostics.Debug.WriteLine($"Reading {data.Length} bytes from address 0x{address:x8}");
            foreach (Memory device in memory) {
                if (address >= device.AddressStart && address <= device.AddressEnd) {
                    return device.Read(address, ref data);
                }
            }
            return false;
        }

        public override bool Write(UInt32 address, byte[] data) {
			System.Diagnostics.Debug.WriteLine($"Writing {data.Length} bytes to address 0x{address:x8}");
            foreach (Memory device in memory) {
				if (address >= device.AddressStart && address <= device.AddressEnd) {
						return device.Write(address, data);
					}
			}
            return false;
        }
    }
}
