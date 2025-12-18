using System.Diagnostics;
using ytdlp.Services.Interfaces;
namespace ytdlp.Services.Interfaces
{
    // IProcess.cs
    public interface IProcess : IDisposable
    {
        ProcessStartInfo StartInfo { get; set; }
        TextReader StandardOutput { get; }
        TextReader StandardError { get; }
        bool Start();
        Task WaitForExitAsync(CancellationToken cancellationToken = default);
    }

    // IProcessFactory.cs
    public interface IProcessFactory
    {
        IProcess CreateProcess();
    }
}

namespace ytdlp.Services
{
    // ProcessWrapper.cs - Wrapper um System.Diagnostics.Process
    public class ProcessWrapper : IProcess
    {
        private readonly Process _process;

        public ProcessWrapper()
        {
            _process = new Process();
        }

        public ProcessStartInfo StartInfo
        {
            get => _process.StartInfo;
            set => _process.StartInfo = value;
        }

        public TextReader StandardOutput => _process.StandardOutput;
        public TextReader StandardError => _process.StandardError;

        public bool Start() => _process.Start();

        public Task WaitForExitAsync(CancellationToken cancellationToken = default)
            => _process.WaitForExitAsync(cancellationToken);

        public void Dispose()
        {
            _process?.Dispose();
            GC.SuppressFinalize(this);
        }
    }

    // ProcessFactory.cs - Factory erstellt ProcessWrapper
    public class ProcessFactory : IProcessFactory
    {
        public IProcess CreateProcess() => new ProcessWrapper();
    }
}
