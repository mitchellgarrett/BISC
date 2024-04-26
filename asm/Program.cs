using System.Collections;
using System.Collections.Generic;

namespace FTG.Studios.BISC.Asm
{

	public class Program
	{

		readonly List<Assembloid> assembloids;

		public int SizeInBytes;

		public Program()
		{
			assembloids = new List<Assembloid>();
		}

		public int Count { get { return assembloids.Count; } }

		public Assembloid this[int index]
		{
			get => assembloids[index];
			set => assembloids[index] = value;
		}

		public void Add(Assembloid assembloid)
		{
			if (assembloid is Instruction) SizeInBytes += 4;
			if (assembloid is Binary) SizeInBytes += (assembloid as Binary).Data.Length;
			assembloids.Add(assembloid);
		}

		public Label GetLabel(string identifer)
		{
			foreach (Assembloid assembloid in assembloids)
			{
				if (assembloid is Label)
				{
					Label label = assembloid as Label;
					if (label.Identifier == identifer) return label;
				}
			}
			return null;
		}
	}
}