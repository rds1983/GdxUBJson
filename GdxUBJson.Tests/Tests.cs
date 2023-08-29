using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace GdxUBJson.Tests
{
	[TestFixture]
	public class Tests
	{
		private static readonly Assembly _assembly = typeof(Tests).Assembly;

		private void TestProperty(JObject obj, string name, object value)
		{
			Assert.IsTrue(obj.ContainsKey(name));

			if (value == null)
			{
				Assert.IsTrue(obj[name].Type == JTokenType.Null);
				return;
			}

			var sourceValue = obj[name].ToObject(value.GetType());
			Assert.AreEqual(sourceValue, value);
		}

		private void TestKnight(JToken data)
		{
			Assert.IsTrue(data is JObject);
			var obj = (JObject)data;

			Assert.IsTrue(obj.Children().Count() == 6);
			Assert.IsTrue(obj.ContainsKey("version"));
			Assert.IsTrue(obj.ContainsKey("id"));
			Assert.IsTrue(obj.ContainsKey("meshes"));
			Assert.IsTrue(obj.ContainsKey("materials"));
			Assert.IsTrue(obj.ContainsKey("nodes"));
			Assert.IsTrue(obj.ContainsKey("animations"));
		}

		[Test]
		public void TestKnight()
		{
			var reader = new UBJsonReader();

			// First reader test
			JToken data;
			using (var stream = _assembly.OpenResourceStream("GdxUBJson.Tests.Resources.knight.g3db"))
			{
				data = reader.Parse(stream);
			}

			TestKnight(data);

			// Now write to memory through UBJsonWriter
			MemoryStream outputStream = new MemoryStream();
			using (var writer = new UBJsonWriter(outputStream))
			{
				writer.Value(data);
			}

			// Read again and test
			var bytes = outputStream.ToArray();
			using (var stream = new MemoryStream(bytes))
			{
				data = reader.Parse(stream);
			}

			TestKnight(data);
		}

		[Test]
		public void TestExample()
		{
			// Read json
			var data = JObject.Parse(_assembly.ReadResourceAsString("GdxUBJson.Tests.Resources.Example.json"));

			// Now write to memory through UBJsonWriter
			MemoryStream outputStream = new MemoryStream();
			using (var writer = new UBJsonWriter(outputStream))
			{
				writer.Value(data);
			}

			var bytes = outputStream.ToArray();

			// Read through UBJsonReader
			// Read again and test
			var reader = new UBJsonReader();
			JToken result;
			using (var stream = new MemoryStream(bytes))
			{
				result = reader.Parse(stream);
			}

			Assert.IsTrue(result is JObject);
			var obj = (JObject)result;

			Assert.IsTrue(obj.Children().Count() == 1);
			Assert.IsTrue(obj.ContainsKey("Actors"));

			Assert.IsTrue(obj["Actors"] is JArray);
			var actors = (JArray)obj["Actors"];
			Assert.IsTrue(actors.Children().Count() == 2);

			Assert.IsTrue(actors[0] is JObject);
			var tomCruise = (JObject)actors[0];
			TestProperty(tomCruise, "name", "Tom Cruise");
			TestProperty(tomCruise, "age", 56);
			TestProperty(tomCruise, "Born At", "Syracuse, NY");
			TestProperty(tomCruise, "Birthdate", "July 3, 1962");
			TestProperty(tomCruise, "photo", "https://jsonformatter.org/img/tom-cruise.jpg");
			TestProperty(tomCruise, "wife", null);
			TestProperty(tomCruise, "weight", 67.5);
			TestProperty(tomCruise, "hasChildren", true);
			TestProperty(tomCruise, "hasGreyHair", false);

			Assert.IsTrue(tomCruise["children"] is JArray);
			var children = (JArray)tomCruise["children"];
			Assert.AreEqual(3, children.Count());
			Assert.AreEqual(children[0].ToString(), "Suri");
			Assert.AreEqual(children[1].ToString(), "Isabella Jane");
			Assert.AreEqual(children[2].ToString(), "Connor");
		}
	}
}
