using NUnit.Framework;
using System;

namespace FTG.Studios.BISC.Test {

    [TestFixture]
    public class SpecificationTest {

        [Test]
        public void AssembleInteger16() {
            UInt16 expected = 0xcafe;
            byte[] expected_bytes = { 0xfe, 0xca };

            UInt16 actual = Specification.AssembleInteger16(expected_bytes[0], expected_bytes[1]);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void AssembleInteger32() {
            UInt32 expected = 0xbabecafe;
            byte[] expected_bytes = { 0xfe, 0xca, 0xbe, 0xba };

            UInt32 actual = Specification.AssembleInteger32(expected_bytes[0], expected_bytes[1], expected_bytes[2], expected_bytes[3]);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void DisassembleInteger16() {
            UInt16 expected = 0xcafe;
            byte[] expected_bytes = { 0xfe, 0xca };

            byte[] actual = Specification.DisassembleInteger16(expected);
            Assert.AreEqual(expected_bytes, actual);
        }

        [Test]
        public void DisassembleInteger32() {
            UInt32 expected = 0xbabecafe;
            byte[] expected_bytes = { 0xfe, 0xca, 0xbe, 0xba };

            byte[] actual = Specification.DisassembleInteger32(expected);
            Assert.AreEqual(expected_bytes, actual);
        }

        [Test]
        public void AssembleAndDisassembleInteger16() {
            UInt16 expected = 0xcafe;
            byte[] expected_bytes = { 0xfe, 0xca };

            UInt16 actual = Specification.AssembleInteger16(expected_bytes[0], expected_bytes[1]);
            Assert.AreEqual(expected, actual);

            byte[] actual_bytes = Specification.DisassembleInteger16(expected);
            Assert.AreEqual(expected_bytes, actual_bytes);
        }

        [Test]
        public void AssembleAndDisassembleInteger32() {
            UInt32 expected = 0xbabecafe;
            byte[] expected_bytes = { 0xfe, 0xca, 0xbe, 0xba };

            UInt32 actual = Specification.AssembleInteger32(expected_bytes[0], expected_bytes[1], expected_bytes[2], expected_bytes[3]);
            Assert.AreEqual(expected, actual);

            byte[] actual_bytes = Specification.DisassembleInteger32(expected);
            Assert.AreEqual(expected_bytes, actual_bytes);
        }
    }
}
