## Desiging a plugin

Prise2 was built with backwards compatability in mind, in order to guarantee this, please respect the following guidelines in order to ensure your plugin can be used correctly:

1. Only use simple DTO (Data Transfer Objects) between the Host and the Plugin, in other words, keep the Contract simple
Avoid using:
- Streams (System.IO.Stream)
- Generics
- Expressions (System.Linq.Expressions)

2. Do not change an existing Contract method.
Let's assume the following Contract:
```
// This is a DTO
public class MyDTO
{
    public int Number { get; set; }
    public string Text { get; set; }
}

// This is the Contract
public interface IPlugin
{
    Task<MyDTO> Create(MyDTO dto);
}
```
Prise2 supports evolving Contracts, meaning that the Host can load a plugin that was originally created using the Contract as described above.
Let's say, the Contract and DTO need to expand to provide more data to newer plugins:
```
// This is a DTO
public class MyDTO
{
    public int Number { get; set; }
    public string Text { get; set; }
    public string Description { get; set; } // This was added
}

// This is the Contract
public interface IPlugin
{
    Task<MyDTO> Create(MyDTO dto);
    Task<MyDTO> Update(MyDTO dto);
}
```
Earlier plugins do not need to be re-compiled. Existing plugins will still be executable via the original Create method.
Since the DTO's are serialized, these plugins will receive a default value (mostly null) for the Description property.
This way, they can still be invoked.
Any newer plugin that supports this new Contract, will be able to benefit from the newly added Description property.

Invoking the Update method on an older Plugin, will result in a Prise.Proxy.PriseProxyException being thrown, which you can try{}catch(Prise.Proxy.PriseProxyException pex){}. This allows you to gracefully alter the execution path and show a meaningful message to the users.

However, if you change the original Create method signature, the Prise.Proxy will not be able to find the correct method to invoke and thus thrown a Prise.Proxy.PriseProxyException.

Tip: Move the Contract method parameters into an Options object and provide that as the sole parameter.
==> This way you can evolve the DTO going forward and support your older plugins:
public enum ActionType
{
    Create,
    Update,
    Delete
}

// This is a DTO
public class PluginObject
{
    public int Number { get; set; }
    public string Text { get; set; }
    public ActionType { get;set; }
    public string Description { get; set; } // This was added
}

// This is the Contract
public interface IPlugin
{
    // We now support 3 types of Plugins
    // Those who were originally created and implement only the Create method
    Task<MyDTO> Create(MyDTO dto);
    // Those who were created after and implement the Create and Update method
    Task<MyDTO> Update(MyDTO dto);
    // Any newer plugins that implement all three methods and chain the execution to the CRUD method, supporting all three scenarios
    Task<PluginObject> CRUD(MyDTO dto);
}
```

3. Use field injection + PluginServiceAttribute + PrisePluginBridge + PluginActivated method to inject and bootstrap your Plugin
Prise2 relies heavily on Plugin field injection
TODO