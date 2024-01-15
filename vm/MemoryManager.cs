using System;
using System.Collections.Generic;

namespace FTG.Studios.BISC.VM {

    public class MemoryManagerMeta {
        public UInt32 Name;
        public UInt32 ID;
        public UInt32 Address;
        public UInt32 Length;
    }

    public class MemoryManager : MemoryModule {

        UInt32              maxModules;

        MemoryManagerMeta[] meta;
        bool[]              metaValid;
        MemoryModule[]      memoryModules;

        public MemoryManager(UInt32 maxModules, UInt32 metaAddress) {

            this.maxModules = maxModules;

            MetaName   = Specification.AssembleInteger32FromString("MMU ");
            Random rng = new Random();
            MetaID     = (UInt32)rng.Next();
            meta       = new MemoryManagerMeta[maxModules * 4];
            metaValid  = new bool[maxModules];

            memoryModules = new MemoryModule[4 * maxModules];
            AddressLength = 0xFFFF_FFFF;

            meta[0] = new MemoryManagerMeta();

            meta[0].Name    = MetaName;
            meta[0].ID      = MetaID;
            meta[0].Address = metaAddress;
            meta[0].Length  = (4 * 4 * maxModules) - 1; // In bytes.
            metaValid[0]    = true;

        }

        public bool AddModule(MemoryModule module, UInt32 addr) {

            UInt32 moduleStart = addr;
            UInt32 moduleEnd   = addr + module.AddressLength;

            // Check for overlaps with existing modules.
            for(int i = 0; i < maxModules; i++) {

                // We only have to check valid entries for collisions.
                if(metaValid[i]) {

                    // See if this range conflicts with the valid entry.
                    if(
                           (meta[i].Address <= moduleStart && (meta[i].Address + meta[i].Length) >= moduleStart) // Check if the starting address falls in a valid range.
                        || (meta[i].Address <= moduleEnd   && (meta[i].Address + meta[i].Length) >= moduleEnd  ) // Check if the ending address falls in a valid range.
                        || (meta[i].Address >= moduleStart && (meta[i].Address + meta[i].Length) <= moduleEnd  ) // Check if the new range completely covers another valid range.
                    ) {
                        Console.Error.WriteLine($"Unable to add module {module} (start=0x{moduleStart:x8}, end=0x{moduleEnd:x8}, length=0x{module.AddressLength:x8}) to MMU due to address overlap!");
                        return false;
                    }

                }
            }

            // Find a valid slot for the new module.
            UInt32 index = 0;
            for(UInt32 i = 0; i < maxModules; i++) {
                if(!metaValid[i]) {
                    index = i;
                    break;
                }
            }

            // Error out if all metadata is valid, indicating that there are no remaining module slots.
            if(index == 0) {
                Console.Error.WriteLine($"Unable to add module {module} (start=0x{addr:x8}, end=0x{moduleEnd:x8}, length=0x{module.AddressLength:x8}) to MMU, the maximum number of modules has been reached");
                return false;
            }

            // Add the module.
            meta[index]         = new MemoryManagerMeta();
            meta[index].Name    = module.MetaName;
            meta[index].ID      = module.MetaID;
            meta[index].Address = addr;
            meta[index].Length  = module.AddressLength;
            metaValid[index]    = true;

            memoryModules[index] = module;

			System.Diagnostics.Debug.WriteLine($"Added module {module} (start=0x{addr:x8}, end=0x{moduleEnd:x8}, length=0x{module.AddressLength:x8}) to MMU at index={index}");
			Console.WriteLine($"Added module {module} (start=0x{addr:x8}, end=0x{moduleEnd:x8}, length=0x{module.AddressLength:x8}) to MMU at index={index}");

            return true;
        }

