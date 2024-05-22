using FTG.Studios.BISC.VM;
using NUnit.Framework;
using System;

namespace FTG.Studios.BISC.Test {

	[TestFixture]
	public class RegisterValueTest {
		
		[Test]
		public void CanSetUValue() {
			const UInt32 expected = 0xbabecafe;
			RegisterValue register;
			
			register.UValue = expected;
			
			AssemblerTest.AreEqual(expected, register.UValue);
		}
		
		[Test]
		public void CanSetIValue() {
			const int expected = 437289237;
			RegisterValue register;
			
			register.IValue = expected;
			
			AssemblerTest.AreEqual(expected, register.IValue);
		}
		
		[Test]
		public void CanSetFValue() {
			const float expected = 4325435.543f;
			RegisterValue register;
			
			register.FValue = expected;
			
			AssemblerTest.AreEqual(expected, register.FValue);
		}
		
		[Test]
		public void CanConvertFromUToI() {
			const UInt32 uvalue = 0xffffffff;
			const int ivalue = -1;
			RegisterValue register;
			
			register.UValue = uvalue;
			
			AssemblerTest.AreEqual(ivalue, register.IValue);
		}
	}
}