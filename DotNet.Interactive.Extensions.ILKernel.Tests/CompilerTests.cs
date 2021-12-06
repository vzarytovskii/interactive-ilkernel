using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.CSharp;
using Microsoft.DotNet.Interactive.Events;
using Microsoft.DotNet.Interactive.Formatting;
using Xunit;

namespace DotNet.Interactive.Extensions.ILKernel.Tests;

[SuppressMessage("ApiDesign", "RS0016", MessageId = "Add public types and members to the declared API")]
[SuppressMessage("ApiDesign", "RS0037", MessageId = "Enable tracking of nullability of reference types in the declared API")]
public sealed class CompilerTests
{
    [Fact]
    public async Task ILKernel_Should_Run_Well_Formed_CIL_Code_With_No_Main_Parameters_And_Produce_Valid_Output()
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
}
