using System;

namespace FTG.Studios.BISC {

    public class Section {

        public string Name;
        public UInt32 Offset;
        public UInt32[] Instructions;

        public Section(UInt32 offset, UInt32[] instructions) {
            this.Name = "Section";
            this.Offset = offset;
            this.Instructions = instructions;
        }
    }
}