        public bool RemoveModule(MemoryModule module) {

            for(int i = 0; i < maxModules; i++) {

                // See if we have a match in our list of modules.
                if(meta[i].ID == module.MetaID) {

                    meta[i]          = null;
                    metaValid[i]     = false;
                    memoryModules[i] = null;

                    return true;
                }
            }

            // Otherwise, the module was not found and cannot be removed.
            return false;
        }

        public override void Reset() {

            // Start at 1 to only reset the modules that the MMU controls.
            for(int i = 1; i < maxModules; i++) {
                if(metaValid[i]) memoryModules[i].Reset();
            }
        }

        public override bool Read(UInt32 address, ref byte[] data) {

			System.Diagnostics.Debug.WriteLine($"Reading {data.Length} bytes from address 0x{address:x8}");

            for(int dataIndex = 0; dataIndex < data.Length; dataIndex++) {

                // Default to zero in case a module is not hit.
                data[dataIndex] = 0;

                for(int moduleIndex = 0; moduleIndex < maxModules; moduleIndex++) {

                    // See if this entry is valid.
                    if(metaValid[moduleIndex]) {

                        // See if the address is within this module's address range.
                        if(address >= meta[moduleIndex].Address && address <= (meta[moduleIndex].Address + meta[moduleIndex].Length)) {

                            // See if the request fits completely in this address space.
                            if((address + data.Length) <= (meta[moduleIndex].Address + meta[moduleIndex].Length)) {
                                return memoryModules[moduleIndex].Read(address % meta[moduleIndex].Length, ref data);
                            }

                            // Otherwise, we will break up the transfer.
                            else {

                                // First, see how many bytes we can read from this module.
                                UInt32 toRead = (meta[moduleIndex].Address + meta[moduleIndex].Length) - address;
                                byte[] readData = new byte[toRead];

                                // Read what we can from this module:
                                // FUTURE: Return on failure?
                                memoryModules[moduleIndex].Read(address % meta[moduleIndex].Length, ref readData);

                                // Apply this read data to our main array:
                                foreach(byte r in readData) {

                                    data[dataIndex] = r;
                                    dataIndex++;
                                    address++;

                                }

                                // Compensate for the incrementation at the end of the loop.
                                dataIndex--;
                            }
                        }
                    }
                }
            }

            // In theory this point is unreachable...
            return true;
        }

        public override bool Write(UInt32 address, byte[] data) {

			System.Diagnostics.Debug.WriteLine($"Writing {data.Length} bytes to address 0x{address:x8}");

            for(int dataIndex = 0; dataIndex < data.Length; dataIndex++) {

                for(int moduleIndex = 0; moduleIndex < maxModules; moduleIndex++) {

                    // See if this entry is valid.
                    if(metaValid[moduleIndex]) {

                        // See if the address is within this module's address range.
                        if(address >= meta[moduleIndex].Address && address <= (meta[moduleIndex].Address + meta[moduleIndex].Length)) {

                            // See if the request fits completely in this address space.
                            if((address + data.Length) <= (meta[moduleIndex].Address + meta[moduleIndex].Length)) {
                                return memoryModules[moduleIndex].Write(address % meta[moduleIndex].Length, data);
                            }

                            // Otherwise, we will break up the transfer.
                            else {

                                // First, see how many bytes we can written to this module.
                                UInt32 toWrite = (meta[moduleIndex].Address + meta[moduleIndex].Length) - address;
                                byte[] writeData = new byte[toWrite];

                                // Copy the write data from the data array:
                                for(int writing = 0; writing < toWrite; writing++) {
                                    writeData[writing] = data[dataIndex];
                                    dataIndex++;
                                }

                                // Write what we can to this module:
                                // FUTURE: Return on failure?
                                memoryModules[moduleIndex].Write(address % meta[moduleIndex].Length, writeData);

                                // Update the address and pointer.
                                address += toWrite;

                                // Compensate for the incrementation at the end of the loop.
                                dataIndex--;
                            }
                        }
                    }
                }
            }

            // In theory this point is unreachable...
            return true;

        }
    }
}
