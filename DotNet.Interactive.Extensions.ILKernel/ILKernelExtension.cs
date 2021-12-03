using Microsoft.AspNetCore.Html;
using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.Commands;
using Microsoft.DotNet.Interactive.Formatting;

namespace DotNet.Interactive.Extensions.ILKernel;

public sealed class ILKernelExtension : IKernelExtension, IStaticContentSource
{
    public string Name => "IL";
    public async Task OnLoadAsync(Kernel kernel)
    {
        if (kernel is CompositeKernel compositeKernel)
            compositeKernel.Add(new ILKernel());

        var message = new HtmlString("<details><summary>Compile and run custom <abbr title=\"Common Intermediate Language\">CIL</abbr><sup><a href=\"https://en.wikipedia.org/wiki/Common_Intermediate_Language\" title=\"Common Intermediate Language\" target=\"_blank\">[?]</a></sup> code.</summary><p> This extension adds a new kernel that can compile and run MSIL / CIL code(using <a href=\"https://github.com/kkokosa/Mobius.ILasm\">Mobius.ILasm</a> library).</details>");


        var formattedValue = new FormattedValue(
            HtmlFormatter.MimeType,
            message.ToDisplayString(HtmlFormatter.MimeType));

        await kernel.SendAsync(new DisplayValue(formattedValue, Guid.NewGuid().ToString()));
    }
}
