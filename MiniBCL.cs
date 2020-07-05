namespace System
{
    using System.Runtime.CompilerServices;
    using Internal.Runtime.CompilerServices;

    public class Object
    {
        // The layout of object is a contract with the compiler.
        public IntPtr m_pEEType;
    }
    public struct Void { }

    // The layout of primitive types is special cased because it would be recursive.
    // These really don't need any fields to work.
    public struct Boolean { }
    public struct Char { }
    public struct SByte { }
    public struct Byte { }
    public struct Int16 { }
    public struct UInt16 { }
    public struct Int32 { }
    public struct UInt32 { }
    public struct Int64 { }
    public struct UInt64 { }
    public struct UIntPtr { }
    public struct Single { }
    public struct Double { }

    public unsafe struct IntPtr
    {
        private void* _value;
        public IntPtr(void* value) { _value = value; }
        [Intrinsic]
        public static readonly IntPtr Zero;
    }

    public abstract class ValueType { }
    public abstract class Enum : ValueType { }

    public struct Nullable<T> where T : struct { }
    
    public sealed class String
    {
        // The layout of the string type is a contract with the compiler.
        public readonly int Length;
        public char _firstChar;

        public unsafe char this[int index]
        {
            [System.Runtime.CompilerServices.Intrinsic]
            get
            {
                return Internal.Runtime.CompilerServices.Unsafe.Add(ref _firstChar, index);
            }
        }
    }
    public abstract class Array
    {
#pragma warning disable CA1823, 169, 649 // private field '{blah}' is never used
        private int _numComponents;
#pragma warning restore CA1823, 169, 649

        public int Length
        {
            get
            {
                // NOTE: The compiler has assumptions about the implementation of this method.
                // Changing the implementation here (or even deleting this) will NOT have the desired impact
                return _numComponents;
            }
        }
    }

    public abstract class Delegate { }
    public abstract class MulticastDelegate : Delegate { }

    public struct RuntimeTypeHandle { }
    public struct RuntimeMethodHandle { }
    public struct RuntimeFieldHandle { }

    public class Attribute { }

    public enum AttributeTargets { }

    public sealed class AttributeUsageAttribute : Attribute
    {
        public AttributeUsageAttribute(AttributeTargets validOn)
        {
        }

        public bool AllowMultiple { get; set; }

        public bool Inherited { get; set; }
    }

    internal readonly ref struct ByReference<T>
    {
#pragma warning disable CA1823, 169 // private field '{blah}' is never used
        private readonly IntPtr _value;
#pragma warning restore CA1823, 169

        [Intrinsic]
        public extern ByReference(ref T value);

        public unsafe ref T Value
        {
            // Implemented as a JIT intrinsic - This default implementation is for
            // completeness and to provide a concrete error if called via reflection
            // or if the intrinsic is missed.
            [Intrinsic]
            get => ref Unsafe.As<byte, T>(ref *(byte*)null);
        }
    }

    public readonly ref struct ReadOnlySpan<T>
    {
        internal readonly ByReference<T> _pointer;
        private readonly int _length;

        public ReadOnlySpan(T[] array)
        {
            if (array == null)
            {
                this = default;
                return; // returns default
            }

            _pointer = new ByReference<T>(ref array[0]);
            _length = array.Length;
        }

        public unsafe ReadOnlySpan(void* pointer, int length)
        {
            _pointer = new ByReference<T>(ref Unsafe.As<byte, T>(ref *(byte*)pointer));
            _length = length;
        }

        public ref readonly T this[int index]
        {
            [Intrinsic]
            get
            {
                //if ((uint)index >= (uint)_length)
                //    ThrowHelper.ThrowIndexOutOfRangeException();
                return ref Unsafe.Add(ref _pointer.Value, index);
            }
        }

        public static implicit operator ReadOnlySpan<T>(T[] array) => new ReadOnlySpan<T>(array);
    }
}

namespace System.Runtime.CompilerServices
{
    internal sealed class IntrinsicAttribute : Attribute { }

    public class RuntimeHelpers
    {
        public static unsafe int OffsetToStringData => sizeof(IntPtr) + sizeof(int);
    }

    public enum MethodImplOptions
    {
        NoInlining = 0x0008,
        InternalCall = 0x1000,
        NoOptimization = 64,
    }

    public sealed class MethodImplAttribute : Attribute
    {
        public MethodImplAttribute(MethodImplOptions methodImplOptions) { }
    }
}

namespace System.Runtime.InteropServices
{
    public enum CharSet
    {
        None = 1,
        Ansi = 2,
        Unicode = 3,
        Auto = 4,
    }

    public sealed class DllImportAttribute : Attribute
    {
        public string EntryPoint;
        public CharSet CharSet;
        public bool ExactSpelling;
        public DllImportAttribute(string dllName) { }
    }

    public enum LayoutKind
    {
        Sequential = 0,
        Explicit = 2,
        Auto = 3,
    }

    public sealed class UnmanagedCallersOnlyAttribute : Attribute
    {
        public string EntryPoint;
        public UnmanagedCallersOnlyAttribute() { }
    }

    public sealed class StructLayoutAttribute : Attribute
    {
        public StructLayoutAttribute(LayoutKind layoutKind) { }
    }

    public sealed class InAttribute : Attribute
    {
    }
}
namespace Internal.Runtime.CompilerServices
{
    public static unsafe partial class Unsafe
    {
        // The body of this method is generated by the compiler.
        // It will do what Unsafe.Add is expected to do. It's just not possible to express it in C#.
        [System.Runtime.CompilerServices.Intrinsic]
        public static extern ref T Add<T>(ref T source, int elementOffset);
        [System.Runtime.CompilerServices.Intrinsic]
        public static extern ref TTo As<TFrom, TTo>(ref TFrom source);

    }
}
