using System.IO;
using Newtonsoft.Json.Linq;

namespace GdxUBJson
{
	internal static class Utility
	{
		public static bool IsEOF(this BinaryReader reader)
		{
			try
			{
				var b = reader.ReadByte();
				reader.BaseStream.Seek(-1, SeekOrigin.Current);
				return false;
			}
			catch (EndOfStreamException)
			{
				return true;
			}
		}
	}
}
