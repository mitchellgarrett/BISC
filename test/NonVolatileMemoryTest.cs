using NUnit.Framework;
using System;
using FTG.Studios.BISC.VM;

namespace FTG.Studios.BISC.Test {

    [TestFixture]
    public class NonVolatileMemoryTest {

        NonVolatileMemory memory;
        UInt32 address_start, address_end, address_length;

        [SetUp]
        public void SetUp() {
            address_start = 0x1000;
            address_length = 0x1000;
            address_end = address_start + address_length;
            memory = new NonVolatileMemory(address_start, address_length);
        }

        [Test]
        public void Reset() {
            MemoryTest.NonZeroReset(memory, address_start, address_end);
        }

        [Test]
        public void Read() {
            MemoryTest.TrivialRead(memory, address_start, address_end);
        }

        [Test]
        public void Write() {
            MemoryTest.TrivialWrite(memory, address_start, address_end);
        }
    }
}
