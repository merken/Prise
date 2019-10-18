﻿using System;
using Contract;
using Prise.Infrastructure;

namespace Hallo.Plugin
{
    [Plugin(PluginType = typeof(IHelloPlugin))]
    public class Hallo : IHelloPlugin
    {
        public string SayHello(string input)
        {
            return $"Hallo {input}";
        }
    }
}
