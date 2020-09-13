# xliff-compiler

When installed via NuGet, this will locate any .xlf files and generate the corresponding resx files before the main build process completes.

NuGet: https://www.nuget.org/packages/XliffCompiler/

## Creating NuGet

1. Run Release build of main project
1. Ensure nuget.exe is downloaded
1. Run something like `"C:\Users\andre\Downloads\nuget.exe" pack XliffCompiler.nuspec`