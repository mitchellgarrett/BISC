using NUnit.Framework;
using System;
using FTG.Studios.BISC.VM;

namespace FTG.Studios.BISC.Test {

    [TestFixture]
    public class TerminalTest {

        Terminal terminal;
        UInt32 address_start, address_end, address_length;
        int width, height;

        [SetUp]
        public void SetUp() {
            width = 80;
            height = 24;
            address_start = 0x1000;
            address_length = (UInt32)(width * height + 3);
            address_end = address_start + address_length;
            Assert.Throws(typeof(System.IO.IOException), delegate {
                terminal = new Terminal(address_start, width, height);
            });
        }

        [Test]
        public void Reset() {
            Assert.IsNull(terminal);
            //MemoryTest.TrivialReset(terminal, address_start, address_end);
        }

        [Test]
        public void Read() {
            Assert.IsNull(terminal);
            //MemoryTest.TrivialRead(terminal, address_start, address_end);
        }

        [Test]
        public void Write() {
            Assert.IsNull(terminal);
            //MemoryTest.TrivialWrite(terminal, address_start, address_end);
        }
    }
}
