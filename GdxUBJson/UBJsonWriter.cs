using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json.Linq;

namespace GdxUBJson
{
	public class UBJsonWriter: IDisposable
	{
		public class JsonObject
		{
			private readonly BinaryWriter _output;
			public readonly bool array;

			public JsonObject(BinaryWriter output, bool array)
			{
				_output = output;
				this.array = array;
				_output.Write((byte)(array ? '[' : '{'));
			}

			public void close()
			{
				_output.Write((byte)(array ? ']' : '}'));
			}
		}

		private BinaryWriter _output;

		private JsonObject current;
		private bool named;
		private readonly Stack<JsonObject> stack = new Stack<JsonObject>();

		public UBJsonWriter(Stream output)
		{
			_output = new BinaryWriter(output);
		}

		public void Dispose()
		{
			if (_output != null)
			{
				_output.Dispose();
				_output = null;
			}
		}

		public UBJsonWriter Object()
		{
			if (current != null)
			{
				if (!current.array)
				{
					if (!named) throw new Exception("Name must be set.");
					named = false;
				}
			}

			current = new JsonObject(_output, false);
			stack.Push(current);
			return this;
		}

		public UBJsonWriter Object(string name)
		{
			Name(name).Object();
			return this;
		}

		public UBJsonWriter Array()
		{
			if (current != null)
			{
				if (!current.array)
				{
					if (!named) throw new Exception("Name must be set.");
					named = false;
				}
			}
			stack.Push(current = new JsonObject(_output, true));
			return this;
		}

		public UBJsonWriter Array(string name)
		{
			Name(name).Array();
			return this;
		}

		public UBJsonWriter Name(string name)
		{
			if (current == null || current.array) throw new Exception("Current item must be an object.");
			byte[] bytes = Encoding.UTF8.GetBytes(name);
			if (bytes.Length <= byte.MaxValue)
			{
				_output.Write((byte)'i');
				_output.Write((byte)bytes.Length);
			}
			else if (bytes.Length <= short.MaxValue)
			{
				_output.Write((byte)'I');
				_output.Write((short)bytes.Length);
			}
			else
			{
				_output.Write((byte)'l');
				_output.Write(bytes.Length);
			}
			_output.Write(bytes);
			named = true;
			return this;
		}

		public UBJsonWriter Value(byte value)
		{
			CheckName();
			_output.Write((byte)'i');
			_output.Write(value);
			return this;
		}

		public UBJsonWriter Value(short value)
		{
			CheckName();
			_output.Write((byte)'I');
			_output.Write(value);
			return this;
		}

		public UBJsonWriter Value(int value)
		{
			CheckName();
			_output.Write((byte)'l');
			_output.Write(value);
			return this;
		}

		public UBJsonWriter Value(long value)
		{
			CheckName();
			_output.Write((byte)'L');
			_output.Write(value);
			return this;
		}

		public UBJsonWriter Value(float value)
		{
			CheckName();
			_output.Write((byte)'d');
			_output.Write((float)value);
			return this;
		}

		public UBJsonWriter Value(double value)
		{
			CheckName();
			_output.Write((byte)'D');
			_output.Write((double)value);
			return this;
		}

		public UBJsonWriter Value(bool value)
		{
			CheckName();
			_output.Write((byte)(value ? 'T' : 'F'));
			return this;
		}

		public UBJsonWriter Value(char value)
		{
			CheckName();
			_output.Write((byte)'I');
			_output.Write(value);
			return this;
		}

		public UBJsonWriter Value(string value)
		{
			CheckName();
			byte[] bytes = Encoding.UTF8.GetBytes(value);
			_output.Write((byte)'S');
			if (bytes.Length <= byte.MaxValue)
			{
				_output.Write((byte)'i');
				_output.Write((byte)bytes.Length);
			}
			else if (bytes.Length <= short.MaxValue)
			{
				_output.Write((byte)'I');
				_output.Write((short)bytes.Length);
			}
			else
			{
				_output.Write((byte)'l');
				_output.Write(bytes.Length);
			}
			_output.Write(bytes);
			return this;
		}

