using UnityEngine;
using UnityEditor;
using System.IO;
using System;

namespace Hibzz.Singletons.Editor
{
    public static class SingletonsEditor
    {
        // These are menu paths
        const string BASE_PATH = "Hibzz/Singletons/";
        const string CREATE_SINGLETON_ASSET_PATH = BASE_PATH + "Create Scriptable Singleton Assets";

        // these are debug logs made into consts
        #region Debug Logs

        const string UPTO_DATE_MESSAGE = "<Color=lightblue>Scriptable singletons up to date.</Color>";
        const string NO_ATTRIBUTE_FOUND_MESSAGE = UPTO_DATE_MESSAGE + "\n" +
            "  None of the classes use the auto create scriptable singleton attribute. \n" +
            "  Please use <i>CreateScriptableSingletonAsset</i> attribute to use this functionality. \n";

        #endregion

        /// <summary>
        /// Create ScriptableSingleton assets for all classes that has a 
        /// CreateScriptableSingletonAsset attribute
        /// </summary>
        [MenuItem(CREATE_SINGLETON_ASSET_PATH)]
        public static void CreateScriptableSingletonAssets()
        {
            // get the collection of classes with the auto create attribute
            var types = TypeCache.GetTypesWithAttribute<CreateScriptableSingletonAssetAttribute>();

            // none of the classes use `CreateScriptableSingletonAsset` Attribute
            // let the user know about that
            if (types.Count <= 0)
            {
                Debug.Log(NO_ATTRIBUTE_FOUND_MESSAGE);
                return;
            }

            // Create the Resources/Singletons directory, if it hasn't been created already
            Directory.CreateDirectory($"{Application.dataPath}/Resources/Singletons/");

            // loop through each of those classes and add a new scriptable object if needed
            int newAssetCount = 0;
            foreach (var type in types)
            {
                bool success = CreateScriptableSingletonAsset(type);
                if(success) { newAssetCount++; }
            }

            // if there were no new assets created, just log info things are up to date
            if(newAssetCount <= 0)
            {
                Debug.Log(UPTO_DATE_MESSAGE);
            }
        }

        /// <summary>
        /// Create a scriptable singleton object of the given type
        /// </summary>
        /// <param name="type">The type of the scriptable object to create</param>
        /// <returns>Was the creation process successful?</returns>
        /// <remarks>
        /// The function will perform type checks to see if the type is a 
        /// subclass of ScriptableObject. It will also skip the creation if a 
        /// ScriptableSingleton asset of the same type already exists in the 
        /// resources folder.
        /// </remarks>
        private static bool CreateScriptableSingletonAsset(Type type)
        {
            // if the type isn't a scriptable scriptable, then don't create a
            // singleton object
            if(!type.IsSubclassOf(typeof(ScriptableObject))) 
            {
                Debug.LogWarning($"Incorrect use of <i>CreateScriptableSingletonAssetAttribute</i> attribute. '{type.FullName}' is not a subclass of type <i>ScriptableObject</i>");
                return false; 
            }

            // if a an object already exists of the type singleton in the
            // resources/singleton folder, then don't create a new one
            var loaded_object = Resources.LoadAll("Singletons", type);
            if(loaded_object.Length > 0) { return false; }

            // create an instance of the object and an asset out of the instance
            var instance = ScriptableObject.CreateInstance(type);
            AssetDatabase.CreateAsset(instance, $"Assets/Resources/Singletons/{type.FullName}.asset");
            AssetDatabase.Refresh();

            // log it to the console and inform the user that the asset is being created
            Debug.Log($"Creating asset of type {type.FullName} in the resources folder");

            // return true indicating that the process was successful
            return true;
        }
    }
}



