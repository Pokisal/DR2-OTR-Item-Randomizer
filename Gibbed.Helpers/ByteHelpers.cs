using System;
using System.Runtime.InteropServices;

namespace Gibbed.Helpers
{
	public static class ByteHelpers
	{
		public static void Reset(this byte[] data, byte value)
		{
			for (int i = 0; i < data.Length; i++)
			{
				data[i] = value;
			}
		}

		public static T ToStructure<T>(this byte[] data, int index)
		{
			int num = Marshal.SizeOf(typeof(T));
			if (index + num > data.Length)
			{
				throw new Exception("not enough data to fit the structure");
			}
			byte[] array = new byte[num];
			Array.Copy(data, index, array, 0, num);
			GCHandle gCHandle = GCHandle.Alloc(array, GCHandleType.Pinned);
			T result = (T)Marshal.PtrToStructure(gCHandle.AddrOfPinnedObject(), typeof(T));
			gCHandle.Free();
			return result;
		}

		public static T ToStructure<T>(this byte[] data)
		{
			return data.ToStructure<T>(0);
		}
	}
}
