using System.Runtime.Loader;
using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.Commands;
using Microsoft.DotNet.Interactive.Events;
using Microsoft.DotNet.Interactive.Formatting;
using Mobius.ILasm.Core;

// namespace ILKernel.InteractiveExtension;

namespace DotNet.Interactive.Extensions.ILKernel;

#nullable disable

public sealed class ILKernel :
    Kernel,
    ISupportNuget,
    IKernelCommandHandler<SubmitCode>
{
    private readonly Lazy<PackageRestoreContext> _packageRestoreContext = default;

    private readonly List<string> _references = new();

    public PackageRestoreContext PackageRestoreContext => _packageRestoreContext.Value;

    public IEnumerable<string> RestoreSources => PackageRestoreContext.RestoreSources;

    public IEnumerable<PackageReference> RequestedPackageReferences => PackageRestoreContext.RequestedPackageReferences;

    public IEnumerable<ResolvedPackageReference> ResolvedPackageReferences => PackageRestoreContext.ResolvedPackageReferences;
    
    public ILKernel() : base("il") =>
        _packageRestoreContext = new(() =>
        {
            using PackageRestoreContext packageRestoreContext = new();
            RegisterForDisposal(packageRestoreContext);

            return packageRestoreContext;
        });

    public Task HandleAsync(SubmitCode command, KernelInvocationContext context)
    {
        using var logger = new PersistentILLogger();
        var cil = new[] { command.Code };
        Driver driver = new(logger: logger, target: Driver.Target.Exe, showParser: false, debuggingInfo: false, showTokens: false);

        using MemoryStream assemblyStream = new();

        try
        {
            var succeeded = driver.Assemble(cil, assemblyStream);
            if (succeeded)
            {
                assemblyStream.Seek(0, SeekOrigin.Begin);

                AssemblyLoadContext assemblyContext = new(null);

                _references.ForEach(r => assemblyContext.LoadFromAssemblyPath(r));

                var assembly = assemblyContext.LoadFromStream(assemblyStream);
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

                if (result is not null)
                    context.DisplayStandardOut(result?.ToString());
            }
            else
            {
                // TODO: Get logs and diagnostics.
                context.DisplayStandardError("Uh-oh. Something went wrong!");
            }
        }
        catch (Exception ex)
        {
            var diagnostics = logger.GetDiagnostics().ToList();
            var formattedDiagnostics = diagnostics.Select(d => new FormattedValue(PlainTextFormatter.MimeType, d.ToString())).ToArray();
            context.Publish(new DiagnosticsProduced(diagnostics, command, formattedDiagnostics));
            context.Fail(command, ex);
        }

        return Task.CompletedTask;
    }

    public Task<PackageRestoreResult> RestoreAsync() => _packageRestoreContext.Value.RestoreAsync();

    public PackageReference GetOrAddPackageReference(string packageName, string packageVersion = null) =>
        _packageRestoreContext.Value.GetOrAddPackageReference(packageName, packageVersion);

    public void TryAddRestoreSource(string source) => _packageRestoreContext.Value.TryAddRestoreSource(source);

    public void RegisterResolvedPackageReferences(IReadOnlyList<ResolvedPackageReference> packageReferences) =>
        _references.AddRange(packageReferences.SelectMany(r => r.AssemblyPaths));
}
