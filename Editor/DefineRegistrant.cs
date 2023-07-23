// This script should only run when define manager is installed
#if ENABLE_DEFINE_MANAGER

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hibzz.DefineManager;

namespace Hibzz.Singletons.Editor
{
    /// <summary>
    /// Used to register the defines
    /// </summary>
    internal class DefineRegistrant
    {
        [RegisterDefine]
        static DefineRegistrationData RegisterDisableScriptableObjectCreator()
        {
            DefineRegistrationData data = new DefineRegistrationData();

            data.Define = "DISABLE_SCRIPTABLE_SINGLETON_CREATOR";
            data.DisplayName = "Disable Scriptable Singleton Creator";
            data.Category = "Hibzz.Singletons";
            data.Description = "The scriptable singleton creator functionality " +
                "lets users mark any ScriptableSingleton class with an attribute " +
                "called `CreateScriptableSingletonAsset`. When the user presses " +
                "the \"Create Scriptable Singleton Assets\" button, the system " +
                "will create any missing assets for those singletons in the " +
                "\"Resources/Singletons\" folder. \n\n" + 
                "Installing this define will disable this feature.";
            data.EnableByDefault = false;

            return data;
        }
    }
}

#endif