		public UBJsonWriter Value(byte[] values)
		{
			Array();
			_output.Write((byte)'$');
			_output.Write((byte)'i');
			_output.Write((byte)'#');
			Value(values.Length);
			for (int i = 0, n = values.Length; i < n; i++)
			{
				_output.Write((byte)values[i]);
			}
			Pop(true);
			return this;
		}

		public UBJsonWriter Value(short[] values)
		{
			Array();
			_output.Write((byte)'$');
			_output.Write((byte)'I');
			_output.Write((byte)'#');
			Value(values.Length);
			for (int i = 0, n = values.Length; i < n; i++)
			{
				_output.Write((short)values[i]);
			}
			Pop(true);
			return this;
		}

		public UBJsonWriter Value(int[] values)
		{
			Array();
			_output.Write((byte)'$');
			_output.Write((byte)'l');
			_output.Write((byte)'#');
			Value(values.Length);
			for (int i = 0, n = values.Length; i < n; i++)
			{
				_output.Write((int)values[i]);
			}
			Pop(true);
			return this;
		}

		public UBJsonWriter Value(long[] values)
		{
			Array();
			_output.Write((byte)'$');
			_output.Write((byte)'L');
			_output.Write((byte)'#');
			Value(values.Length);
			for (int i = 0, n = values.Length; i < n; i++)
			{
				_output.Write((long)values[i]);
			}
			Pop(true);
			return this;
		}

		public UBJsonWriter Value(float[] values)
		{
			Array();
			_output.Write((byte)'$');
			_output.Write((byte)'d');
			_output.Write((byte)'#');
			Value(values.Length);
			for (int i = 0, n = values.Length; i < n; i++)
			{
				_output.Write((float)values[i]);
			}
			Pop(true);
			return this;
		}

		public UBJsonWriter Value(double[] values)
		{
			Array();
			_output.Write((byte)'$');
			_output.Write((byte)'D');
			_output.Write((byte)'#');
			Value(values.Length);
			for (int i = 0, n = values.Length; i < n; i++)
			{
				_output.Write((double)values[i]);
			}
			Pop(true);
			return this;
		}

		public UBJsonWriter Value(bool[] values)
		{
			Array();
			for (int i = 0, n = values.Length; i < n; i++)
			{
				_output.Write((byte)(values[i] ? 'T' : 'F'));
			}
			Pop();
			return this;
		}

		public UBJsonWriter Value(char[] values)
		{
			Array();
			_output.Write((byte)'$');
			_output.Write((byte)'C');
			_output.Write((byte)'#');
			Value(values.Length);
			for (int i = 0, n = values.Length; i < n; i++)
			{
				_output.Write((char)values[i]);
			}
			Pop(true);
			return this;
		}

		public UBJsonWriter Value(String[] values)
		{
			Array();
			_output.Write((byte)'$');
			_output.Write((byte)'S');
			_output.Write((byte)'#');
			Value(values.Length);
			for (int i = 0, n = values.Length; i < n; i++)
			{
				byte[] bytes = Encoding.UTF8.GetBytes(values[i]);
				if (bytes.Length <= byte.MaxValue)
				{
					_output.Write((byte)'i');
					_output.Write((byte)bytes.Length);
				}
				else if (bytes.Length <= short.MaxValue)
				{
					_output.Write((byte)'I');
					_output.Write((short)bytes.Length);
				}
				else
				{
					_output.Write((byte)'l');
					_output.Write((int)bytes.Length);
				}
				_output.Write(bytes);
			}
			Pop(true);
			return this;
		}

