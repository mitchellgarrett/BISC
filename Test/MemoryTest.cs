using NUnit.Framework;
using System;
using FTG.Studios.BISC.VM;

namespace FTG.Studios.BISC.Test {

    public class MemoryTest {

        public static void TrivialReset(MemoryModule memory, UInt32 address_start, UInt32 address_end) {
            Assert.NotNull(memory);

            UInt32 expected = 0xffff;
            UInt32 actual = 0;

            // Write value
            byte[] data = BitConverter.GetBytes(expected);
            Assert.NotNull(data);
            Assert.IsTrue(memory.Write(address_start, data));

            // Reset byte array
            data = new byte[sizeof(UInt32)];
            Assert.NotNull(data);

            // Read back value, it should be the same
            Assert.IsTrue(memory.Read(address_start, ref data));
            Assert.NotNull(data);
            Assert.DoesNotThrow(delegate { actual = BitConverter.ToUInt32(data); });
            Assert.AreEqual(expected, actual);

            // Reset memory
            memory.Reset();

            // Reset byte array
            data = new byte[sizeof(UInt32)];
            Assert.NotNull(data);

            // Read from same address, it should be zero
            Assert.IsTrue(memory.Read(address_start, ref data));
            Assert.NotNull(data);
            Assert.DoesNotThrow(delegate { actual = BitConverter.ToUInt32(data); });
            Assert.AreEqual(0, actual);
        }

        public static void NonZeroReset(MemoryModule memory, UInt32 address_start, UInt32 address_end) {
            Assert.NotNull(memory);

            UInt32 expected = 0xffff;
            UInt32 actual = 0;

            // Write value
            byte[] data = BitConverter.GetBytes(expected);
            Assert.NotNull(data);
            Assert.IsTrue(memory.Write(address_start, data));

            // Reset byte array
            data = new byte[sizeof(UInt32)];
            Assert.NotNull(data);

            // Read back value, it should be the same
            Assert.IsTrue(memory.Read(address_start, ref data));
            Assert.NotNull(data);
            Assert.DoesNotThrow(delegate { actual = BitConverter.ToUInt32(data); });
            Assert.AreEqual(expected, actual);

            // Reset memory
            memory.Reset();

            // Reset byte array
            data = new byte[sizeof(UInt32)];
            Assert.NotNull(data);

            // Read from same address, it should still be the same
            Assert.IsTrue(memory.Read(address_start, ref data));
            Assert.NotNull(data);
            Assert.DoesNotThrow(delegate { actual = BitConverter.ToUInt32(data); });
            Assert.AreEqual(expected, actual);
        }

        public static void TrivialRead(MemoryModule memory, UInt32 address_start, UInt32 address_end) {
            Assert.NotNull(memory);

            UInt32 expected = 0x0000;
            UInt32 actual = 0xffff;

            // Reset byte array
            byte[] data = new byte[sizeof(UInt32)];
            Assert.NotNull(data);

            // Check that initial value is zero
            Assert.IsTrue(memory.Read(address_start, ref data));
            Assert.DoesNotThrow(delegate { actual = BitConverter.ToUInt32(data); });
            Assert.AreEqual(0, actual);

            // Do some writes
            expected = 0xbabecafe;
            data = BitConverter.GetBytes(expected);
            Assert.IsTrue(memory.Write(address_start, data));
            Assert.IsTrue(memory.Write(address_end - 4, data));

            // Reset byte array
            data = new byte[sizeof(UInt32)];
            Assert.NotNull(data);

            // Read back written values, they should be the same
            Assert.IsTrue(memory.Read(address_start, ref data));
            Assert.DoesNotThrow(delegate { actual = BitConverter.ToUInt32(data); });
            Assert.NotNull(data);
            Assert.AreEqual(expected, actual);

            Assert.IsTrue(memory.Read(address_end - 4, ref data));
            Assert.DoesNotThrow(delegate { actual = BitConverter.ToUInt32(data); });
            Assert.NotNull(data);
            Assert.AreEqual(expected, actual);
        }

        public static void TrivialWrite(MemoryModule memory, UInt32 address_start, UInt32 address_end) {
            Assert.NotNull(memory);

            // Write values to address boundaries
            UInt32 expected0 = 0xbabecafe;
            byte[] data = BitConverter.GetBytes(expected0);
            Assert.IsTrue(memory.Write(address_start, data));

            UInt32 expected1 = 0xdeadbeef;
            data = BitConverter.GetBytes(expected1);
            Assert.IsTrue(memory.Write(address_end - 4, data));

            // Reset byte array
            data = new byte[sizeof(UInt32)];
            Assert.NotNull(data);

            // Read back written values, they should be the same
            UInt32 actual = 0x0000;
            Assert.IsTrue(memory.Read(address_start, ref data));
            Assert.DoesNotThrow(delegate { actual = BitConverter.ToUInt32(data); });
            Assert.NotNull(data);
            Assert.AreEqual(expected0, actual);

            Assert.IsTrue(memory.Read(address_end - 4, ref data));
            Assert.DoesNotThrow(delegate { actual = BitConverter.ToUInt32(data); });
            Assert.NotNull(data);
            Assert.AreEqual(expected1, actual);

            // Writes outside of address range should fail
            Assert.IsFalse(memory.Write(address_start - 1, data));
            Assert.IsFalse(memory.Write(address_end, data));
        }
    }
}
