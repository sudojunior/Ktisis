﻿using System;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;

using Dalamud.Logging;

using Ktisis.Structs.Poses;

namespace Ktisis.Data.Serialization.Converters {

	internal class QuaternionConverter : JsonConverter<Quaternion> {
		public override Quaternion Read(ref Utf8JsonReader reader, Type type, JsonSerializerOptions options) {
			var str = reader.GetString() ?? "";
			var split = str.Split(",");
			return new Quaternion(
				float.Parse(split[0]),
				float.Parse(split[1]),
				float.Parse(split[2]),
				float.Parse(split[3])
			);
		}

		public override void Write(Utf8JsonWriter writer, Quaternion value, JsonSerializerOptions options) {
			writer.WriteStringValue($"{value.X}, {value.Y}, {value.Z}, {value.W}");
		}
	}

	internal class Vector3Converter : JsonConverter<Vector3> {
		public override Vector3 Read(ref Utf8JsonReader reader, Type type, JsonSerializerOptions options) {
			var str = reader.GetString() ?? "";
			var split = str.Split(",");
			return new Vector3(
				float.Parse(split[0]),
				float.Parse(split[1]),
				float.Parse(split[2])
			);
		}

		public override void Write(Utf8JsonWriter writer, Vector3 value, JsonSerializerOptions options) {
			writer.WriteStringValue($"{value.X}, {value.Y}, {value.Z}");
		}
	}

	internal class TransformConverter : JsonConverter<Transform> {
		// i despise this
		public override Transform Read(ref Utf8JsonReader reader, Type type, JsonSerializerOptions options) {
			var result = new Transform();

			reader.Read();
			for (var i = 0; i < 3; i++) {
				if (reader.TokenType == JsonTokenType.EndObject)
					break;

				var prop = reader.GetString();
				reader.Read();

				if (prop == "Rotation") {
					result.Rotation = ((QuaternionConverter)JsonParser.GetConverter<Quaternion>()).Read(ref reader, type, options);
				} else {
					var vec = ((Vector3Converter)JsonParser.GetConverter<Vector3>()).Read(ref reader, type, options);
					if (prop == "Position")
						result.Position = vec;
					else if (prop == "Scale")
						result.Scale = vec;
				}

				reader.Read();
			}

			return result;
		}

		public override void Write(Utf8JsonWriter writer, Transform value, JsonSerializerOptions options) {
			writer.WriteStartObject();
			foreach (var prop in typeof(Transform).GetFields()) {
				writer.WritePropertyName(prop.Name);

				// did I complain about how bad this is yet?
				var val = prop.GetValue(value);
				if (val is Vector3)
					((Vector3Converter)JsonParser.GetConverter<Vector3>()).Write(writer, (Vector3)val, options);
				else if (val is Quaternion)
					((QuaternionConverter)JsonParser.GetConverter<Quaternion>()).Write(writer, (Quaternion)val, options);
			}
			writer.WriteEndObject();
		}
	}
}