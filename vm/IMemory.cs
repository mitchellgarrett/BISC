using System;
using System.Collections.Generic;

namespace FTG.Studios.BISC {

	/// <summary>
	/// BISC memory element.
	/// </summary>
	public interface IMemory {
		void Reset();
		UInt32 Read(UInt32 address);
		bool Write(UInt32 address, UInt32 data);
	}

	public class Memory : IMemory {

		Dictionary<UInt32, UInt32> memory;

		public Memory() {
			memory = new Dictionary<UInt32, UInt32>();
		}

#region IMemory
		void IMemory.Reset() { memory.Clear(); }

		UInt32 IMemory.Read(UInt32 address) { return memory[address]; }

		bool IMemory.Write(UInt32 address, UInt32 data) { 
			memory[address] = data;
			return true;
		}
#endregion

	}

}
