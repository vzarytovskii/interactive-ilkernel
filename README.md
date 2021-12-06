# IL Kernel for .NET Interactive

## Description

An experimental preview verision of [MSIL/CIL](https://en.wikipedia.org/wiki/Common_Intermediate_Language) kernel for the [.NET Interactive](https://github.com/dotnet/interactive), using [@kkokosa](https://github.com/kkokosa)'s [Mobius.ILasm](https://github.com/kkokosa/Mobius.ILasm) as an IL compiler.

## Usage

First, new kernel should be loaded into the interactive session:

```csharp
// Optional, uncomment the following line, if you need to include additional paths to reference libraries from.
// #i "C:\Users\user\Downloads"
#r "nuget:DotNet.Interactive.Extensions.ILKernel,0.1.0-pre"
using Microsoft.DotNet.Interactive;
using DotNet.Interactive.Extensions.ILKernel;
var kernel = new ILKernel();
((CompositeKernel)Kernel.Root).Add(kernel);
```

You should now be able to run IL, for example:

```csharp
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

        ldstr "HI"
        call void[System.Console]System.Console::Write(string)
        ret
    }
}
```
