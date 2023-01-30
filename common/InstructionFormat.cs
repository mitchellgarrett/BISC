namespace FTG.Studios.BISC {

    /// <summary>
    /// Possible BISC instruction formats.
    /// </summary>
    public enum InstructionFormat {
        /// <summary>
        /// Instruction format.
        /// </summary>
        N,

        /// <summary>
        /// Register format.
        /// </summary>
        R,

        /// <summary>
        /// Register-immediate format.
        /// </summary>
        I,

        /// <summary>
        /// Memory format.
        /// </summary>
        M,
        
        /// <summary>
        /// Double register format.
        /// </summary>
        D, 

        /// <summary>
        /// Triple register format.
        /// </summary>
        T
    }
}