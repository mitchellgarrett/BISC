namespace FTG.Studios.BISC {

    /// <summary>
    /// Byte-encoded value of each system call.
    /// </summary>
    public enum SystemCall : byte {
        CLS = 0x01, 
        SCS = 0x02, 
        SCB = 0x03, 
        SCP = 0x04, 
        SBG = 0x05, 
        SFG = 0x06, 
        WCHR = 0x07, 
        WSTR = 0x08, 
        WPXL = 0x09, 
        RCHR = 0x0A, 
        RMP = 0x0B, 
        RMBP = 0x0C, 
        RMBR = 0x0D
    }
}