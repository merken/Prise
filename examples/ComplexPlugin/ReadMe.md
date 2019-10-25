## Complex example

This directory contains the most complex example of loading a plugin.
The AppHost is an ASP.NET Core 3 application that exposes various endpoints.
Every call to the API needs to have a **PluginType** HTTP Header with one of the following values: PluginA, PluginB or PluginC.
Each plugin has its own special characteristics. 

PluginA is the most simple one

PluginB has broken the contract.

PluginC is dependend on a third party assembly.

### TOO LONG DID NOT READ
Launch the AppHost using the `dotnet run --urls=https://localhost:5001` command from inside the **AppHost** directory.

Publish all the components using the `cake` command from inside the **ComplexPlugin** directory.

Try out the various endpoints using Postman!

... Or just run the tests via the `dotnet test` command from inside the **Tests** directory.