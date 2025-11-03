using System.ComponentModel;
using System.IO;
using System.Reflection;

namespace YTDLManager.Helpers;

public static class EnumHelper
{
    public static string GetDescription(this Enum value)
    {
        FieldInfo? field = value.GetType().GetField(value.ToString());
        
        if (field != null)
        {
            DescriptionAttribute? attribute = field.GetCustomAttribute<DescriptionAttribute>();
            if (attribute != null)
            {
                return attribute.Description;
            }
        }
        
        return value.ToString();
    }

    public static T? GetEnumFromDescription<T>(string description) where T : Enum
    {
        foreach (var field in typeof(T).GetFields())
        {
            if (Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) is DescriptionAttribute attribute)
            {
                if (attribute.Description == description)
                    return (T?)field.GetValue(null);
            }
            else
            {
                if (field.Name == description)
                    return (T?)field.GetValue(null);
            }
        }
        
        return default;
    }
}

public static class PathHelper
{
    public static string SanitizeFileName(string fileName)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = new string(fileName.Select(c => invalidChars.Contains(c) ? '_' : c).ToArray());
        return sanitized;
    }

    public static string GetUniqueFilePath(string filePath)
    {
        if (!File.Exists(filePath))
            return filePath;

        var directory = Path.GetDirectoryName(filePath) ?? "";
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
        var extension = Path.GetExtension(filePath);
        var counter = 1;

        string newPath;
        do
        {
            newPath = Path.Combine(directory, $"{fileNameWithoutExtension} ({counter}){extension}");
            counter++;
        }
        while (File.Exists(newPath));

        return newPath;
    }

    public static long GetDirectorySize(string directoryPath)
    {
        if (!Directory.Exists(directoryPath))
            return 0;

        var directoryInfo = new DirectoryInfo(directoryPath);
        return directoryInfo.EnumerateFiles("*", SearchOption.AllDirectories).Sum(file => file.Length);
    }
}

public static class UrlHelper
{
    public static bool IsValidUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
            && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }

    public static bool IsYouTubeUrl(string url)
    {
        return url.Contains("youtube.com") || url.Contains("youtu.be");
    }

    public static bool IsPlaylistUrl(string url)
    {
        return url.Contains("list=") || url.Contains("/playlist");
    }
}
