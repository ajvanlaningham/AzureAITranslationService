using System;
using System.IO;
using System.Reflection;

public static class Constants
{
    //A note on exporting files with vsix:
    //In the csproj file, the content resource must have the
    //<IncludeInVSIX>true</IncludeInVSIX> tag. 
    public static readonly string ResxSchemaPath = GetFilePath("Resources", "ResxSchema.txt");

    private static string GetFilePath(params string[] paths)
    {
        string assemblyLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        string debugResourcesPath = Path.Combine(assemblyLocation, "Resources", "ResxSchema.txt");

        if (File.Exists(debugResourcesPath))
        {
            return debugResourcesPath;
        }

        string filePath = Path.Combine(assemblyLocation, Path.Combine(paths));
        if (File.Exists(filePath))
        {
            return filePath;
        }

        string parentDirectory = Directory.GetParent(assemblyLocation).FullName;
        string parentFilePath = Path.Combine(parentDirectory, Path.Combine(paths));
        if (File.Exists(parentFilePath))
        {
            return parentFilePath;
        }

        string devDirectory = Directory.GetParent(assemblyLocation).Parent.Parent.Parent.FullName;
        string devFilePath = Path.Combine(devDirectory, Path.Combine(paths));
        if (File.Exists(devFilePath))
        {
            return devFilePath;
        }

        System.Diagnostics.Debug.WriteLine($"File not found in: {debugResourcesPath}");
        System.Diagnostics.Debug.WriteLine($"File not found in: {filePath}");
        System.Diagnostics.Debug.WriteLine($"File not found in: {parentFilePath}");
        System.Diagnostics.Debug.WriteLine($"File not found in: {devFilePath}");

        throw new FileNotFoundException($"Schema file not found in any checked locations: {debugResourcesPath}, {filePath}, {parentFilePath}, {devFilePath}");
    }
}