		public UBJsonWriter Value(JToken value, string name = null)
		{
			switch (value.Type)
			{
				case JTokenType.Object:
					if (name != null)
						Object(name);
					else
						Object();

					var obj = (JObject)value;
					foreach (var pair in obj)
					{
						Value(pair.Value, pair.Key);

					}
					Pop();
					break;
				case JTokenType.Property:
					if (name != null)
						Object(name);
					else
						Object();

					var prop = (JProperty)value;
					Value(prop.Value, prop.Name);
					Pop();
					break;
				case JTokenType.Array:
					if (name != null)
						Array(name);
					else
						Array();

					foreach (var child in value.Children())
					{
						Value(child);
					}
					Pop();
					break;
				case JTokenType.Integer:
					if (name != null) Name(name);
					Value((long)value);
					break;
				case JTokenType.Float:
					if (name != null) Name(name);
					Value((float)value);
					break;
				case JTokenType.String:
					if (name != null) Name(name);
					Value((string)value);
					break;
				case JTokenType.Boolean:
					if (name != null) Name(name);
					Value((bool)value);
					break;
				case JTokenType.Null:
					if (name != null) Name(name);
					Value();
					break;
				default:
					throw new Exception($"Unhandled JValue type {value.Type}");
			}

			return this;
		}

		public UBJsonWriter Value()
		{
			CheckName();
			_output.Write((byte)'Z');
			return this;
		}

		public UBJsonWriter Set(string name, byte value)
		{
			return Name(name).Value(value);
		}

		public UBJsonWriter Set(string name, short value)
		{
			return Name(name).Value(value);
		}

		public UBJsonWriter Set(string name, int value)
		{
			return Name(name).Value(value);
		}

		public UBJsonWriter Set(string name, long value)
		{
			return Name(name).Value(value);
		}

		public UBJsonWriter Set(string name, float value)
		{
			return Name(name).Value(value);
		}

		public UBJsonWriter Set(string name, double value)
		{
			return Name(name).Value(value);
		}

		public UBJsonWriter Set(string name, bool value)
		{
			return Name(name).Value(value);
		}

		public UBJsonWriter Set(string name, char value)
		{
			return Name(name).Value(value);
		}

		public UBJsonWriter Set(string name, string value)
		{
			return Name(name).Value(value);
		}

		public UBJsonWriter Set(string name, byte[] value)
		{
			return Name(name).Value(value);
		}

		public UBJsonWriter Set(string name, short[] value)
		{
			return Name(name).Value(value);
		}

		public UBJsonWriter Set(string name, int[] value)
		{
			return Name(name).Value(value);
		}

		public UBJsonWriter Set(string name, long[] value)
		{
			return Name(name).Value(value);
		}

		public UBJsonWriter Set(string name, float[] value)
		{
			return Name(name).Value(value);
		}

		public UBJsonWriter Set(string name, double[] value)
		{
			return Name(name).Value(value);
		}

		public UBJsonWriter Set(string name, bool[] value)
		{
			return Name(name).Value(value);
		}

		public UBJsonWriter Set(string name, char[] value)
		{
			return Name(name).Value(value);
		}

		public UBJsonWriter Set(string name, String[] value)
		{
			return Name(name).Value(value);
		}

		public UBJsonWriter Set(string name)
		{
			return Name(name).Value();
		}

		private void CheckName()
		{
			if (current != null)
			{
				if (!current.array)
				{
					if (!named) throw new Exception("Name must be set.");
					named = false;
				}
			}
		}

		public UBJsonWriter Pop()
		{
			return Pop(false);
		}

		protected UBJsonWriter Pop(bool silent)
		{
			if (named) throw new Exception("Expected an object, array, or value since a name was set.");

			if (silent)
				stack.Pop();
			else
				stack.Pop().close();
			current = stack.Count == 0 ? null : stack.Peek();
			return this;
		}

		/** Flushes the underlying stream. This forces any buffered output bytes to be written out to the stream. */
		public void Flush()
		{
			_output.Flush();
		}
	}
}