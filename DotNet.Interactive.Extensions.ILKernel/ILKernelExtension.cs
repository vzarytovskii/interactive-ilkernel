using Microsoft.AspNetCore.Html;
using Microsoft.DotNet.Interactive;

namespace DotNet.Interactive.Extensions.ILKernel;

#nullable disable

public sealed class ILKernelExtension : IKernelExtension
{
    public Task OnLoadAsync(Kernel kernel)
    {
        if (kernel is not CompositeKernel compositeKernel) return Task.CompletedTask;
        
        compositeKernel.Add(new ILKernel());

        KernelInvocationContext.Current?.Display(
            new HtmlString(@"<details><summary>Write, compile and run IL code.</summary>
        <p>This extension adds support for <a href=""https://en.wikipedia.org/wiki/Common_Intermediate_Language"">MSIL/CIL</a>. Try this code:</p>
<pre>
    <code>
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

        ldstr ""Hello, world from IL!""
        call void[System.Console]System.Console::Write(string)
        ret
    }
}</code>
</pre>
        </details>"),
            "text/html");

        return Task.CompletedTask;
    }
}
