using NUnit.Framework;
using System;
using FTG.Studios.BISC.Asm;

namespace FTG.Studios.BISC.Test
{

	[TestFixture]
	public class AssemblerTest
	{

		// TODO: Get rid of these functions and use the extension methods

		/// <summary>
		/// Assembles a 16-bit integer from two bytes supplied in little-endian order.
		/// </summary>
		/// <param name="a">Least significant byte.</param>
		/// <param name="b">Most significant byte.</param>
		/// <returns>A 16-bit integer in the endianness of the host machine.</returns>
		public static UInt16 AssembleInteger16(byte a, byte b)
		{
			if (!BitConverter.IsLittleEndian) return BitConverter.ToUInt16(new byte[] { b, a }, 0);
			return BitConverter.ToUInt16(new byte[] { a, b }, 0);
		}

		/// <summary>
		/// Assembles a 32-bit integer from four bytes supplied in little-endian order.
		/// </summary>
		/// <param name="a">Least significant byte.</param>
		/// <param name="b">Second byte.</param>
		/// <param name="c">Third byte.</param>
		/// <param name="d">Most significant byte.</param>
		/// <returns>A 16-bit integer in the endianness of the host machine.</returns>
		public static UInt32 AssembleInteger32(byte a, byte b, byte c, byte d)
		{
			if (!BitConverter.IsLittleEndian) return BitConverter.ToUInt32(new byte[] { d, c, b, a }, 0);
			return BitConverter.ToUInt32(new byte[] { a, b, c, d }, 0);
		}

		/// <summary>
		/// Disassembles a 16-bit integer into two bytes in little-endian order.
		/// </summary>
		/// <param name="value">16-bit integer.</param>
		/// <returns>A byte array of two bytes in little-endian order.</returns>
		public static byte[] DisassembleInteger16(UInt16 value)
		{
			byte[] bytes = BitConverter.GetBytes(value);
			if (!BitConverter.IsLittleEndian) Array.Reverse(bytes);
			return bytes;
		}

		/// <summary>
		/// Disassembles a 32-bit integer into four bytes in little-endian order.
		/// </summary>
		/// <param name="value">16-bit integer.</param>
		/// <returns>A byte array of four bytes in little-endian order.</returns>
		public static byte[] DisassembleInteger32(UInt32 value)
		{
			byte[] bytes = BitConverter.GetBytes(value);
			if (!BitConverter.IsLittleEndian) Array.Reverse(bytes);
			return bytes;
		}

		UInt32 AssembleInstruction(byte a, byte b, byte c, byte d)
		{
			return AssembleInteger32(a, b, c, d);
		}

		UInt32 AssembleInstruction(byte a, byte b, UInt16 c)
		{
			byte[] cd = DisassembleInteger16(c);
			return AssembleInteger32(a, b, cd[0], cd[1]);
		}

		const string file_name = "test";

		[SetUp]
		public void SetUp()
		{

		}

		[Test]
		public void AssembleInstruction()
		{
			UInt32[] expected = new UInt32[] { AssembleInstruction((byte)Opcode.NOP, 0, 0, 0) };
			UInt32[] actual = Assembler.Assemble("nop");
			Console.WriteLine($"Expected: 0x{expected[0]:x8}\nActual: 0x{actual[0]:x8}");
			Assert.IsNotNull(actual);
			Assert.AreEqual(expected, actual);
		}

		[Test]
		public void AssembleInstruction_Invalid()
		{
			Assert.Throws<ArgumentException>(() => Assembler.Assemble("this is an invalid instruction"));
		}

		#region Pseduo Instructions
		[Test]
		public void LDIp()
		{
			Register reg = Register.R0;
			UInt32 imm = 0xbabecafe;
			byte[] immb = Specification.DisassembleInteger32(imm);
			UInt32[] actual = Assembler.Assemble($"ldi {reg}, {imm}");

			UInt32[] expected = {
				AssembleInstruction((byte)Opcode.LLI, (byte)reg, immb[0], immb[1]),
				AssembleInstruction((byte)Opcode.LUI, (byte)reg, immb[2], immb[3])
			};

			Assert.AreEqual(expected, actual);
		}

		[Test]
		public void LRAp()
		{
			Register reg = Register.R0;
			UInt32 imm = 0xbabecafe;
			byte[] immb = Specification.DisassembleInteger32(imm);
			UInt32[] actual = Assembler.Assemble($"lra {reg}, {imm}");

			UInt32[] expected = {
				AssembleInstruction((byte)Opcode.LLI, (byte)reg, immb[0], immb[1]),
				AssembleInstruction((byte)Opcode.LUI, (byte)reg, immb[2], immb[3])
			};

			Assert.AreEqual(expected, actual);
		}

		[Test]
		public void SYSp()
		{
			UInt32 imm = 0xbabecafe;
			byte[] immb = Specification.DisassembleInteger32(imm);
			UInt32[] actual = Assembler.Assemble($"sys {imm}");

			UInt32[] expected = {
				AssembleInstruction((byte)Opcode.LLI, (byte)Register.RI, immb[0], immb[1]),
				AssembleInstruction((byte)Opcode.LUI, (byte)Register.RI, immb[2], immb[3]),
				AssembleInstruction((byte)Opcode.SYS, (byte)Register.RI, 0, 0)
			};

			Assert.AreEqual(expected, actual);
		}

		[Test]
		public void CALLp0()
		{
			UInt32 imm = 0xbabecafe;
			byte[] immb = Specification.DisassembleInteger32(imm);
			UInt32[] actual = Assembler.Assemble($"call {imm}");

			UInt32[] expected = {
				AssembleInstruction((byte)Opcode.LLI, (byte)Register.RI, immb[0], immb[1]),
				AssembleInstruction((byte)Opcode.LUI, (byte)Register.RI, immb[2], immb[3]),
				AssembleInstruction((byte)Opcode.CALL, (byte)Register.RI, 0, 0)
			};

			Assert.AreEqual(expected, actual);
		}

		[Test]
		public void CALLp1()
		{
			string label = "label";
			UInt32 imm = 0xc;
			byte[] immb = Specification.DisassembleInteger32(imm);
			UInt32[] actual = Assembler.Assemble($"call {label}\n{label}:\nnop");

			UInt32[] expected = {
				AssembleInstruction((byte)Opcode.LLI, (byte)Register.RI, immb[0], immb[1]),
				AssembleInstruction((byte)Opcode.LUI, (byte)Register.RI, immb[2], immb[3]),
				AssembleInstruction((byte)Opcode.CALL, (byte)Register.RI, 0, 0),
				AssembleInstruction((byte)Opcode.NOP, 0, 0, 0)
			};

			Assert.AreEqual(expected, actual);
		}
		#endregion

		#region System Instructions
		[Test]
		public void NOP()
		{
			UInt32[] expected = { AssembleInstruction((byte)Opcode.NOP, 0, 0, 0) };
			UInt32[] actual = Assembler.Assemble("nop");
			Assert.AreEqual(expected, actual);
		}
		#endregion
	}
}
