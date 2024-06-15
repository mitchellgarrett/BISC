using System;
using System.Collections.Generic;

namespace FTG.Studios.BISC.Asm {
	
	public static partial class CodeGenerator {
		
		public static byte[] AssembleBinary(this AssemblyTree program) {
			List<byte> data = new List<byte>();
			foreach (var section in program.Sections) data.AddRange(AssembleSection(section));
			return data.ToArray();
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
			throw new SyntaxErrorException($"Invalid constant '{constant}'");
		}
		
		static byte[] AssembleLinkerRelocation(AssemblyNode.LinkerRelocation relocation) {
			byte[] value = AssembleConstant(relocation.Value);

			return relocation.Type switch {
				// Return lower 16 bits of immediate
				AssemblyNode.LinkerRelocation.RelocationType.Lo => new byte[] { value[0], value[1], 0, 0 },
				// Return upper 16 bits of immediate
				AssemblyNode.LinkerRelocation.RelocationType.Hi => new byte[] { value[2], value[3], 0, 0 },
				
				_ => throw new SyntaxErrorException($"Invalid linker directive '{relocation}'")
			};
		}
		
		static byte[] AssembleDataInitializer(AssemblyNode.DataInitializer initializer) {
			return initializer.Data;
		}
	}
}