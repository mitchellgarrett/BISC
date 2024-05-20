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
			byte[] expected = new byte[] { (byte)Opcode.NOP, 0, 0, 0 };
			byte[] actual = Assembler.Assemble(file_name, "nop").ToBinary();

			Assert.IsNotNull(actual);
			Assert.AreEqual(expected, actual);
		}

		[Test]
		public void AssembleInstruction_Invalid_ThrowsException()
		{
			Assert.Throws<SyntaxErrorException>(() => Assembler.Assemble(file_name, "this is an invalid instruction"));
		}

		#region Pseduo Instructions
		[Test]
		public void Pseduo_LDI_To_LLI_LUI()
		{
			Register register = Register.R0;
			UInt32 immediate = 0xbabecafe;
			byte[] immediate_bytes = immediate.DisassembleUInt32();
			byte[] actual = Assembler.Assemble(file_name, $"ldi {register}, {immediate}").ToBinary();

			byte[] expected = {
				(byte)Opcode.LLI, (byte)register, immediate_bytes[0], immediate_bytes[1],
				(byte)Opcode.LUI, (byte)register, immediate_bytes[2], immediate_bytes[3]
			};

			Assert.AreEqual(expected, actual);
		}

		[Test]
		public void Pseudo_LRA_To_LLI_LUI()
		{
			Register register = Register.R0;
			UInt32 immediate = 0xbabecafe;
			byte[] immediate_bytes = immediate.DisassembleUInt32();
			byte[] actual = Assembler.Assemble(file_name, $"lra {register}, {immediate}").ToBinary();

			byte[] expected = {
				(byte)Opcode.LLI, (byte)register, immediate_bytes[0], immediate_bytes[1],
				(byte)Opcode.LUI, (byte)register, immediate_bytes[2], immediate_bytes[3]
			};

			Assert.AreEqual(expected, actual);
		}

		[Test]
		public void Pseudo_CALL_Immediate_To_LDI_Immediate_Call_Register()
		{
			UInt32 immediate = 0xbabecafe;
			byte[] immediate_bytes = immediate.DisassembleUInt32();
			byte[] actual = Assembler.Assemble(file_name, $"call {immediate}").ToBinary();

			byte[] expected = {
				(byte)Opcode.LLI, (byte)Register.TA, immediate_bytes[0], immediate_bytes[1],
				(byte)Opcode.LUI, (byte)Register.TA, immediate_bytes[2], immediate_bytes[3],
				(byte)Opcode.CALL, (byte)Register.TA, 0, 0
			};

			Assert.AreEqual(expected, actual);
		}

		[Test]
		public void Pseudo_CALL_Label_To_LDI_Immediate_Call_Register()
		{
			string label = "label";
			UInt32 address = 0x00000000;
			byte[] address_bytes = address.DisassembleUInt32();
			byte[] actual = Assembler.Assemble(file_name, $"{label}:\ncall {label}").ToBinary();

			byte[] expected = {
				(byte)Opcode.LLI, (byte)Register.TA, address_bytes[0], address_bytes[1],
				(byte)Opcode.LUI, (byte)Register.TA, address_bytes[2], address_bytes[3],
				(byte)Opcode.CALL, (byte)Register.TA, 0, 0
			};

			Assert.AreEqual(expected, actual);
		}
		#endregion

		#region System Instructions
		[Test]
		public void Op_NOP()
		{
			byte[] expected = { (byte)Opcode.NOP, 0, 0, 0 };
			byte[] actual = Assembler.Assemble(file_name, "nop").ToBinary();
			Assert.AreEqual(expected, actual);
		}
		#endregion
	}
}
