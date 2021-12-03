using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.CSharp;
using Microsoft.DotNet.Interactive.Events;
using Microsoft.DotNet.Interactive.Formatting;
using Xunit;

namespace DotNet.Interactive.Extensions.ILKernel.Tests;

public class CompilerTests
{
    [Fact]
    public async Task ILkernel_Should_Run_Well_Formed_CIL_Code_And_Produce_Valid_Output()
    {
        using CompositeKernel kernel = new() { new CSharpKernel().UseNugetDirective() };

        var extension = new ILKernelExtension();

        await extension.OnLoadAsync(kernel);

        var result = await kernel.SubmitCodeAsync(@"
#!il
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
        call void[System.Console]System.Console::WriteLine(string)
        ret
    }
}
");

        var events = result.KernelEvents.ToSubscribedList();

        var formattedData = events
            .OfType<DisplayedValueProduced>()
            .Single()
            .FormattedValues
            .Single(fm => fm.MimeType == HtmlFormatter.MimeType)
            .Value;

    }
}
