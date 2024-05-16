namespace FTG.Studios.BISC.VM
{

	public class Computer
	{

		VirtualMachine vm;
		MemoryManager mmu;

		public Computer()
		{
			mmu = new MemoryManager();

			VolatileMemory ram = new VolatileMemory(0, 0x4000);
			mmu.AddDevice(ram);

			vm = new VirtualMachine(mmu);
		}
	}
}