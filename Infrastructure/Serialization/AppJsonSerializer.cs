using System.Diagnostics;
using System.Text.Json;
using MicPic.Infrastructure.Exceptions;

#pragma warning disable CA1031 // Do not catch general exception types

namespace MicPic.Infrastructure.Serialization;

public static class AppJsonSerializer
{
    public static string Serialize<T>(T value)
        => JsonSerializer.Serialize(value, AppJsonSerializerOptions.Default);

    public static string? SerializeOrNull<T>(T? value)
        => value == null ? null : JsonSerializer.Serialize(value, AppJsonSerializerOptions.Default);

    public static T Deserialize<T>(string value)
        => JsonSerializer.Deserialize<T>(value, AppJsonSerializerOptions.Default)
            ?? throw new AppException(BusinessErrorCodes.NoData);

    public static bool TryDeserialize<T>(string value, out T result)
    {
        var res = DeserializeOrDefault<T>(value);
        if (res is null)
        {
            result = default!;
            return false;
        }
        result = res;
        return true;
    }

    [DebuggerStepThrough]
    public static T? DeserializeOrDefault<T>(string value)
    {
        try
        {
            return JsonSerializer.Deserialize<T>(value, AppJsonSerializerOptions.Default) ?? default;
        }
        catch
        {
            return default;
        }
    }

    [DebuggerStepThrough]
    public static T? DeserializeOrDefault<T>(string value, T defaultValue)
    {
        try
        {
            return JsonSerializer.Deserialize<T>(value, AppJsonSerializerOptions.Default) ?? defaultValue;
        }
        catch
        {
            return defaultValue;
        }
    }

    public static T DeepCopy<T>(T value) where T : new()
        => JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(value, AppJsonSerializerOptions.Default), AppJsonSerializerOptions.Default) ?? new();
}
