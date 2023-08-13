#if !DISABLE_SINGLETON_AUTO_PUBLIC_STATIC_ACCESSOR

using System;

namespace Hibzz.Singletons
{    
    public class StaticAccessAttribute : Attribute 
    {
        public string VariableName { get; protected set; } = null;

        /// <summary>
        /// Exposes contents inside a partial singleton class through a public 
        /// static interface using the instance property
        /// </summary>
        /// <remarks>
        /// Without a variable name passed to the attribute, the system will 
        /// attempt to autogenerate the variable name. Currently the naming 
        /// schemes available are as follows:
        /// <list type="number">
        /// <item>
        /// Must start with an underscore: For example, <c>_health</c> will have a new public static accessor <c>Health</c>
        /// </item>
        /// <item>
        /// Must contain an underscore: For example, <c>p_mana</c> will have a new public static accessor <c>Mana</c>
        /// </item>
        /// </list>
        /// 
        /// <i>If things aren't working as expected, please file a bug report in GitHub Issues</i>
        /// </remarks>
        public StaticAccessAttribute() { }

        /// <summary>
        /// Exposes contents inside a partial singleton class through a public static accessor
        /// </summary>
        /// <param name="variableName">
        /// The variable name to set for the new public accessor.
        /// <para><i>Make sure that the variable name is unique</i></para>
        /// </param>
        public StaticAccessAttribute(string variableName) 
        {
            VariableName = variableName;
        }
    }
}

#endif
