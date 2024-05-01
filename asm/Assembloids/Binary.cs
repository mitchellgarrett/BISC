namespace FTG.Studios.BISC.Asm
{

	public class Binary : Assembloid
	{

		public byte[] Data;

		public Binary(byte[] data)
		{
			Data = data;
			Size = Data.Length;
			HasUndefinedSymbol = false;
		}

		public override byte[] Assemble()
		{
			return Data;
		}

		public override string ToString()
		{
			string output = "Binary: ";
			foreach (var b in Data) output += $"{b:x2} ";
			return output;
		}
	}
}