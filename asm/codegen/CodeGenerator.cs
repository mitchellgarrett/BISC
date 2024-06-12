using System;
using System.Collections.Generic;
using FTG.Studios.BEEF;

namespace FTG.Studios.BISC.Asm {
	
	public static class CodeGenerator {
		
		public static ObjectFile AssembleBEEF(AssemblyTree program) {
			SectionHeader[] section_headers = new SectionHeader[1];
			
			byte[][] section_data = new byte[1][];
			
			List<byte> current_section_data = new List<byte>();
			foreach (var item in program.Program.Body) {
				current_section_data.AddRange(AssembleBlockItem(item));
			}
			
			section_data[0] = current_section_data.ToArray();
			
			section_headers[0] = new SectionHeader() {
				Type = SectionType.Program,
				Flags = SectionFlag.Readable | SectionFlag.Writable | SectionFlag.Executable | SectionFlag.Code,
				Offset = FileHeader.SizeInBytes + SectionHeader.SizeInBytes,
				Address = 0,
				Size = (UInt32)section_data[0].Length,
				Name = ".text"
			};
			
			FileHeader file_header = new FileHeader() {
				HeaderBegin = FileHeader.MAGIC_NUMBER,
				Architecture = 0xb,
				Endianness = Endianness.Little,
				EntryPoint = 0,
				SectionTableOffset = FileHeader.SizeInBytes,
				SectionCount = 1,
				HeaderEnd = FileHeader.MAGIC_NUMBER
			};

			return  new ObjectFile {
				FileHeader = file_header,
				SectionHeaders = section_headers,
				SectionData = section_data
			};
		}
		
		static byte[] AssembleBlockItem(AssemblyNode.BlockItem item) {
			if (item is AssemblyNode.Instruction instruction) return AssembleInstruction(instruction);
			return new byte[] { };
		}
		
		static byte[] AssembleInstruction(AssemblyNode.Instruction instruction) {
			if (instruction is AssemblyNode.NInstruction n) return AssembleNInstruction(n);
			if (instruction is AssemblyNode.RInstruction r) return AssembleRInstruction(r);
			if (instruction is AssemblyNode.IInstruction i) return AssembleIInstruction(i);
			if (instruction is AssemblyNode.MInstruction m) return AssembleMInstruction(m);
			if (instruction is AssemblyNode.DInstruction d) return AssembleDInstruction(d);
			if (instruction is AssemblyNode.TInstruction t) return AssembleTInstruction(t);
			return new byte[] { };
		}
		
		static byte[] AssembleNInstruction(AssemblyNode.NInstruction instruction) {
			return new byte[] { (byte)instruction.Opcode, 0, 0, 0 };
		}
		
		static byte[] AssembleRInstruction(AssemblyNode.RInstruction instruction) {
			return new byte[] { (byte)instruction.Opcode, (byte)instruction.Operand, 0, 0 };
		}
		
		static byte[] AssembleIInstruction(AssemblyNode.IInstruction instruction) {
			byte[] immediate = AssembleConstant(instruction.Immediate);
			if (immediate[2] != 0 || immediate[3] != 0) throw new ArgumentException("TODO: Immediate should be 16 bits");
			
			return new byte[] { (byte)instruction.Opcode, (byte)instruction.Destination, immediate[0], immediate[1] };
		}
		
		static byte[] AssembleMInstruction(AssemblyNode.MInstruction instruction) {
			byte[] offset = AssembleConstant(instruction.Offset);
			if (offset[1] != 0 || offset[2] != 0 || offset[3] != 0) throw new ArgumentException("TODO: Offset should be 8 bits");
			
			return new byte[] { (byte)instruction.Opcode, (byte)instruction.Destination, (byte)instruction.Source, offset[0] };
		}
		
		static byte[] AssembleDInstruction(AssemblyNode.DInstruction instruction) {
			return new byte[] { (byte)instruction.Opcode, (byte)instruction.Destination, (byte)instruction.Operand, 0 };
		}
		
		static byte[] AssembleTInstruction(AssemblyNode.TInstruction instruction) {
			return new byte[] { (byte)instruction.Opcode, (byte)instruction.Destination, (byte)instruction.LeftOperand, (byte)instruction.RightOperand };
		}
		
		static byte[] AssembleConstant(AssemblyNode.Constant constant) {
			if (constant is AssemblyNode.Immediate immediate) return immediate.Value.DisassembleUInt32();
			if (constant is AssemblyNode.LinkerRelocation relocation) return AssembleLinkerRelocation(relocation);
			throw new InvalidOperationException("TODO: AssmebleConstant did not work");
		}
		
		static byte[] AssembleLinkerRelocation(AssemblyNode.LinkerRelocation relocation) {
			byte[] value = AssembleConstant(relocation.Constant);

			return relocation.Type switch {
				// Return lower 16 bits of immediate
				AssemblyNode.LinkerRelocation.RelocationType.Lo => new byte[] { value[0], value[1], 0, 0 },
				// Return upper 16 bits of immediate
				AssemblyNode.LinkerRelocation.RelocationType.Hi => new byte[] { value[2], value[3], 0, 0 },
				
				_ => throw new InvalidOperationException("TODO: AssembleLinkerRelocation did not work"),
			};
		}
	}
}