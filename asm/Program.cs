﻿using System.Collections.Generic;

namespace FTG.Studios.BISC.Asm {

    public class Program {

        public List<Instruction> Instructions;
        public Dictionary<string, Instruction> Labels;

        public Program() {
            Instructions = new List<Instruction>();
            Labels = new Dictionary<string, Instruction>();
        }
    }
}
