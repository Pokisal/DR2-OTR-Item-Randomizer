using System;

namespace Gibbed.Helpers
{
	public static class NumberHelpers
	{
		public static short BigEndian(this short value)
		{
			if (BitConverter.IsLittleEndian)
			{
				return value.Swap();
			}
			return value;
		}

		public static ushort BigEndian(this ushort value)
		{
			if (BitConverter.IsLittleEndian)
			{
				return value.Swap();
			}
			return value;
		}

		public static int BigEndian(this int value)
		{
			if (BitConverter.IsLittleEndian)
			{
				return value.Swap();
			}
			return value;
		}

		public static uint BigEndian(this uint value)
		{
			if (BitConverter.IsLittleEndian)
			{
				return value.Swap();
			}
			return value;
		}

		public static long BigEndian(this long value)
		{
			if (BitConverter.IsLittleEndian)
			{
				return value.Swap();
			}
			return value;
		}

		public static ulong BigEndian(this ulong value)
		{
			if (BitConverter.IsLittleEndian)
			{
				return value.Swap();
			}
			return value;
		}

		public static short LittleEndian(this short value)
		{
			if (!BitConverter.IsLittleEndian)
			{
				return value.Swap();
			}
			return value;
		}

		public static ushort LittleEndian(this ushort value)
		{
			if (!BitConverter.IsLittleEndian)
			{
				return value.Swap();
			}
			return value;
		}

		public static int LittleEndian(this int value)
		{
			if (!BitConverter.IsLittleEndian)
			{
				return value.Swap();
			}
			return value;
		}

		public static uint LittleEndian(this uint value)
		{
			if (!BitConverter.IsLittleEndian)
			{
				return value.Swap();
			}
			return value;
		}

		public static long LittleEndian(this long value)
		{
			if (!BitConverter.IsLittleEndian)
			{
				return value.Swap();
			}
			return value;
		}

		public static ulong LittleEndian(this ulong value)
		{
			if (!BitConverter.IsLittleEndian)
			{
				return value.Swap();
			}
			return value;
		}

		public static int Align(this int value, int align)
		{
			if (value == 0)
			{
				return value;
			}
			return value + (align - value % align) % align;
		}

		public static uint Align(this uint value, uint align)
		{
			if (value == 0)
			{
				return value;
			}
			return value + (align - value % align) % align;
		}

		public static long Align(this long value, long align)
		{
			if (value == 0)
			{
				return value;
			}
			return value + (align - value % align) % align;
		}

		public static ulong Align(this ulong value, ulong align)
		{
			if (value == 0)
			{
				return value;
			}
			return value + (align - value % align) % align;
		}

		public static short Swap(this short value)
		{
			return (short)(ushort)((0xFF & (value >> 8)) | (0xFF00 & (value << 8)));
		}

		public static ushort Swap(this ushort value)
		{
			return (ushort)((0xFFu & (uint)(value >> 8)) | (0xFF00u & (uint)(value << 8)));
		}

		public static int Swap(this int value)
		{
			return (int)((0xFF & ((uint)value >> 24)) | (0xFF00 & ((uint)value >> 8))) | (0xFF0000 & (value << 8)) | (-16777216 & (value << 24));
		}

		public static int Swap24(this int value)
		{
			return (0xFF & (value >> 16)) | (0xFF00 & value) | (0xFF0000 & (value << 16));
		}

		public static uint Swap(this uint value)
		{
			return (0xFFu & (value >> 24)) | (0xFF00u & (value >> 8)) | (0xFF0000u & (value << 8)) | (0xFF000000u & (value << 24));
		}

		public static uint Swap24(this uint value)
		{
			return (0xFFu & (value >> 16)) | (0xFF00u & value) | (0xFF0000u & (value << 16));
		}

		public static long Swap(this long value)
		{
			return (long)((0xFF & ((ulong)value >> 56)) | (0xFF00 & ((ulong)value >> 40)) | (0xFF0000 & ((ulong)value >> 24)) | (0xFF000000u & ((ulong)value >> 8))) | (0xFF00000000L & (value << 8)) | (0xFF0000000000L & (value << 24)) | (0xFF000000000000L & (value << 40)) | (-72057594037927936L & (value << 56));
		}

		public static ulong Swap(this ulong value)
		{
			return (0xFF & (value >> 56)) | (0xFF00 & (value >> 40)) | (0xFF0000 & (value >> 24)) | (0xFF000000u & (value >> 8)) | (0xFF00000000uL & (value << 8)) | (0xFF0000000000uL & (value << 24)) | (0xFF000000000000uL & (value << 40)) | (0xFF00000000000000uL & (value << 56));
		}
	}
}
