using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TSBot.Command.TS;

namespace TSBot.Command
{
    public static class CommandHelper
    {
        // To have a "default" interface implementation for the Identifiers 
        private static string GetCommandIdentifier<T>() where T : ICommand
        {
            if (typeof(T) == typeof(ITSCommand))
            {
                return "!";
            }
            //else if(typeof(T) == typeof(ITelegramCommand))
            //{
            //    return "/";
            //}
            else
            {
                return null;
            }
        }

        public static string BuildCommand<T>(this T t) where T : ICommand
        {
            return GetCommandIdentifier<T>() + t.Command;
        }

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
