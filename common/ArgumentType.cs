namespace FTG.Studios.BISC {
    
    /// <summary>
    /// Possible argument types for instructions.
    /// </summary>
    public enum ArgumentType { None, Register, Memory, Symbol, Immediate16, Immediate32 };
	
	public static class ArgumentTypeExtensions {
		public static bool IsEquivalent(this ArgumentType t, ArgumentType other) {
			return t == other || ((t == ArgumentType.Symbol || t == ArgumentType.Immediate16 || t == ArgumentType.Immediate32) && (other == ArgumentType.Symbol || other == ArgumentType.Immediate16 || other == ArgumentType.Immediate32));
		}
	}
}