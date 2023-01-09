﻿namespace FTG.Studios.BISC {

    /// <summary>
    /// Byte-encoded enum of each BISC register.
    /// </summary>
    public enum Register : byte {
        PC = 0x00, SP = 0x01, FP = 0x02, RA = 0x03, RV = 0x04, RI = 0x05, 
        INVALID0 = 0x06, INVALID1 = 0x07,
        R0 = 0x08, R1 = 0x09, R2 = 0x0A, R3 = 0x0B, R4 = 0x0C, R5 = 0x0D, R6 = 0x0E, R7 = 0x0F,
        T0 = 0x10, T1 = 0x11, T2 = 0x12, T3 = 0x13, T4 = 0x14, T5 = 0x15, T6 = 0x16, T7 = 0x17,
        F0 = 0x18, F1 = 0x19, F2 = 0x1A, F3 = 0x1B, F4 = 0x1C, F5 = 0x1D, F6 = 0x1E, F7 = 0x1F,
        FT0 = 0x20, FT1 = 0x21, FT2 = 0x22, FT3 = 0x23, FT4 = 0x24, FT5 = 0x25, FT6 = 0x26, FT7 = 0x27,
    }

    public static class RegisterExtensions {

        public static bool IsValid(this Register r) {
            return r != Register.INVALID0 && r != Register.INVALID1;
        }
    }
}
