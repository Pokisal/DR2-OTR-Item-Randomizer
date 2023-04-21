using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Gibbed.Helpers;

namespace Gibbed.DeadRising2.FileFormats
{
	public class BigFile
	{
		public class Entry
		{
			public string Name;

			public uint NameHash;

			public uint CompressedSize;

			public uint UncompressedSize;

			public uint Offset;

			public uint Alignment;

			public CompressionScheme CompressionScheme;

			public bool Compressed => CompressionScheme != CompressionScheme.None;

			public override string ToString()
			{
				if (Name != null)
				{
					return Name;
				}
				return base.ToString();
			}
		}

		public enum CompressionScheme : uint
		{
			None,
			ZLib,
			XBox
		}

		public uint Version;

		public List<Entry> Entries = new List<Entry>();

		public bool DuplicateNames;

		public void Serialize(Stream output)
		{
			if (Version != 2)
			{
				throw new FormatException("unsupported version " + Version);
			}
			MemoryStream memoryStream = new MemoryStream();
			uint num = 0u;
			uint num2 = 0u;
			num += 24;
			num += (uint)(28 * Entries.Count);
			memoryStream.Position = num;
			foreach (Entry entry in Entries)
			{
				memoryStream.WriteStringZ(entry.Name, Encoding.ASCII);
			}
			memoryStream.Position = 24L;
			uint num3 = num;
			foreach (Entry entry2 in Entries)
			{
				memoryStream.WriteValueU32(num3);
				memoryStream.WriteValueU32(entry2.NameHash);
				if (entry2.CompressionScheme == CompressionScheme.XBox)
				{
					memoryStream.WriteValueU32(entry2.UncompressedSize);
					memoryStream.WriteValueU32(entry2.CompressedSize);
				}
				else
				{
					memoryStream.WriteValueU32(entry2.CompressedSize);
					memoryStream.WriteValueU32(entry2.UncompressedSize);
				}
				memoryStream.WriteValueU32(entry2.Offset);
				memoryStream.WriteValueU32(entry2.Alignment);
				memoryStream.WriteValueU32((uint)entry2.CompressionScheme);
				num3 += (uint)(entry2.Name.Length + 1);
				num2 += entry2.CompressedSize.Align(entry2.Alignment);
			}
			uint value = (uint)memoryStream.Length;
			memoryStream.SetLength(memoryStream.Length.Align(4L));
			num2 += (uint)(int)memoryStream.Length;
			memoryStream.Position = 0L;
			switch (Version)
			{
			case 0u:
				memoryStream.WriteValueU32(16909060u);
				break;
			case 1u:
				memoryStream.WriteValueU32(33752069u);
				break;
			case 2u:
				memoryStream.WriteValueU32(50595078u);
				break;
			default:
				throw new FormatException("unsupported version " + Version);
			}
			memoryStream.WriteValueU32(value);
			memoryStream.WriteValueU32(num2);
			memoryStream.WriteValueS32(Entries.Count);
			memoryStream.WriteValueU32(24u);
			memoryStream.WriteValueU32(num);
			memoryStream.Position = 0L;
			output.WriteFromStream(memoryStream, memoryStream.Length);
		}

		public void Deserialize(Stream input)
		{
			long position = input.Position;
			if (input.Position == input.Length)
			{
				Version = 2u;
				Entries.Clear();
				DuplicateNames = false;
				return;
			}
			switch (input.ReadValueU32())
			{
			case 16909060u:
				Version = 0u;
				break;
			case 33752069u:
				Version = 1u;
				break;
			case 50595078u:
				Version = 2u;
				break;
			default:
				throw new FormatException("invalid magic");
			}
			if (Version != 2)
			{
				throw new FormatException("unsupported version " + Version);
			}
			uint num = input.ReadValueU32();
			if (position + num > input.Length)
			{
				throw new EndOfStreamException("not enough data for header");
			}
			input.Seek(-8L, SeekOrigin.Current);
			MemoryStream memoryStream = input.ReadToMemoryStream(num);
			input = null;
			memoryStream.Seek(8L, SeekOrigin.Begin);
			memoryStream.ReadValueU32();
			uint num2 = memoryStream.ReadValueU32();
			uint num3 = memoryStream.ReadValueU32();
			memoryStream.ReadValueU32();
			memoryStream.Seek(num3, SeekOrigin.Begin);
			if (memoryStream.Position + num2 * 28 > memoryStream.Length)
			{
				throw new EndOfStreamException("not enough data for file table");
			}
			List<string> list = new List<string>();
			uint[] array = new uint[num2];
			Entry[] array2 = new Entry[num2];
			for (uint num4 = 0u; num4 < num2; num4++)
			{
				array[num4] = memoryStream.ReadValueU32();
				Entry entry = new Entry();
				entry.NameHash = memoryStream.ReadValueU32();
				uint num5 = memoryStream.ReadValueU32();
				uint num6 = memoryStream.ReadValueU32();
				entry.Offset = memoryStream.ReadValueU32();
				entry.Alignment = memoryStream.ReadValueU32();
				entry.CompressionScheme = memoryStream.ReadValueEnum<CompressionScheme>();
				CompressionScheme compressionScheme = entry.CompressionScheme;
				if (compressionScheme > CompressionScheme.XBox)
				{
					throw new InvalidDataException("entry has unknown compression scheme");
				}
				if (entry.CompressionScheme == CompressionScheme.XBox)
				{
					entry.CompressedSize = num6;
					entry.UncompressedSize = num5;
				}
				else
				{
					entry.CompressedSize = num5;
					entry.UncompressedSize = num6;
				}
				if (!entry.Compressed && entry.CompressedSize != entry.UncompressedSize)
				{
					throw new InvalidDataException("entry isn't compressed yet sizes don't match");
				}
				array2[num4] = entry;
			}
			for (uint num7 = 0u; num7 < num2; num7++)
			{
				if (array[num7] > memoryStream.Length)
				{
					throw new EndOfStreamException("not enough data for file name");
				}
				memoryStream.Seek(array[num7], SeekOrigin.Begin);
				string text = memoryStream.ReadStringZ(Encoding.ASCII);
				array2[num7].Name = text;
				if (list.Contains(text))
				{
					DuplicateNames = true;
				}
				else
				{
					list.Add(text);
				}
			}
			Entries.Clear();
			Entries.AddRange(array2);
		}
	}
}
