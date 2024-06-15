namespace FTG.Studios.BISC.VM
{

	public class Computer
	{

		VirtualMachine vm;
		MemoryManager mmu;

		public Computer()
		{
			mmu = new MemoryManager(32, 0xFFFF_0000);

			VolatileMemory ram = new VolatileMemory(0x4000);
			mmu.AddModule(ram, 0);

			vm = new VirtualMachine(mmu);
		}
	}
}