## A simple plugin

This project contains a simple plugin `HelloWorldPlugin`. The host is an ASP.NET Core web api that loads the plugin from the Plugins directory on disk when the HelloController is activated.

First, build the plugin using Cake. run the `cake` command from the SimplePlugin directory.

Launch the MyHost application using the `dotnet run` command from inside the MyHost directory.

Open your browser to https://localhost:5001/hello?input=Maarten