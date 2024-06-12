using System;

namespace FTG.Studios.BISC.Asm {
	
	public static partial class AssemblyNode {
		
		public abstract class Instruction : BlockItem {
			public readonly Opcode Opcode;
			
			public Instruction(Opcode opcode) {
				Opcode = opcode;
			}
		}
		
		public class NInstruction : Instruction {
			public NInstruction(Opcode opcode) : base(opcode) { }
			
			public override string ToString() {
				return $"{Opcode}()";
			}
		}
		
		public class RInstruction : Instruction {
			public readonly Register Operand;
			
			public RInstruction(Opcode opcode, Register operand) : base(opcode) {
				Operand = operand;
			}
			
			public override string ToString() {
				return $"{Opcode}({Operand})";
			}
		}
		
		public class IInstruction : Instruction {
			public readonly Register Destination;
			public readonly Constant Immediate;
			
			public IInstruction(Opcode opcode, Register destination, Constant immediate) : base(opcode) {
				Destination = destination;
				Immediate = immediate;
			}
			
			public override string ToString() {
				return $"{Opcode}({Destination}, {Immediate})";
			}
		}
		
		public class MInstruction : Instruction {
			public readonly Register Destination;
			public readonly Register Source;
			public readonly Constant Offset;
			
			public MInstruction(Opcode opcode, Register destination, Register source, Constant offset) : base(opcode) {
				Destination = destination;
				Source = source;
				Offset = offset;
			}
			
			public override string ToString() {
				return $"{Opcode}({Destination}, {Source}, {Offset})";
			}
		}
		
		public class DInstruction : Instruction {
			public readonly Register Destination;
			public readonly Register Operand;
			
			public DInstruction(Opcode opcode, Register destination, Register operand) : base(opcode) {
				Destination = destination;
				Operand = operand;
			}
			
			public override string ToString() {
				return $"{Opcode}({Destination}, {Operand})";
			}
		}
		
		public class TInstruction : Instruction {
			public readonly Register Destination;
			public readonly Register LeftOperand;
			public readonly Register RightOperand;
			
			public TInstruction(Opcode opcode, Register destination, Register left_operand, Register right_operand) : base(opcode) {
				Destination = destination;
				LeftOperand = left_operand;
				RightOperand = right_operand;
			}
			
			public override string ToString() {
				return $"{Opcode}({Destination}, {LeftOperand}, {RightOperand})";
			}
		}
	}
}