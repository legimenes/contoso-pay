using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace Gateway.API;

public static class ApiMetadata
{
    private static readonly Assembly ApiAssembly = typeof(ApiMetadata).Assembly;

    public const string Name = "Contoso Pay";

    public static string ApplicationVersion =>
        ApiAssembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion
        ?? ApiAssembly.GetName().Version?.ToString()
        ?? "Unknown";

    public static string RuntimeVersion =>
        RuntimeInformation.FrameworkDescription;

    public static string TargetFramework =>
        ApiAssembly.GetCustomAttribute<TargetFrameworkAttribute>()
            ?.FrameworkName
        ?? "Unknown";
}