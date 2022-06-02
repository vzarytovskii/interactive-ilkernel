using FluentAssertions;
using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.Events;
using Microsoft.DotNet.Interactive.Formatting;
using Xunit;

namespace DotNet.Interactive.Extensions.ILKernel.Tests;

public sealed class CompilerTests
{
    [Fact]
    public async Task ILKernel_Should_Run_Well_Formed_CIL_Code_With_No_Main_Parameters_And_Produce_Valid_Output()
    {
        using CompositeKernel kernel = new() { new ILKernel() };

        var result = await kernel.SubmitCodeAsync(@"
.assembly ConsoleApp
{
}

.class public auto ansi abstract sealed beforefieldinit Program
    extends System.Object
{
    .method public hidebysig static
        void Main () cil managed
    {
        .entrypoint
        .maxstack 8

        ldstr ""HI""
        call void[System.Console]System.Console::Write(string)
        ret
    }
}
");

        var events = result.KernelEvents.ToSubscribedList();

        var data = events
            .OfType<StandardOutputValueProduced>()
            .First()
            .FormattedValues
            .Single(fm => fm.MimeType == PlainTextFormatter.MimeType)
            .Value;
        data.Should().Be("HI");
    }

    [Fact]
    public async Task ILKernel_Should_Run_Well_Formed_CIL_Code_With_Standard_Main_Parameters_And_Produce_Valid_Output()
    {
        using CompositeKernel kernel = new() { new ILKernel() };

        var result = await kernel.SubmitCodeAsync(@"
.assembly ConsoleApp
{
}

.class public auto ansi abstract sealed beforefieldinit Program
    extends System.Object
{
    .method public hidebysig static
        void Main (string[] args) cil managed
    {
        .entrypoint
        .maxstack 8

        ldstr ""HI""
        call void[System.Console]System.Console::Write(string)
        ret
    }
}
");

        var events = result.KernelEvents.ToSubscribedList();

        var data = events
            .OfType<StandardOutputValueProduced>()
            .First()
            .FormattedValues
            .Single(fm => fm.MimeType == PlainTextFormatter.MimeType)
            .Value;
        data.Should().Be("HI");
    }

    [Fact]
    public async Task ILKernel_Should_Fail_On_Malformed_CIL_Code()
    {
        using CompositeKernel kernel = new() { new ILKernel() };

        var result = await kernel.SubmitCodeAsync(@"
.assembly ConsoleApp
{
}

.class public auto ansi abstract sealed beforefieldinit Program
    extends System.Object
{
    .method public hidebysig static
        void Main (string[] args) cil managed
    {
        .entrypoint
        .maxstack ""8""

        ldstr ""HI""
        call void[System.Console]System.Console::Write(string)
        ret
    }
}
");

        var events = result.KernelEvents.ToSubscribedList();
        var diagnostic =
            events.OfType<DiagnosticsProduced>().Single()
                  .Diagnostics.Single();

        diagnostic.Message.Should().Be("irrecoverable syntax error");
        diagnostic.Code.Should().Be("0");
        diagnostic.LinePositionSpan.Start.Line.Should().Be(12);
        diagnostic.LinePositionSpan.Start.Character.Should().Be(21);

        var commandFailed =
            events.OfType<CommandFailed>().Single();
        commandFailed.Exception.Message.Should().Be("irrecoverable syntax error");

    }
}
