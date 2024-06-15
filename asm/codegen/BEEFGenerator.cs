using System;
using System.Collections.Generic;
using FTG.Studios.BEEF;

namespace FTG.Studios.BISC.Asm {
	
	public static partial class CodeGenerator {
		
		// TODO: Make this better
		public static ObjectFile AssembleBEEF(this AssemblyTree program) {
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
				AssemblyNode.Section section = program.Sections[i];
				
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
	}
}