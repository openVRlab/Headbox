using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
namespace Burst.Compiler.IL.Tests
{
    /// <summary>
    /// Tests types
    /// </summary>
    internal class NotSupported
    {
        [TestCompiler(1, ExpectCompilerException = true)]
        public static int TestDelegate(int data)
        {
            return ProcessData(i => i + 1, data);
        }

        [TestCompiler(1, ExpectCompilerException = true)]
        public static bool TestIsOfType(object data)
        {
            var check = data as NotSupported;
            return (check != null);
        }

        private static int ProcessData(Func<int, int> yo, int value)
        {
            return yo(value);
        }

        public struct HasMarshalAttribute
        {
            [MarshalAs(UnmanagedType.U1)] public bool A;
        }

        //[TestCompiler(ExpectCompilerException = true)]
        [TestCompiler()] // Because MarshalAs is used in mathematics we cannot disable it for now
        public static void TestStructWithMarshalAs()
        {
#pragma warning disable 0219
            var x = new HasMarshalAttribute();
#pragma warning restore 0219
        }

        [TestCompiler(true, ExpectCompilerException = true)]
        public static void TestMethodWithMarshalAsParameter([MarshalAs(UnmanagedType.U1)] bool x)
        {
        }

        [TestCompiler(ExpectCompilerException = true)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static bool TestMethodWithMarshalAsReturnType()
        {
            return true;
        }
    }
}
