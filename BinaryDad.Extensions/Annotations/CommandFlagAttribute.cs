using System;

namespace BinaryDad.Extensions
{
    /// <summary>
    /// Binds data from command line flags to attached property. In the example, [<see cref="CommandFlagAttribute"/>("context")] public string Context { get; set; }, a command of "command.exe -context Production" will set Context property equal to "Production".
    /// </summary>
    public class CommandFlagAttribute : Attribute
    {
        public string Flag { get; }

        /// <summary>
        /// The name of the flag, without the preceding dash (-)
        /// </summary>
        /// <param name="flag"></param>
        public CommandFlagAttribute(string flag)
        {
            Flag = flag;
        }
    }
}
