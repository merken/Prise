# Prise is a multi-targeted codebase

In order to support a variety of applications, Prise was built on .NET Core 2.1.
.NET Core 3.0 support was added in v1.2.3.

This is enabled by adding a <TargetFrameworks> tag in the *.csproj file and include <ItemGroup> specific to the target platform.
By including the <ItemGroup>, we can remove the other framework directories from compilation and include preprocessor symbols; NETCORE2_1 and NETCORE3_0.

These symbols are used later on, in the code, in order to split the codebase into framework specific modules.

You can build any project or example in this repository using the -f netcoreapp2.1 or -f netcoreapp3.0 flag, in order to specify the target framework.

`dotnet build -f netcoreapp2.1`
