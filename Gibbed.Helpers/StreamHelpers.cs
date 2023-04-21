#define DEBUG
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Gibbed.Helpers
{
	public static class StreamHelpers
	{
		public static Encoding DefaultEncoding = Encoding.ASCII;

		internal static bool ShouldSwap(bool littleEndian)
		{
			if (littleEndian && !BitConverter.IsLittleEndian)
			{
				return true;
			}
			if (!littleEndian && BitConverter.IsLittleEndian)
			{
				return true;
			}
			return false;
		}

		public static MemoryStream ReadToMemoryStream(this Stream stream, long size, int buffer)
		{
			MemoryStream memoryStream = new MemoryStream();
			long num = size;
			byte[] array = new byte[buffer];
			while (num > 0)
			{
				int num2 = (int)Math.Min(num, array.Length);
				stream.Read(array, 0, num2);
				memoryStream.Write(array, 0, num2);
				num -= num2;
			}
			memoryStream.Seek(0L, SeekOrigin.Begin);
			return memoryStream;
		}

		public static MemoryStream ReadToMemoryStream(this Stream stream, long size)
		{
			return stream.ReadToMemoryStream(size, 16384);
		}

		public static void WriteFromStream(this Stream stream, Stream input, long size, int buffer)
		{
			long num = size;
			byte[] array = new byte[buffer];
			while (num > 0)
			{
				int num2 = (int)Math.Min(num, array.Length);
				input.Read(array, 0, num2);
				stream.Write(array, 0, num2);
				num -= num2;
			}
		}

		public static void WriteFromStream(this Stream stream, Stream input, long size)
		{
			stream.WriteFromStream(input, size, 16384);
		}

		public static Guid ReadValueGuid(this Stream stream, bool littleEndian)
		{
			int a = stream.ReadValueS32(littleEndian);
			short b = stream.ReadValueS16(littleEndian);
			short c = stream.ReadValueS16(littleEndian);
			byte[] array = new byte[8];
			stream.Read(array, 0, array.Length);
			return new Guid(a, b, c, array);
		}

		public static Guid ReadValueGuid(this Stream stream)
		{
			return stream.ReadValueGuid(littleEndian: true);
		}

		public static void WriteValueGuid(this Stream stream, Guid value, bool littleEndian)
		{
			byte[] array = value.ToByteArray();
			stream.WriteValueS32(BitConverter.ToInt32(array, 0), littleEndian);
			stream.WriteValueS16(BitConverter.ToInt16(array, 4), littleEndian);
			stream.WriteValueS16(BitConverter.ToInt16(array, 6), littleEndian);
			stream.Write(array, 8, 8);
		}

		public static void WriteValueGuid(this Stream stream, Guid value)
		{
			stream.WriteValueGuid(value, littleEndian: true);
		}

		public static string ReadString(this Stream stream, uint size)
		{
			return stream.ReadStringInternalStatic(DefaultEncoding, size, trailingNull: false);
		}

		public static string ReadString(this Stream stream, uint size, bool trailingNull)
		{
			return stream.ReadStringInternalStatic(DefaultEncoding, size, trailingNull);
		}

		public static string ReadStringZ(this Stream stream)
		{
			return stream.ReadStringInternalDynamic(DefaultEncoding, '\0');
		}

		public static void WriteString(this Stream stream, string value)
		{
			stream.WriteStringInternalStatic(DefaultEncoding, value);
		}

		public static void WriteStringZ(this Stream stream, string value)
		{
			stream.WriteStringInternalDynamic(DefaultEncoding, value, '\0');
		}

		public static ushort ReadValueU16(this Stream stream)
		{
			return stream.ReadValueU16(littleEndian: true);
		}

		public static ushort ReadValueU16(this Stream stream, bool littleEndian)
		{
			byte[] array = new byte[2];
			Debug.Assert(stream.Read(array, 0, 2) == 2);
			ushort num = BitConverter.ToUInt16(array, 0);
			if (ShouldSwap(littleEndian))
			{
				num = num.Swap();
			}
			return num;
		}

		public static void WriteValueU16(this Stream stream, ushort value)
		{
			stream.WriteValueU16(value, littleEndian: true);
		}

		public static void WriteValueU16(this Stream stream, ushort value, bool littleEndian)
		{
			if (ShouldSwap(littleEndian))
			{
				value = value.Swap();
			}
			byte[] bytes = BitConverter.GetBytes(value);
			Debug.Assert(bytes.Length == 2);
			stream.Write(bytes, 0, 2);
		}

		public static short ReadValueS16(this Stream stream)
		{
			return stream.ReadValueS16(littleEndian: true);
		}

		public static short ReadValueS16(this Stream stream, bool littleEndian)
		{
			byte[] array = new byte[2];
			Debug.Assert(stream.Read(array, 0, 2) == 2);
			short num = BitConverter.ToInt16(array, 0);
			if (ShouldSwap(littleEndian))
			{
				num = num.Swap();
			}
			return num;
		}

		public static void WriteValueS16(this Stream stream, short value)
		{
			stream.WriteValueS16(value, littleEndian: true);
		}

		public static void WriteValueS16(this Stream stream, short value, bool littleEndian)
		{
			if (ShouldSwap(littleEndian))
			{
				value = value.Swap();
			}
			byte[] bytes = BitConverter.GetBytes(value);
			Debug.Assert(bytes.Length == 2);
			stream.Write(bytes, 0, 2);
		}

		public static bool ReadValueBoolean(this Stream stream)
		{
			return stream.ReadValueB8();
		}

		public static void WriteValueBoolean(this Stream stream, bool value)
		{
			stream.WriteValueB8(value);
		}

		public static bool ReadValueB8(this Stream stream)
		{
			return stream.ReadValueU8() > 0;
		}

		public static void WriteValueB8(this Stream stream, bool value)
		{
			stream.WriteValueU8((byte)(value ? 1u : 0u));
		}

		public static bool ReadValueB32(this Stream stream)
		{
			return (stream.ReadValueU32() & 1) == 1;
		}

		public static void WriteValueB32(this Stream stream, bool value)
		{
			stream.WriteValueU32((byte)(value ? 1u : 0u));
		}

		internal static string ReadStringInternalStatic(this Stream stream, Encoding encoding, uint size, bool trailingNull)
		{
			byte[] array = new byte[size];
			stream.Read(array, 0, array.Length);
			string text = encoding.GetString(array, 0, array.Length);
			if (trailingNull)
			{
				int num = text.IndexOf('\0');
				if (num >= 0)
				{
					text = text.Substring(0, num);
				}
			}
			return text;
		}

		internal static void WriteStringInternalStatic(this Stream stream, Encoding encoding, string value)
		{
			byte[] bytes = encoding.GetBytes(value);
			stream.Write(bytes, 0, bytes.Length);
		}

		internal static string ReadStringInternalDynamic(this Stream stream, Encoding encoding, char end)
		{
			int byteCount = encoding.GetByteCount("e");
			Debug.Assert(byteCount == 1 || byteCount == 2 || byteCount == 4);
			string text = end.ToString();
			int num = 0;
			byte[] array = new byte[128 * byteCount];
			while (true)
			{
				if (num + byteCount > array.Length)
				{
					Array.Resize(ref array, array.Length + 128 * byteCount);
				}
				Debug.Assert(stream.Read(array, num, byteCount) == byteCount);
				if (encoding.GetString(array, num, byteCount) == text)
				{
					break;
				}
				num += byteCount;
			}
			if (num == 0)
			{
				return "";
			}
			return encoding.GetString(array, 0, num);
		}

		internal static void WriteStringInternalDynamic(this Stream stream, Encoding encoding, string value, char end)
		{
			byte[] bytes = encoding.GetBytes(value);
			stream.Write(bytes, 0, bytes.Length);
			bytes = encoding.GetBytes(end.ToString());
			stream.Write(bytes, 0, bytes.Length);
		}

		public static uint ReadValueU24(this Stream stream)
		{
			return stream.ReadValueU24(littleEndian: true);
		}

		public static uint ReadValueU24(this Stream stream, bool littleEndian)
		{
			byte[] array = new byte[4];
			Debug.Assert(stream.Read(array, 0, 3) == 3);
			uint num = BitConverter.ToUInt32(array, 0);
			if (ShouldSwap(littleEndian))
			{
				num = num.Swap24();
			}
			return num & 0xFFFFFFu;
		}

		public static void WriteValueU24(this Stream stream, uint value)
		{
			stream.WriteValueU24(value, littleEndian: true);
		}

		public static void WriteValueU24(this Stream stream, uint value, bool littleEndian)
		{
			if (ShouldSwap(littleEndian))
			{
				value = value.Swap24();
			}
			value &= 0xFFFFFFu;
			byte[] bytes = BitConverter.GetBytes(value);
			Debug.Assert(bytes.Length == 4);
			stream.Write(bytes, 0, 3);
		}

		public static sbyte ReadValueS8(this Stream stream)
		{
			return (sbyte)stream.ReadByte();
		}

		public static void WriteValueS8(this Stream stream, sbyte value)
		{
			stream.WriteByte((byte)value);
		}

		public static float ReadValueF32(this Stream stream)
		{
			return stream.ReadValueF32(littleEndian: true);
		}

		public static float ReadValueF32(this Stream stream, bool littleEndian)
		{
			byte[] array = new byte[4];
			Debug.Assert(stream.Read(array, 0, 4) == 4);
			if (ShouldSwap(littleEndian))
			{
				return BitConverter.ToSingle(BitConverter.GetBytes(BitConverter.ToInt32(array, 0).Swap()), 0);
			}
			return BitConverter.ToSingle(array, 0);
		}

		public static void WriteValueF32(this Stream stream, float value)
		{
			stream.WriteValueF32(value, littleEndian: true);
		}

		public static void WriteValueF32(this Stream stream, float value, bool littleEndian)
		{
			byte[] array = ((!ShouldSwap(littleEndian)) ? BitConverter.GetBytes(value) : BitConverter.GetBytes(BitConverter.ToInt32(BitConverter.GetBytes(value), 0).Swap()));
			Debug.Assert(array.Length == 4);
			stream.Write(array, 0, 4);
		}

		public static byte ReadValueU8(this Stream stream)
		{
			return (byte)stream.ReadByte();
		}

		public static void WriteValueU8(this Stream stream, byte value)
		{
			stream.WriteByte(value);
		}

		public static double ReadValueF64(this Stream stream)
		{
			return stream.ReadValueF64(littleEndian: true);
		}

		public static double ReadValueF64(this Stream stream, bool littleEndian)
		{
			byte[] array = new byte[8];
			Debug.Assert(stream.Read(array, 0, 8) == 8);
			if (ShouldSwap(littleEndian))
			{
				return BitConverter.Int64BitsToDouble(BitConverter.ToInt64(array, 0).Swap());
			}
			return BitConverter.ToDouble(array, 0);
		}

		public static void WriteValueF64(this Stream stream, double value)
		{
			stream.WriteValueF64(value, littleEndian: true);
		}

		public static void WriteValueF64(this Stream stream, double value, bool littleEndian)
		{
			byte[] array = ((!ShouldSwap(littleEndian)) ? BitConverter.GetBytes(value) : BitConverter.GetBytes(BitConverter.DoubleToInt64Bits(value).Swap()));
			Debug.Assert(array.Length == 8);
			stream.Write(array, 0, 8);
		}

		public static uint ReadValueU32(this Stream stream)
		{
			return stream.ReadValueU32(littleEndian: true);
		}

		public static uint ReadValueU32(this Stream stream, bool littleEndian)
		{
			byte[] array = new byte[4];
			Debug.Assert(stream.Read(array, 0, 4) == 4);
			uint num = BitConverter.ToUInt32(array, 0);
			if (ShouldSwap(littleEndian))
			{
				num = num.Swap();
			}
			return num;
		}

		public static void WriteValueU32(this Stream stream, uint value)
		{
			stream.WriteValueU32(value, littleEndian: true);
		}

		public static void WriteValueU32(this Stream stream, uint value, bool littleEndian)
		{
			if (ShouldSwap(littleEndian))
			{
				value = value.Swap();
			}
			byte[] bytes = BitConverter.GetBytes(value);
			Debug.Assert(bytes.Length == 4);
			stream.Write(bytes, 0, 4);
		}

		public static ulong ReadValueU64(this Stream stream)
		{
			return stream.ReadValueU64(littleEndian: true);
		}

		public static ulong ReadValueU64(this Stream stream, bool littleEndian)
		{
			byte[] array = new byte[8];
			Debug.Assert(stream.Read(array, 0, 8) == 8);
			ulong num = BitConverter.ToUInt64(array, 0);
			if (ShouldSwap(littleEndian))
			{
				num = num.Swap();
			}
			return num;
		}

		public static void WriteValueU64(this Stream stream, ulong value)
		{
			stream.WriteValueU64(value, littleEndian: true);
		}

		public static void WriteValueU64(this Stream stream, ulong value, bool littleEndian)
		{
			if (ShouldSwap(littleEndian))
			{
				value = value.Swap();
			}
			byte[] bytes = BitConverter.GetBytes(value);
			Debug.Assert(bytes.Length == 8);
			stream.Write(bytes, 0, 8);
		}

		public static T ReadValueEnum<T>(this Stream stream, bool littleEndian)
		{
			Type typeFromHandle = typeof(T);
			if (!typeFromHandle.IsEnum)
			{
				throw new InvalidOperationException("not an enum");
			}
			Type underlyingType = Enum.GetUnderlyingType(typeFromHandle);
			if (!underlyingType.IsPrimitive)
			{
				throw new InvalidOperationException("enum is not primitive");
			}
			if (underlyingType == typeof(byte))
			{
				return (T)Enum.ToObject(typeFromHandle, stream.ReadValueU8());
			}
			if (underlyingType == typeof(sbyte))
			{
				return (T)Enum.ToObject(typeFromHandle, stream.ReadValueS8());
			}
			if (underlyingType == typeof(short))
			{
				return (T)Enum.ToObject(typeFromHandle, stream.ReadValueS16(littleEndian));
			}
			if (underlyingType == typeof(ushort))
			{
				return (T)Enum.ToObject(typeFromHandle, stream.ReadValueU16(littleEndian));
			}
			if (underlyingType == typeof(int))
			{
				return (T)Enum.ToObject(typeFromHandle, stream.ReadValueS32(littleEndian));
			}
			if (underlyingType == typeof(uint))
			{
				return (T)Enum.ToObject(typeFromHandle, stream.ReadValueU32(littleEndian));
			}
			throw new InvalidOperationException("unhandled enum primitive type");
		}

		public static T ReadValueEnum<T>(this Stream stream)
		{
			return stream.ReadValueEnum<T>(littleEndian: true);
		}

		public static void WriteValueEnum<T>(this Stream stream, object value, bool littleEndian)
		{
			Type typeFromHandle = typeof(T);
			if (!typeFromHandle.IsEnum)
			{
				throw new InvalidOperationException("not an enum");
			}
			Type underlyingType = Enum.GetUnderlyingType(typeFromHandle);
			if (!underlyingType.IsPrimitive)
			{
				throw new InvalidOperationException("enum is not primitive");
			}
			if (underlyingType == typeof(byte))
			{
				stream.WriteValueU8((byte)value);
				return;
			}
			if (underlyingType == typeof(sbyte))
			{
				stream.WriteValueS8((sbyte)value);
				return;
			}
			if (underlyingType == typeof(short))
			{
				stream.WriteValueS16((short)value, littleEndian);
				return;
			}
			if (underlyingType == typeof(ushort))
			{
				stream.WriteValueU16((ushort)value, littleEndian);
				return;
			}
			if (underlyingType == typeof(int))
			{
				stream.WriteValueS32((int)value, littleEndian);
				return;
			}
			if (underlyingType == typeof(uint))
			{
				stream.WriteValueU32((uint)value, littleEndian);
				return;
			}
			throw new InvalidOperationException("unhandled enum primitive type");
		}

		public static void WriteValueEnum<T>(this Stream stream, object value)
		{
			stream.WriteValueEnum<T>(value, littleEndian: true);
		}

		public static int ReadAligned(this Stream stream, byte[] buffer, int offset, int size, int align)
		{
			if (size == 0)
			{
				return 0;
			}
			int result = stream.Read(buffer, offset, size);
			int num = size % align;
			if (num > 0)
			{
				stream.Seek(align - num, SeekOrigin.Current);
			}
			return result;
		}

		public static void WriteAligned(this Stream stream, byte[] buffer, int offset, int size, int align)
		{
			if (size != 0)
			{
				stream.Write(buffer, offset, size);
				int num = size % align;
				if (num > 0)
				{
					byte[] buffer2 = new byte[align - num];
					stream.Write(buffer2, 0, align - num);
				}
			}
		}

		public static T ReadStructure<T>(this Stream stream)
		{
			int num = Marshal.SizeOf(typeof(T));
			byte[] array = new byte[num];
			if (stream.Read(array, 0, num) != num)
			{
				throw new InvalidOperationException("could not read all of data for structure");
			}
			GCHandle gCHandle = GCHandle.Alloc(array, GCHandleType.Pinned);
			T result = (T)Marshal.PtrToStructure(gCHandle.AddrOfPinnedObject(), typeof(T));
			gCHandle.Free();
			return result;
		}

		public static T ReadStructure<T>(this Stream stream, int size)
		{
			byte[] array = new byte[Math.Max(Marshal.SizeOf(typeof(T)), size)];
			if (stream.Read(array, 0, size) != size)
			{
				throw new InvalidOperationException("could not read all of data for structure");
			}
			GCHandle gCHandle = GCHandle.Alloc(array, GCHandleType.Pinned);
			T result = (T)Marshal.PtrToStructure(gCHandle.AddrOfPinnedObject(), typeof(T));
			gCHandle.Free();
			return result;
		}

		public static void WriteStructure<T>(this Stream stream, T structure)
		{
			byte[] array = new byte[Marshal.SizeOf(typeof(T))];
			GCHandle gCHandle = GCHandle.Alloc(array, GCHandleType.Pinned);
			Marshal.StructureToPtr(structure, gCHandle.AddrOfPinnedObject(), fDeleteOld: false);
			gCHandle.Free();
			stream.Write(array, 0, array.Length);
		}

		public static void WriteStructure<T>(this Stream stream, T structure, int size)
		{
			byte[] array = new byte[Math.Max(Marshal.SizeOf(typeof(T)), size)];
			GCHandle gCHandle = GCHandle.Alloc(array, GCHandleType.Pinned);
			Marshal.StructureToPtr(structure, gCHandle.AddrOfPinnedObject(), fDeleteOld: false);
			gCHandle.Free();
			stream.Write(array, 0, array.Length);
		}

		public static string ReadString(this Stream stream, uint size, Encoding encoding)
		{
			return stream.ReadStringInternalStatic(encoding, size, trailingNull: false);
		}

		public static string ReadString(this Stream stream, int size, Encoding encoding)
		{
			return stream.ReadStringInternalStatic(encoding, (uint)size, trailingNull: false);
		}

		public static string ReadString(this Stream stream, uint size, bool trailingNull, Encoding encoding)
		{
			return stream.ReadStringInternalStatic(encoding, size, trailingNull);
		}

		public static string ReadString(this Stream stream, int size, bool trailingNull, Encoding encoding)
		{
			return stream.ReadStringInternalStatic(encoding, (uint)size, trailingNull);
		}

		public static string ReadStringZ(this Stream stream, Encoding encoding)
		{
			return stream.ReadStringInternalDynamic(encoding, '\0');
		}

		public static void WriteString(this Stream stream, string value, Encoding encoding)
		{
			stream.WriteStringInternalStatic(encoding, value);
		}

		public static void WriteStringZ(this Stream stream, string value, Encoding encoding)
		{
			stream.WriteStringInternalDynamic(encoding, value, '\0');
		}

		public static int ReadValueS32(this Stream stream)
		{
			return stream.ReadValueS32(littleEndian: true);
		}

		public static int ReadValueS32(this Stream stream, bool littleEndian)
		{
			byte[] array = new byte[4];
			Debug.Assert(stream.Read(array, 0, 4) == 4);
			int num = BitConverter.ToInt32(array, 0);
			if (ShouldSwap(littleEndian))
			{
				num = num.Swap();
			}
			return num;
		}

		public static void WriteValueS32(this Stream stream, int value)
		{
			stream.WriteValueS32(value, littleEndian: true);
		}

		public static void WriteValueS32(this Stream stream, int value, bool littleEndian)
		{
			if (ShouldSwap(littleEndian))
			{
				value = value.Swap();
			}
			byte[] bytes = BitConverter.GetBytes(value);
			Debug.Assert(bytes.Length == 4);
			stream.Write(bytes, 0, 4);
		}

		public static long ReadValueS64(this Stream stream)
		{
			return stream.ReadValueS64(littleEndian: true);
		}

		public static long ReadValueS64(this Stream stream, bool littleEndian)
		{
			byte[] array = new byte[8];
			Debug.Assert(stream.Read(array, 0, 8) == 8);
			long num = BitConverter.ToInt64(array, 0);
			if (ShouldSwap(littleEndian))
			{
				num = num.Swap();
			}
			return num;
		}

		public static void WriteValueS64(this Stream stream, long value)
		{
			stream.WriteValueS64(value, littleEndian: true);
		}

		public static void WriteValueS64(this Stream stream, long value, bool littleEndian)
		{
			if (ShouldSwap(littleEndian))
			{
				value = value.Swap();
			}
			byte[] bytes = BitConverter.GetBytes(value);
			Debug.Assert(bytes.Length == 8);
			stream.Write(bytes, 0, 8);
		}
	}
}
