namespace FTG.Studios.BISC.Asm
{

	public class Binary : Assembloid
	{

		public byte[] Data;

		public Binary(byte[] data)
		{
			Data = data;
		}

		public override bool HasUndefinedSymbol()
		{
			return false;
		}
	}
}