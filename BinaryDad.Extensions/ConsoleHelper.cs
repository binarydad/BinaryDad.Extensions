using System;
using System.Reflection;

namespace BinaryDad.Extensions
{
    /// <summary>
    /// A set of utilities to assist with Console applications
    /// </summary>
    public static class ConsoleHelper
    {
        /// <summary>
        /// Parses command argument flags in the format "-flag1 value -flag2 value2"
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="args">A raw string of arguments</param>
        /// <returns></returns>
        public static T ParseCommandFlags<T>(string args) where T : new()
        {
            return ParseCommandFlags<T>(args.Split(' '));
        }

        /// <summary>
        /// Parses command argument flags in the format "command.exe -flag1 value -flag2 value2"
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="args">Collection of arguments, typically from Program.Main(string[] args)</param>
        /// <returns></returns>
        public static T ParseCommandFlags<T>(string[] args) where T : new()
        {
            // the new parameter instance
            var parameters = new T();

            parameters
                .GetType()
                .GetProperties()
                .EmptyIfNull()
                .ForEach(p =>
                {
                    var commandFlagAttribute = p.GetCustomAttribute<CommandFlagAttribute>(true);

                    if (commandFlagAttribute != null)
                    {
                        var valueFlagIndex = args.IndexOf($"-{commandFlagAttribute.Flag}", StringComparer.OrdinalIgnoreCase);
                        var valueIndex = valueFlagIndex + 1;

                        // find the argument value in the list, convert to the desired type, and set the value
                        if (valueFlagIndex >= 0 && args.Length > valueIndex)
                        {
                            p.SetValue(parameters, args[valueIndex].To(p.PropertyType));
                        }
                    }
                });

            return parameters;
        }
    }
}
