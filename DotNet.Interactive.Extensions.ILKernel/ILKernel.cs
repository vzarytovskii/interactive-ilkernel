using System.Runtime.Loader;
using System.Text.RegularExpressions;
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

            AssemblyLoadContext assemblyContext = new(null);
            var assembly = assemblyContext.LoadFromStream(assmeblyStream);
            var entryPoint = assembly.EntryPoint;

            if (entryPoint is null)
                throw new ApplicationException("Cannot find a valid entry point.");

            var @params = entryPoint.GetParameters() switch
            {
                { Length: 0 } => null,
                { Length: 1 } pi when pi.First().ParameterType == typeof(string[]) => new object[] { Array.Empty<string>() },
                _ => throw new ApplicationException("Could not find a suitable entry point, should be either one of the following: void Main() or void Main(string[] args)")
            };

            var result = entryPoint?.Invoke(default, @params);

            context.DisplayStandardOut(result?.ToString());
        }
        else
        {
            // TODO: Get logs and diagnostics.
            context.DisplayStandardError("Uh-oh. Something went wrong!");
        }
        return Task.CompletedTask;
    }
}
