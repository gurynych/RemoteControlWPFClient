using System;
using System.IO;

namespace RemoteControlWPFClient.BusinessLayer.Helpers;

public static class FileSaver
{
    public static string CreateUniqDownloadPath(string fileName, string downloadDirectory)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(fileName));
        if (string.IsNullOrWhiteSpace(downloadDirectory))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(downloadDirectory));
        if (!Directory.Exists(downloadDirectory))
            throw new DirectoryNotFoundException();

        string fullPath = downloadDirectory;
        int lastDotIndex = fileName.LastIndexOf('.');
        string extension = "";
        string fileNameWithoutExtension = fileName;
        if (lastDotIndex != -1)
        {
            extension = fileName[lastDotIndex..];
            fileNameWithoutExtension = fileName[..lastDotIndex];
        }
        
        string fileNameWithoutExtensionConst = fileNameWithoutExtension;
        
        int i = 1;
        while (File.Exists(Path.Combine(fullPath, fileNameWithoutExtension + extension)))
        {
            fileNameWithoutExtension = fileNameWithoutExtensionConst + $"({i++})";
        }

        return Path.Combine(fullPath, fileNameWithoutExtension + extension);
    }
}