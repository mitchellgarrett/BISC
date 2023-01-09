namespace FTG.Studios.BISC {

    /// <summary>
    /// Possible BISC instruction formats.
    /// </summary>
    public enum InstructionFormat {
        /// <summary>
        /// Instruction format.
        /// </summary>
        I,

        /// <summary>
        /// Register format.
        /// </summary>
        R,

        /// <summary>
        /// Register-immediate format.
        /// </summary>
        RI,

        /// <summary>
        /// Memory format.
        /// </summary>
        M,
        
        /// <summary>
        /// Register-destination format.
        /// </summary>
        RD, 

        /// <summary>
        /// Register-register-destination format.
        /// </summary>
        RRD
    }
}