using System;
using System.IO;
using System.Text;
using Newtonsoft.Json.Linq;

namespace GdxUBJson
{

	public class UBJsonReader
	{
		private readonly bool _oldFormat;

		public UBJsonReader(bool oldFormat = true)
		{
			_oldFormat = oldFormat;
		}

		public JToken Parse(Stream stream)
		{
			using (BinaryReader din = new BinaryReader(stream))
			{
				return Parse(din, din.ReadByte());
			}
		}

		protected JToken Parse(BinaryReader din, byte type)
		{
			if (type == '[')
				return ParseArray(din);
			else if (type == '{')
				return ParseObject(din);
			else if (type == 'Z')
				return new JValue(JValue.CreateNull());
			else if (type == 'T')
				return new JValue(true);
			else if (type == 'F')
				return new JValue(false);
			else if (type == 'B')
				return new JValue(din.ReadByte());
			else if (type == 'U')
				return new JValue(din.ReadByte());
			else if (type == 'i')
				return new JValue(_oldFormat ? din.ReadInt16() : din.ReadByte());
			else if (type == 'I')
				return new JValue(_oldFormat ? din.ReadInt32() : din.ReadInt16());
			else if (type == 'l')
				return new JValue(din.ReadInt32());
			else if (type == 'L')
				return new JValue(din.ReadInt64());
			else if (type == 'd')
				return new JValue(din.ReadSingle());
			else if (type == 'D')
				return new JValue(din.ReadDouble());
			else if (type == 's' || type == 'S')
				return new JValue(ParseString(din, type));
			else if (type == 'a' || type == 'A')
				return ParseData(din, type);
			else if (type == 'C')
				return new JValue(din.ReadChar());
			else
				throw new Exception("Unrecognized data type");
		}

		protected JArray ParseArray(BinaryReader din)
		{
			var result = new JArray();
			byte type = din.ReadByte();
			byte valueType = 0;
			if (type == '$')
			{
				valueType = din.ReadByte();
				type = din.ReadByte();
			}
			long size = -1;
			if (type == '#')
			{
				size = ParseSize(din, false, -1);
				if (size < 0) throw new Exception("Unrecognized data type");
				if (size == 0) return result;
				type = valueType == 0 ? din.ReadByte() : valueType;
			}

			long c = 0;
			while (!din.IsEOF() && type != ']')
			{
				JToken val = Parse(din, type);
				result.Add(val);
				if (size > 0 && ++c >= size) break;
				type = valueType == 0 ? din.ReadByte() : valueType;
			}
			return result;
		}

		protected JObject ParseObject(BinaryReader din)
		{
			var result = new JObject();
			byte type = din.ReadByte();
			byte valueType = 0;
			if (type == '$')
			{
				valueType = din.ReadByte();
				type = din.ReadByte();
			}
			long size = -1;
			if (type == '#')
			{
				size = ParseSize(din, false, -1);
				if (size < 0) throw new Exception("Unrecognized data type");
				if (size == 0) return result;
				type = din.ReadByte();
			}

			long c = 0;
			while (!din.IsEOF() && type != '}')
			{
				string key = ParseString(din, true, type);
				JToken child = Parse(din, valueType == 0 ? din.ReadByte() : valueType);

				result[key] = child;
				if (size > 0 && ++c >= size) break;
				type = din.ReadByte();
			}
			return result;
		}

		protected JArray ParseData(BinaryReader din, byte blockType)
		{
			// FIXME: a/A is currently not following the specs because it lacks strong typed, fixed sized containers,
			// see: https://github.com/thebuzzmedia/universal-binary-json/issues/27
			byte dataType = din.ReadByte();
			long size = blockType == 'A' ? din.ReadUInt32() : din.ReadByte();
			var result = new JArray();
			for (long i = 0; i < size; i++)
			{
				JToken val = Parse(din, dataType);
				result.Add(val);
			}
			return result;
		}

		protected string ParseString(BinaryReader din, byte type)
		{
			return ParseString(din, false, type);
		}

		protected string ParseString(BinaryReader din, bool sOptional, byte type)
		{
			long size = -1;
			if (type == 'S')
			{
				size = ParseSize(din, true, -1);
			}
			else if (type == 's')
				size = din.ReadByte();
			else if (sOptional) size = ParseSize(din, type, false, -1);
			if (size < 0) throw new Exception("Unrecognized data type, string expected");
			return size > 0 ? ReadString(din, size) : "";
		}

		protected long ParseSize(BinaryReader din, bool useIntOnError, long defaultValue)
		{
			return ParseSize(din, din.ReadByte(), useIntOnError, defaultValue);
		}

		protected long ParseSize(BinaryReader din, byte type, bool useIntOnError, long defaultValue)
		{
			if (type == 'i') return din.ReadByte();
			if (type == 'I') return din.ReadUInt16();
			if (type == 'l') return din.ReadUInt32();
			if (type == 'L') return din.ReadInt64();
			if (useIntOnError)
			{
				long result = type << 24;
				result |= (long)(din.ReadByte() << 16);
				result |= (long)(din.ReadByte() << 8);
				result |= din.ReadByte();
				return result;
			}
			return defaultValue;
		}

		protected string ReadString(BinaryReader din, long size)
		{
			var data = new byte[(int)size];
			din.Read(data, 0, data.Length);
			return Encoding.UTF8.GetString(data);
		}
	}
}