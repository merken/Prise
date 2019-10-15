## Creating a simple plugin

Creating a plugin using Prise is easy, it starts off with a contract, also known as an Interface :
```
public interface ICalculationPlugin
{
    int Calculate(int a, int b);
}
```
Put this interface in its own lightweight assembly so that it can be easily shared. This is now called 'the contract'.

Next, start creating some plugins that implement this interface, put them in their own projects, this will generate an assembly per plugin.
