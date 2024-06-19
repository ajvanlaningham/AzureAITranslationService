using System;
using System.IO;

public static class Constants
{
    public static readonly string ResxSchemaPath = GetFilePath("Resources", "ResxSchema.txt");

    private static string GetFilePath(params string[] paths)
    {
        return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Path.Combine(paths));
    }
}