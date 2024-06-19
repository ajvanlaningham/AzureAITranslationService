using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

public static class Constants
{
    public static readonly string ResxSchemaPath = GetFilePath("AzureAITranslatorService", "Resources", "ResxSchema.txt");

    private static string GetFilePath(params string[] paths)
    {
        throw new NotImplementedException();
        //TODO: fix this shit
    }
}