using System;

// as long as the scriptable singleton asset creator isn't disabled
#if !DISABLE_SCRIPTABLE_SINGLETON_CREATOR

namespace Hibzz.Singletons
{
    public class CreateScriptableSingletonAssetAttribute : Attribute { }
}

#endif
