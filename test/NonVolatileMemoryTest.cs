using NUnit.Framework;
using System;
using FTG.Studios.BISC.VM;

namespace FTG.Studios.BISC.Test {

    [TestFixture]
    public class NonVolatileMemoryTest {

        NonVolatileMemory memory;
        UInt32 address_length;

        [SetUp]
        public void SetUp() {
            address_length = 0x1000;
            memory = new NonVolatileMemory(address_length);
        }

        [Test]
        public void Reset() {
            MemoryTest.NonZeroReset(memory, 0, address_length);
        }

        [Test]
        public void Read() {
            MemoryTest.TrivialRead(memory, 0, address_length);
        }

        [Test]
        public void Write() {
            MemoryTest.TrivialWrite(memory, 0, address_length);
        }
    }
}
