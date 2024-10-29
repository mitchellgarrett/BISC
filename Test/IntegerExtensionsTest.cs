using NUnit.Framework;
using System;

namespace FTG.Studios.BISC.Test
{

	[TestFixture]
	public class IntegerExtensionsTest
	{

		[Test]
		public void AssembleUInt16()
		{
			UInt16 expected = 0xcafe;
			byte[] expected_bytes = { 0xfe, 0xca };

			UInt16 actual = (expected_bytes[0], expected_bytes[1]).AssembleUInt16();
			Assert.AreEqual(expected, actual);
		}

		[Test]
		public void AssembleUInt32()
		{
			UInt32 expected = 0xbabecafe;
			byte[] expected_bytes = { 0xfe, 0xca, 0xbe, 0xba };

			UInt32 actual = (expected_bytes[0], expected_bytes[1], expected_bytes[2], expected_bytes[3]).AssembleUInt32();
			Assert.AreEqual(expected, actual);
		}

		[Test]
		public void DisassembleUInt16()
		{
			UInt16 expected = 0xcafe;
			byte[] expected_bytes = { 0xfe, 0xca };

			byte[] actual = expected.DisassembleUInt16();
			Assert.AreEqual(expected_bytes, actual);
		}

		[Test]
		public void DisassembleUInt32()
		{
			UInt32 expected = 0xbabecafe;
			byte[] expected_bytes = { 0xfe, 0xca, 0xbe, 0xba };

			byte[] actual = expected.DisassembleUInt32();
			Assert.AreEqual(expected_bytes, actual);
		}

		[Test]
		public void DisassembleUInt16_From_UInt32() {
			UInt32 expected = 0xbabecafe;
			byte[] expected_bytes = { 0xfe, 0xca };

			byte[] actual = expected.DisassembleUInt16();
			Assert.AreEqual(expected_bytes, actual);
		}

		[Test]
		public void AssembleAndDisassembleInteger16()
		{
			UInt16 expected = 0xcafe;
			byte[] expected_bytes = { 0xfe, 0xca };

			UInt16 actual = (expected_bytes[0], expected_bytes[1]).AssembleUInt16();
			Assert.AreEqual(expected, actual);

			byte[] actual_bytes = expected.DisassembleUInt16();
			Assert.AreEqual(expected_bytes, actual_bytes);
		}

		[Test]
		public void AssembleAndDisassembleInteger32()
		{
			UInt32 expected = 0xbabecafe;
			byte[] expected_bytes = { 0xfe, 0xca, 0xbe, 0xba };

			UInt32 actual = (expected_bytes[0], expected_bytes[1], expected_bytes[2], expected_bytes[3]).AssembleUInt32();
			Assert.AreEqual(expected, actual);

			byte[] actual_bytes = expected.DisassembleUInt32();
			Assert.AreEqual(expected_bytes, actual_bytes);
		}
	}
}
