using System.IO;
using System.Runtime.Loader;
using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.Commands;
using Mobius.ILasm.Core;
using Mobius.ILasm.interfaces;

namespace DotNet.Interactive.Extensions.ILKernel;

public sealed class ILKernel : Kernel, IKernelCommandHandler<SubmitCode>
{
    private static readonly ILogger _logger = new DummyILLogger();
    public ILKernel() : base("il")
    {
    }

    public Task HandleAsync(SubmitCode command, KernelInvocationContext context)
    {
        var cil = new[] { command.Code };
        Driver driver = new(logger: _logger, target: Driver.Target.Exe, showParser: false, debuggingInfo: false, showTokens: false);

        using MemoryStream assmeblyStream = new();

        var succeeded = driver.Assemble(cil, assmeblyStream);

        if (succeeded)
        {
            assmeblyStream.Seek(0, SeekOrigin.Begin);

            var assemblyContext = new AssemblyLoadContext(null);
            var assembly = assemblyContext.LoadFromStream(assmeblyStream);
            var entryPoint = assembly.EntryPoint;
            var result = entryPoint?.Invoke(null, new object[] { Array.Empty<string>() });

            context.DisplayStandardOut(result?.ToString() ?? "");
        }
        else
        {
            // TODO: Get logs and diagnostics.
            context.DisplayStandardError("Uh-oh. Something went wrong!");
        }
        return Task.CompletedTask;
    }
}
