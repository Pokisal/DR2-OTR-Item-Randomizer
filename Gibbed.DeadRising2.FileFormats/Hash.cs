using System.Text;

namespace Gibbed.DeadRising2.FileFormats
{
	public static class Hash
	{
		public static uint Calculate(byte[] buffer, int offset, int length, uint magic)
		{
			uint num = 0u;
			for (int i = offset; i < offset + length; i++)
			{
				num *= magic;
				num ^= buffer[i];
			}
			return num;
		}

		public static uint Calculate(byte[] buffer, int offset, int length)
		{
			return Calculate(buffer, offset, length, 33u);
		}

		public static uint Calculate(string text, uint magic)
		{
			byte[] bytes = Encoding.ASCII.GetBytes(text);
			return Calculate(bytes, 0, bytes.Length, magic);
		}

		public static uint Calculate(string text)
		{
			byte[] bytes = Encoding.ASCII.GetBytes(text);
			return Calculate(bytes, 0, bytes.Length);
		}
	}
}
