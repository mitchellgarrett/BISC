using System;
using System.Linq;

namespace FTG.Studios.BISC.Asm
{

	public static class IntegerExtensions
	{

		/// <summary>
		/// Assembles a 16-bit integer from two bytes supplied in little-endian order.
		/// </summary>
		/// <param name="bytes">Byte array of two bytes in little-endian order.</param>
		/// <returns>A 16-bit integer in the endianness of the host machine.</returns>
		public static UInt16 Assemble16(this byte[] bytes)
		{
			if (bytes.Length != 2) throw new ArgumentException("Length of byte array passed to byte[].Assemble16 must be 2.");
			if (!BitConverter.IsLittleEndian) return BitConverter.ToUInt16(bytes.Reverse().ToArray(), 0);
			return BitConverter.ToUInt16(bytes, 0);
		}

		/// <summary>
		/// Assembles a 32-bit integer from four bytes supplied in little-endian order.
		/// </summary>
		/// <param name="bytes">Byte array of four bytes in little-endian order.</param>
		/// <returns>A 16-bit integer in the endianness of the host machine.</returns>
		public static UInt32 Assemble32(this byte[] bytes)
		{
			if (bytes.Length != 2) throw new ArgumentException("Length of byte array passed to byte[].Assemble32 must be 4.");
			if (!BitConverter.IsLittleEndian) return BitConverter.ToUInt32(bytes.Reverse().ToArray(), 0);
			return BitConverter.ToUInt32(bytes, 0);
		}

		/// <summary>
		/// Disassembles a 16-bit integer into two bytes in little-endian order.
		/// </summary>
		/// <param name="value">16-bit integer.</param>
		/// <returns>A byte array of two bytes in little-endian order.</returns>
		public static byte[] Disassemble(this UInt16 value)
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
		public static byte[] Disassemble(this UInt32 value)
		{
			byte[] bytes = BitConverter.GetBytes(value);
			if (!BitConverter.IsLittleEndian) Array.Reverse(bytes);
			return bytes;
		}
	}
}