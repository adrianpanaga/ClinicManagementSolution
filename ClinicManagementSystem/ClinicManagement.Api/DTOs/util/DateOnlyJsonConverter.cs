﻿using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ClinicManagement.Api.DTOs.util
{
    /// <summary>
    /// Custom JsonConverter for System.DateOnly to handle JSON serialization/deserialization.
    /// This is necessary because DateOnly does not have a default JSON conversion behavior.
    /// </summary>
    public class DateOnlyJsonConverter : JsonConverter<DateOnly>
    {
        private const string DateFormat = "yyyy-MM-dd";

        public override DateOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return DateOnly.ParseExact(reader.GetString()!, DateFormat);
        }

        public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString(DateFormat));
        }
    }
}