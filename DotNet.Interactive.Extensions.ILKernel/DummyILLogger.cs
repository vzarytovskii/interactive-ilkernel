using Mobius.ILasm.interfaces;
using Mono.ILASM;

namespace DotNet.Interactive.Extensions.ILKernel;

internal class DummyILLogger : ILogger
{
    public void Error(string message)
    {
    }

    public void Error(Location location, string message)
    {
    }

    public void Info(string message)
    {
    }

    public void Warning(string message)
    {
    }

    public void Warning(Location location, string message)
    {
    }
}
