using ytdlp.Services.Interfaces;
using System.IO.Abstractions;

namespace ytdlp.Services;

public class ConfigsServices(IFileSystem fileSystem) : IConfigsServices
{
    private readonly IFileSystem _fileSystem = fileSystem;
    public string GetWholeConfigPath(string configName)
    {
        return $"../configs/{configName}.conf";
    }
    public List<string> GetAllConfigNames()
    {
        string path = "../configs/";
        var files = _fileSystem.Directory.GetFiles(path, "*.conf");
        var configNames = new List<string>();

        foreach (var file in files)
        {
            string fileName = _fileSystem.Path.GetFileName(file);
            string nameWithoutExtension = _fileSystem.Path.GetFileNameWithoutExtension(fileName);
            configNames.Add(nameWithoutExtension);
        }

        return configNames;
    }
}
