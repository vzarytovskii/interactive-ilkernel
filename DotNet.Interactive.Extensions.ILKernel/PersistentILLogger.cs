using Microsoft.CodeAnalysis;
using Microsoft.DotNet.Interactive;
using Mobius.ILasm.interfaces;

using Diagnostic = Microsoft.DotNet.Interactive.Diagnostic;
using Location = Mono.ILASM.Location;

namespace DotNet.Interactive.Extensions.ILKernel;

internal class PersistentILLogger : ILogger, IDisposable
{
    private enum Severity { Warning, Error };

    private readonly List<(Severity, Location, string)> _storage = new();

    internal IEnumerable<Diagnostic> GetDiagnostics() => _storage.Select(
        d =>
        {
            var (severity, location, message) = d;
            // Position is adjusted for the fact that the ILASM source is 1-based,
            LinePosition linePos = new(location.line - 1, location.column - 1);
            LinePositionSpan span = new(linePos, linePos);

            var diagnosticSeverity = severity is Severity.Warning
                                   ? DiagnosticSeverity.Warning
                                   : DiagnosticSeverity.Error;

            return new Diagnostic(span, diagnosticSeverity, "0", message);
        });

    public void Dispose() => _storage.Clear();

    public void Error(string message)
    {
    }

    public void Error(Location location, string message) => _storage.Add((Severity.Error, location, message));

    public void Info(string message)
    {
    }

    public void Warning(string message)
    {
    }

    public void Warning(Location location, string message) => _storage.Add((Severity.Warning, location, message));
}
