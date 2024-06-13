using System;
using System.Collections.Generic;
using FTG.Studios.BEEF;

namespace FTG.Studios.BISC.Asm {
	
	public static class CodeGenerator {
		
		// TODO: Make this better
		public static ObjectFile AssembleBEEF(AssemblyTree program) {
			AssembleProgram(program, out SectionHeader[] section_headers, out byte[][] section_data);
			
			FileHeader file_header = new FileHeader() {
				HeaderBegin = FileHeader.MAGIC_NUMBER,
				Architecture = 0xb,
				Endianness = Endianness.Little,
				EntryPoint = section_headers[0].Address,
				SectionTableOffset = FileHeader.SizeInBytes,
				SectionCount = (UInt16)section_headers.Length,
				HeaderEnd = FileHeader.MAGIC_NUMBER
			};

			return  new ObjectFile {
				FileHeader = file_header,
				SectionHeaders = section_headers,
				SectionData = section_data
			};
		}
		
		static void AssembleProgram(AssemblyTree program, out SectionHeader[] section_headers, out byte[][] section_data) {
			// TODO: This assumets everything in the program is in a section
			section_headers = new SectionHeader[program.Sections.Count];
			section_data = new byte[program.Sections.Count][];
			
			UInt32 offset = (UInt32)(FileHeader.SizeInBytes + SectionHeader.SizeInBytes * section_headers.Length);
			UInt32 address = 0;
			
			for (int i = 0; i < program.Sections.Count; i++) {
				AssemblyNode.Section section = program.Sections[i] as AssemblyNode.Section;
				
				section_data[i] = AssembleSection(section);
				UInt32 section_size = (UInt32)section_data[i].Length;
				
				section_headers[i] = new SectionHeader() {
					Type = SectionType.Program,
					Flags = SectionFlag.Readable | SectionFlag.Writable | SectionFlag.Executable | SectionFlag.Code,
					Offset = offset,
					Address = address,
					Size = section_size,
					Name = section.Identifier
				};
				
				offset += section_size;
				address += section_size;
			}
		}
		
		static byte[] AssembleSection(AssemblyNode.Section section) {
			List<byte> section_data = new List<byte>();
			foreach (var item in section.Body) {
				section_data.AddRange(AssembleBlockItem(item));
			}
			return section_data.ToArray();
		}
		
		static byte[] AssembleBlockItem(AssemblyNode.BlockItem item) {
			if (item is AssemblyNode.Instruction instruction) return AssembleInstruction(instruction);
			if (item is AssemblyNode.DataInitializer initializer) return AssembleDataInitializer(initializer);
			return new byte[] { };
		}
		
		static byte[] AssembleInstruction(AssemblyNode.Instruction instruction) {
			if (instruction is AssemblyNode.NInstruction n) return AssembleNInstruction(n);
			if (instruction is AssemblyNode.RInstruction r) return AssembleRInstruction(r);
			if (instruction is AssemblyNode.IInstruction i) return AssembleIInstruction(i);
			if (instruction is AssemblyNode.MInstruction m) return AssembleMInstruction(m);
			if (instruction is AssemblyNode.DInstruction d) return AssembleDInstruction(d);
			if (instruction is AssemblyNode.TInstruction t) return AssembleTInstruction(t);
			throw new InvalidOperationException($"TODO: AssembleInstruction did not work {instruction.GetType()}");
		}
		
		static byte[] AssembleNInstruction(AssemblyNode.NInstruction instruction) {
			return new byte[] { (byte)instruction.Opcode, 0, 0, 0 };
		}
		
		static byte[] AssembleRInstruction(AssemblyNode.RInstruction instruction) {
			return new byte[] { (byte)instruction.Opcode, (byte)instruction.Operand.Type, 0, 0 };
		}
		
		static byte[] AssembleIInstruction(AssemblyNode.IInstruction instruction) {
			byte[] immediate = AssembleConstant(instruction.Immediate);
			if (immediate[2] != 0 || immediate[3] != 0) throw new ArgumentException("TODO: Immediate should be 16 bits");
			
			return new byte[] { (byte)instruction.Opcode, (byte)instruction.Destination.Type, immediate[0], immediate[1] };
		}
		
		static byte[] AssembleMInstruction(AssemblyNode.MInstruction instruction) {
			byte[] offset = AssembleConstant(instruction.Offset);
			if (offset[1] != 0 || offset[2] != 0 || offset[3] != 0) throw new ArgumentException("TODO: Offset should be 8 bits");
			
			return new byte[] { (byte)instruction.Opcode, (byte)instruction.Destination.Type, (byte)instruction.Source.Type, offset[0] };
		}
		
		static byte[] AssembleDInstruction(AssemblyNode.DInstruction instruction) {
			return new byte[] { (byte)instruction.Opcode, (byte)instruction.Destination.Type, (byte)instruction.Operand.Type, 0 };
		}
		
		static byte[] AssembleTInstruction(AssemblyNode.TInstruction instruction) {
			return new byte[] { (byte)instruction.Opcode, (byte)instruction.Destination.Type, (byte)instruction.LeftOperand.Type, (byte)instruction.RightOperand.Type };
		}
		
		static byte[] AssembleConstant(AssemblyNode.Constant constant) {
			if (constant is AssemblyNode.Immediate immediate) return immediate.Value.DisassembleUInt32();
			if (constant is AssemblyNode.LinkerRelocation relocation) return AssembleLinkerRelocation(relocation);
			throw new InvalidOperationException($"TODO: AssmebleConstant did not work {constant.GetType()}");
		}
		
		static byte[] AssembleLinkerRelocation(AssemblyNode.LinkerRelocation relocation) {
			byte[] value = AssembleConstant(relocation.Value);

			return relocation.Type switch {
				// Return lower 16 bits of immediate
				AssemblyNode.LinkerRelocation.RelocationType.Lo => new byte[] { value[0], value[1], 0, 0 },
				// Return upper 16 bits of immediate
				AssemblyNode.LinkerRelocation.RelocationType.Hi => new byte[] { value[2], value[3], 0, 0 },
				
				_ => throw new InvalidOperationException("TODO: AssembleLinkerRelocation did not work"),
			};
		}
		
		static byte[] AssembleDataInitializer(AssemblyNode.DataInitializer initializer) {
			return initializer.Data;
		}
	}
}