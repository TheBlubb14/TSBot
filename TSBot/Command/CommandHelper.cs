using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace TSBot.Command
{
    public static class CommandHelper
    {
        public static IEnumerable<T> GetCommands<T>(Assembly assembly = null) where T : ICommand
        {
            return (assembly ?? Assembly.GetExecutingAssembly())
                .GetTypes()
                .Where(x => x.GetInterface(typeof(T).FullName) != null)
                .Select(x => Activator.CreateInstance(x))
                .Cast<T>();
        }
    }
}
