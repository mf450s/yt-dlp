using System;

namespace ytdlp.Services;

public interface IConfigsServices
{
    string GetWholeConfigPath(string configName);
    List<string> GetAllConfigNames();
}
