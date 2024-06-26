using System;

namespace FTG.Studios.BISC.VM
{

    // Returns a random 32-bit integer on read
    // Memory map: 0x00, 0x04
    public class RandomNumberGenerator : MemoryModule {

		readonly UInt32 SeedAddress;
		readonly UInt32 ValueAddress;
		Random rng;

        public RandomNumberGenerator() : base() {
			SeedAddress = 0;
			ValueAddress = SeedAddress + 4;
			AddressLength = 8;
			rng = new Random();
		}

		public override void Reset() { }

		public override bool Read(UInt32 address, ref byte[] data)
		{
			if (address == SeedAddress)
			{
				// TODO: Read seed value
				return true;
			}

			if (address == ValueAddress)
			{
				rng.NextBytes(data);
				return true;
			}
			return false;
		}

		public override bool Write(UInt32 address, byte[] data)
		{
			if (address == SeedAddress)
			{
				rng = new Random((int)(data[0], data[1], data[2], data[3]).AssembleUInt32());
				return true;
			}

			if (address == ValueAddress) return true;
			return false;
		}
	}
}
