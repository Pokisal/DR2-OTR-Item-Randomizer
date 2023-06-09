using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace NDesk.Options
{
	public class OptionSet : KeyedCollection<string, Option>
	{
		private sealed class ActionOption : Option
		{
			private Action<OptionValueCollection> action;

			public ActionOption(string prototype, string description, int count, Action<OptionValueCollection> action)
				: base(prototype, description, count)
			{
				if (action == null)
				{
					throw new ArgumentNullException("action");
				}
				this.action = action;
			}

			protected override void OnParseComplete(OptionContext c)
			{
				action(c.OptionValues);
			}
		}

		private sealed class ActionOption<T> : Option
		{
			private Action<T> action;

			public ActionOption(string prototype, string description, Action<T> action)
				: base(prototype, description, 1)
			{
				if (action == null)
				{
					throw new ArgumentNullException("action");
				}
				this.action = action;
			}

			protected override void OnParseComplete(OptionContext c)
			{
				action(Option.Parse<T>(c.OptionValues[0], c));
			}
		}

		private sealed class ActionOption<TKey, TValue> : Option
		{
			private OptionAction<TKey, TValue> action;

			public ActionOption(string prototype, string description, OptionAction<TKey, TValue> action)
				: base(prototype, description, 2)
			{
				if (action == null)
				{
					throw new ArgumentNullException("action");
				}
				this.action = action;
			}

			protected override void OnParseComplete(OptionContext c)
			{
				action(Option.Parse<TKey>(c.OptionValues[0], c), Option.Parse<TValue>(c.OptionValues[1], c));
			}
		}

		private const int OptionWidth = 29;

		private Converter<string, string> localizer;

		private readonly Regex ValueOption = new Regex("^(?<flag>--|-|/)(?<name>[^:=]+)((?<sep>[:=])(?<value>.*))?$");

		public Converter<string, string> MessageLocalizer => localizer;

		public OptionSet()
			: this((string f) => f)
		{
		}

		public OptionSet(Converter<string, string> localizer)
		{
			this.localizer = localizer;
		}

		protected override string GetKeyForItem(Option item)
		{
			if (item == null)
			{
				throw new ArgumentNullException("option");
			}
			if (item.Names != null && item.Names.Length != 0)
			{
				return item.Names[0];
			}
			throw new InvalidOperationException("Option has no names!");
		}

		[Obsolete("Use KeyedCollection.this[string]")]
		protected Option GetOptionForName(string option)
		{
			if (option == null)
			{
				throw new ArgumentNullException("option");
			}
			try
			{
				return base[option];
			}
			catch (KeyNotFoundException)
			{
				return null;
			}
		}

		protected override void InsertItem(int index, Option item)
		{
			base.InsertItem(index, item);
			AddImpl(item);
		}

		protected override void RemoveItem(int index)
		{
			base.RemoveItem(index);
			Option option = base.Items[index];
			for (int i = 1; i < option.Names.Length; i++)
			{
				base.Dictionary.Remove(option.Names[i]);
			}
		}

		protected override void SetItem(int index, Option item)
		{
			base.SetItem(index, item);
			RemoveItem(index);
			AddImpl(item);
		}

		private void AddImpl(Option option)
		{
			if (option == null)
			{
				throw new ArgumentNullException("option");
			}
			List<string> list = new List<string>(option.Names.Length);
			try
			{
				for (int i = 1; i < option.Names.Length; i++)
				{
					base.Dictionary.Add(option.Names[i], option);
					list.Add(option.Names[i]);
				}
			}
			catch (Exception)
			{
				foreach (string item in list)
				{
					base.Dictionary.Remove(item);
				}
				throw;
			}
		}

		public new OptionSet Add(Option option)
		{
			base.Add(option);
			return this;
		}

		public OptionSet Add(string prototype, Action<string> action)
		{
			return Add(prototype, null, action);
		}

		public OptionSet Add(string prototype, string description, Action<string> action)
		{
			if (action == null)
			{
				throw new ArgumentNullException("action");
			}
			Option item = new ActionOption(prototype, description, 1, delegate(OptionValueCollection v)
			{
				action(v[0]);
			});
			base.Add(item);
			return this;
		}

		public OptionSet Add(string prototype, OptionAction<string, string> action)
		{
			return Add(prototype, null, action);
		}

		public OptionSet Add(string prototype, string description, OptionAction<string, string> action)
		{
			if (action == null)
			{
				throw new ArgumentNullException("action");
			}
			Option item = new ActionOption(prototype, description, 2, delegate(OptionValueCollection v)
			{
				action(v[0], v[1]);
			});
			base.Add(item);
			return this;
		}

		public OptionSet Add<T>(string prototype, Action<T> action)
		{
			return Add(prototype, null, action);
		}

		public OptionSet Add<T>(string prototype, string description, Action<T> action)
		{
			return Add(new ActionOption<T>(prototype, description, action));
		}

		public OptionSet Add<TKey, TValue>(string prototype, OptionAction<TKey, TValue> action)
		{
			return Add(prototype, null, action);
		}

		public OptionSet Add<TKey, TValue>(string prototype, string description, OptionAction<TKey, TValue> action)
		{
			return Add(new ActionOption<TKey, TValue>(prototype, description, action));
		}

		protected virtual OptionContext CreateOptionContext()
		{
			return new OptionContext(this);
		}

		public List<string> Parse(IEnumerable<string> arguments)
		{
			bool process = true;
			OptionContext c = CreateOptionContext();
			c.OptionIndex = -1;
			Option def = GetOptionForName("<>");
			List<string> result = arguments.Where(delegate(string argument)
			{
				if (++c.OptionIndex >= 0 && (process || def != null))
				{
					if (process)
					{
						if (!(argument == "--"))
						{
							if (!Parse(argument, c))
							{
								if (def != null)
								{
									return Unprocessed(null, def, c, argument);
								}
								return true;
							}
							return false;
						}
						return process = false;
					}
					if (def != null)
					{
						return Unprocessed(null, def, c, argument);
					}
					return true;
				}
				return true;
			}).ToList();
			if (c.Option != null)
			{
				c.Option.Invoke(c);
			}
			return result;
		}

		private static bool Unprocessed(ICollection<string> extra, Option def, OptionContext c, string argument)
		{
			if (def == null)
			{
				extra.Add(argument);
				return false;
			}
			c.OptionValues.Add(argument);
			c.Option = def;
			c.Option.Invoke(c);
			return false;
		}

		protected bool GetOptionParts(string argument, out string flag, out string name, out string sep, out string value)
		{
			if (argument == null)
			{
				throw new ArgumentNullException("argument");
			}
			flag = (name = (sep = (value = null)));
			Match match = ValueOption.Match(argument);
			if (!match.Success)
			{
				return false;
			}
			flag = match.Groups["flag"].Value;
			name = match.Groups["name"].Value;
			if (match.Groups["sep"].Success && match.Groups["value"].Success)
			{
				sep = match.Groups["sep"].Value;
				value = match.Groups["value"].Value;
			}
			return true;
		}

		protected virtual bool Parse(string argument, OptionContext c)
		{
			if (c.Option != null)
			{
				ParseValue(argument, c);
				return true;
			}
			if (!GetOptionParts(argument, out var flag, out var name, out var sep, out var value))
			{
				return false;
			}
			if (Contains(name))
			{
				Option option = base[name];
				c.OptionName = flag + name;
				c.Option = option;
				switch (option.OptionValueType)
				{
				case OptionValueType.None:
					c.OptionValues.Add(name);
					c.Option.Invoke(c);
					break;
				case OptionValueType.Optional:
				case OptionValueType.Required:
					ParseValue(value, c);
					break;
				}
				return true;
			}
			if (ParseBool(argument, name, c))
			{
				return true;
			}
			if (ParseBundledValue(flag, string.Concat(name + sep + value), c))
			{
				return true;
			}
			return false;
		}

		private void ParseValue(string option, OptionContext c)
		{
			if (option != null)
			{
				string[] array = ((c.Option.ValueSeparators != null) ? option.Split(c.Option.ValueSeparators, StringSplitOptions.None) : new string[1] { option });
				string[] array2 = array;
				foreach (string item in array2)
				{
					c.OptionValues.Add(item);
				}
			}
			if (c.OptionValues.Count == c.Option.MaxValueCount || c.Option.OptionValueType == OptionValueType.Optional)
			{
				c.Option.Invoke(c);
			}
			else if (c.OptionValues.Count > c.Option.MaxValueCount)
			{
				throw new OptionException(localizer($"Error: Found {c.OptionValues.Count} option values when expecting {c.Option.MaxValueCount}."), c.OptionName);
			}
		}

		private bool ParseBool(string option, string n, OptionContext c)
		{
			string key;
			if (n.Length >= 1 && (n[n.Length - 1] == '+' || n[n.Length - 1] == '-') && Contains(key = n.Substring(0, n.Length - 1)))
			{
				Option option2 = base[key];
				string item = ((n[n.Length - 1] == '+') ? option : null);
				c.OptionName = option;
				c.Option = option2;
				c.OptionValues.Add(item);
				option2.Invoke(c);
				return true;
			}
			return false;
		}

		private bool ParseBundledValue(string f, string n, OptionContext c)
		{
			if (f != "-")
			{
				return false;
			}
			for (int i = 0; i < n.Length; i++)
			{
				string text = f + n[i];
				string key = n[i].ToString();
				if (!Contains(key))
				{
					if (i == 0)
					{
						return false;
					}
					throw new OptionException(string.Format(localizer("Cannot bundle unregistered option '{0}'."), text), text);
				}
				Option option = base[key];
				switch (option.OptionValueType)
				{
				case OptionValueType.Optional:
				case OptionValueType.Required:
				{
					string text2 = n.Substring(i + 1);
					c.Option = option;
					c.OptionName = text;
					ParseValue((text2.Length != 0) ? text2 : null, c);
					return true;
				}
				default:
					throw new InvalidOperationException("Unknown OptionValueType: " + option.OptionValueType);
				case OptionValueType.None:
					break;
				}
				Invoke(c, text, n, option);
			}
			return true;
		}

		private static void Invoke(OptionContext c, string name, string value, Option option)
		{
			c.OptionName = name;
			c.Option = option;
			c.OptionValues.Add(value);
			option.Invoke(c);
		}

		public void WriteOptionDescriptions(TextWriter o)
		{
			using IEnumerator<Option> enumerator = GetEnumerator();
			while (enumerator.MoveNext())
			{
				Option current = enumerator.Current;
				int written = 0;
				if (!WriteOptionPrototype(o, current, ref written))
				{
					continue;
				}
				if (written < 29)
				{
					o.Write(new string(' ', 29 - written));
				}
				else
				{
					o.WriteLine();
					o.Write(new string(' ', 29));
				}
				bool flag = false;
				string value = new string(' ', 31);
				foreach (string line in GetLines(localizer(GetDescription(current.Description))))
				{
					if (flag)
					{
						o.Write(value);
					}
					o.WriteLine(line);
					flag = true;
				}
			}
		}

		private bool WriteOptionPrototype(TextWriter o, Option p, ref int written)
		{
			string[] names = p.Names;
			int nextOptionIndex = GetNextOptionIndex(names, 0);
			if (nextOptionIndex == names.Length)
			{
				return false;
			}
			if (names[nextOptionIndex].Length == 1)
			{
				Write(o, ref written, "  -");
				Write(o, ref written, names[0]);
			}
			else
			{
				Write(o, ref written, "      --");
				Write(o, ref written, names[0]);
			}
			for (nextOptionIndex = GetNextOptionIndex(names, nextOptionIndex + 1); nextOptionIndex < names.Length; nextOptionIndex = GetNextOptionIndex(names, nextOptionIndex + 1))
			{
				Write(o, ref written, ", ");
				Write(o, ref written, (names[nextOptionIndex].Length == 1) ? "-" : "--");
				Write(o, ref written, names[nextOptionIndex]);
			}
			if (p.OptionValueType == OptionValueType.Optional || p.OptionValueType == OptionValueType.Required)
			{
				if (p.OptionValueType == OptionValueType.Optional)
				{
					Write(o, ref written, localizer("["));
				}
				Write(o, ref written, localizer("=" + GetArgumentName(0, p.MaxValueCount, p.Description)));
				string text = ((p.ValueSeparators != null && p.ValueSeparators.Length != 0) ? p.ValueSeparators[0] : " ");
				for (int i = 1; i < p.MaxValueCount; i++)
				{
					Write(o, ref written, localizer(text + GetArgumentName(i, p.MaxValueCount, p.Description)));
				}
				if (p.OptionValueType == OptionValueType.Optional)
				{
					Write(o, ref written, localizer("]"));
				}
			}
			return true;
		}

		private static int GetNextOptionIndex(string[] names, int i)
		{
			while (i < names.Length && names[i] == "<>")
			{
				i++;
			}
			return i;
		}

		private static void Write(TextWriter o, ref int n, string s)
		{
			n += s.Length;
			o.Write(s);
		}

		private static string GetArgumentName(int index, int maxIndex, string description)
		{
			if (description == null)
			{
				if (maxIndex != 1)
				{
					return "VALUE" + (index + 1);
				}
				return "VALUE";
			}
			string[] array = ((maxIndex != 1) ? new string[1] { "{" + index + ":" } : new string[2] { "{0:", "{" });
			for (int i = 0; i < array.Length; i++)
			{
				int num = 0;
				int num2;
				do
				{
					num2 = description.IndexOf(array[i], num);
				}
				while (num2 >= 0 && num != 0 && description[num++ - 1] == '{');
				if (num2 != -1)
				{
					int num3 = description.IndexOf("}", num2);
					if (num3 != -1)
					{
						return description.Substring(num2 + array[i].Length, num3 - num2 - array[i].Length);
					}
				}
			}
			if (maxIndex != 1)
			{
				return "VALUE" + (index + 1);
			}
			return "VALUE";
		}

		private static string GetDescription(string description)
		{
			if (description == null)
			{
				return string.Empty;
			}
			StringBuilder stringBuilder = new StringBuilder(description.Length);
			int num = -1;
			for (int i = 0; i < description.Length; i++)
			{
				switch (description[i])
				{
				case '{':
					if (i == num)
					{
						stringBuilder.Append('{');
						num = -1;
					}
					else if (num < 0)
					{
						num = i + 1;
					}
					continue;
				case '}':
					if (num < 0)
					{
						if (i + 1 == description.Length || description[i + 1] != '}')
						{
							throw new InvalidOperationException("Invalid option description: " + description);
						}
						i++;
						stringBuilder.Append("}");
					}
					else
					{
						stringBuilder.Append(description.Substring(num, i - num));
						num = -1;
					}
					continue;
				case ':':
					if (num >= 0)
					{
						num = i + 1;
						continue;
					}
					break;
				}
				if (num < 0)
				{
					stringBuilder.Append(description[i]);
				}
			}
			return stringBuilder.ToString();
		}

		private static IEnumerable<string> GetLines(string description)
		{
			if (string.IsNullOrEmpty(description))
			{
				yield return string.Empty;
				yield break;
			}
			int length = 50;
			int num = 0;
			int end;
			do
			{
				end = GetLineEnd(num, length, description);
				char c = description[end - 1];
				if (char.IsWhiteSpace(c))
				{
					end--;
				}
				yield return string.Concat(str1: (end != description.Length && !IsEolChar(c)) ? "-" : "", str0: description.Substring(num, end - num));
				num = end;
				if (char.IsWhiteSpace(c))
				{
					num++;
				}
				length = 48;
			}
			while (end < description.Length);
		}

		private static bool IsEolChar(char c)
		{
			return !char.IsLetterOrDigit(c);
		}

		private static int GetLineEnd(int start, int length, string description)
		{
			int num = Math.Min(start + length, description.Length);
			int num2 = -1;
			for (int i = start + 1; i < num; i++)
			{
				if (description[i] == '\n')
				{
					return i + 1;
				}
				if (IsEolChar(description[i]))
				{
					num2 = i + 1;
				}
			}
			if (num2 == -1 || num == description.Length)
			{
				return num;
			}
			return num2;
		}
	}
}
