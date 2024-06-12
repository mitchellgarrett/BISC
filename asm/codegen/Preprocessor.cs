using System.Collections.Generic;

namespace FTG.Studios.BISC.Asm {
	
	public static class Preprocessor {
		
		public static void PartitionSections(AssemblyTree program) {
			// List of sections that have been split up
			List<AssemblyNode.BlockItem> partitioned_program = new List<AssemblyNode.BlockItem>();
			
			// List of BlockItems in current segment
			List<AssemblyNode.BlockItem> current_section = new List<AssemblyNode.BlockItem>();
			
			// Loop over items in program
			AssemblyNode.SectionDefinition previous_section_definition = null;
			foreach (var item in program.Body) {
				// If the item is a SectionDefinition, add it to the partioned list
				if (item is AssemblyNode.SectionDefinition current_section_definition) {
					if (previous_section_definition == null) {
						partitioned_program.AddRange(current_section);
					} else {
						partitioned_program.Add(new AssemblyNode.Section(previous_section_definition.Identifier, current_section));
					}
					previous_section_definition = current_section_definition;
					current_section = new List<AssemblyNode.BlockItem>();
				} else {
					current_section.Add(item);
				}
			}
			
			if (previous_section_definition != null) {
				partitioned_program.Add(new AssemblyNode.Section(previous_section_definition.Identifier, current_section));
			}
			
			// Assign the partioned sections to the program body
			program.Body = partitioned_program;
		}
	}
}