using ytdlp.Services.Interfaces;
using System.IO.Abstractions;
using FluentResults;
namespace ytdlp.Services;

public class ConfigsServices(IFileSystem fileSystem) : IConfigsServices
{
    private readonly string configFolder = "../configs/";
    private readonly IFileSystem _fileSystem = fileSystem;
    public string GetWholeConfigPath(string configName)
    {
        return $"{configFolder}{configName}.conf";
    }
    public List<string> GetAllConfigNames()
    {
        var files = _fileSystem.Directory.GetFiles(configFolder, "*.conf");
        var configNames = new List<string>();

        foreach (var file in files)
        {
            string fileName = _fileSystem.Path.GetFileName(file);
            string nameWithoutExtension = _fileSystem.Path.GetFileNameWithoutExtension(fileName);
            configNames.Add(nameWithoutExtension);
        }

        return configNames;
    }
    public Result<string> GetConfigContentByName(string name)
    {
        string path = GetWholeConfigPath(name);
        if (_fileSystem.File.Exists(path))
        {
            using var reader = _fileSystem.File.OpenText(path);
            return Result.Ok(reader.ReadToEnd());
        }
        else
        {
            return Result.Fail($"Config file not found: {path}");
        }
    }
    public Result<string> DeleteConfigByName(string name)
    {
        string path = GetWholeConfigPath(name);
        if (_fileSystem.File.Exists(path))
        {
            _fileSystem.File.Delete(path);
            return Result.Ok();
        }
        else return Result.Fail("File already exists");
    }
    public async Task<Result<string>> CreateNewConfigAsync(string name, string configContent)
    {
        string newPath = GetWholeConfigPath(name);
        if (_fileSystem.File.Exists(newPath))
        {
            return Result.Fail($"File with name '{name}' already exists");
        }
        else
        {
            await WriteContentToFile(newPath, configContent);
            return Result.Ok($"Config file '{name}' created successfully.");
        }
    }

    public async Task<Result<string>> SetConfigContentAsync(string name, string configContent)
    {
        string path = GetWholeConfigPath(name);
        if (_fileSystem.File.Exists(path))
        {
            await WriteContentToFile(path, configContent);
            return Result.Ok();
        }
        else
            return Result.Fail($"File with name '{name}' doesnt exists");
        
    }

    internal async Task WriteContentToFile(string path, string configContent )
    {
        await using var writer = _fileSystem.File.CreateText(path);
        await writer.WriteAsync(configContent);
    }
}
