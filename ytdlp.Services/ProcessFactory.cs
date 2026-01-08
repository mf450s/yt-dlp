using ytdlp.Services.Interfaces;

namespace ytdlp.Services
{
    public class ProcessFactory : IProcessFactory
    {
        public IProcess CreateProcess() => new ProcessWrapper();
    }
}
