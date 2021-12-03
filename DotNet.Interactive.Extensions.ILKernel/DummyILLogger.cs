using Mobius.ILasm.interfaces;
using Mono.ILASM;

namespace DotNet.Interactive.Extensions.ILKernel;

internal class DummyILLogger : ILogger
{
    public void Error(string message)
    {
        return;
    }

    public void Error(Location location, string message)
    {
        return;
    }

    public void Info(string message)
    {
        return;
    }

    public void Warning(string message)
    {
        return;
    }

    public void Warning(Location location, string message)
    {
        return;
    }
}
