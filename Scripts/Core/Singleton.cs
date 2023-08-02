using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Hibzz.Singletons
{
    public class Singleton<T> : MonoBehaviour where T : Component
    {
        private static T instance;

        /// <summary>
        /// Gives a reference to the singleton instance. If none is available, 
        /// creates a new one and returns it
        /// </summary>
        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = RequestNewInstance();
                }

                return instance;
            }
        }

        /// <summary>
        /// Gives any available singleton instance. 
        /// If none was created or if one was destroyed, returns null.
        /// </summary>
        public static T AvailableInstance => instance;

        // Used to create a new instance of the singleton
        private static T RequestNewInstance()
        {
            T[] items = GetAllObjects();
            if (items.Length == 0)
            {
                // Using the reflection system, look if CreateNewInstance is being overriden
                Type type = typeof(T);
                MethodInfo method = type.GetMethod("CreateNewInstance", BindingFlags.NonPublic | BindingFlags.Static);

                // If the reflection system can find an overriden version of the static function CreateNewInstance, then invoke it
                if(method is not null) 
                {
                    return (T) method.Invoke(null, null);
                }

                // No overrides found, call base implementation
                return CreateNewInstance();
            }
            else if (items.Length > 1)
            {
                // more than one instance found. So returning null
                Debug.LogError("Multiple instances of the singleton found. So, cant determine what's the singleton. Returning null.");
                return null;
            }

            // only one instance of type T found in the scene
            return items[0];
        }

        // Overridable function used to create custom instance creation if needed
        protected static T CreateNewInstance()
        {
            GameObject obj = new GameObject();
            obj.name = typeof(T).Name + "Object";
            return obj.AddComponent<T>();
        }

        // Summary: Get all components of type T in the scene
        // Remarks: This is an editor friendly version of FindObjectsOfType
        //          that works on singletons that exist on an editor context.
        //          The function is more expensive in editor context, however
        //          remains cheap in regular playmode.
        protected static T[] GetAllObjects()
        {
            #if UNITY_EDITOR

            // when the editor is in play mode, FindObjectsOfType will work... 
            // however, if the nothing was found, we can do an extensive search
            // with the objects that might be marked with "Don't Save" flags
            if (Application.isPlaying)
            {
                var result = FindObjectsOfType<T>();
                if(result.Length > 0) 
                { 
                    return result; 
                }
            }

            // the editor is not in playmode, so in the editor context
            // this container will store all objects found of type T in the scene
            List<T> singletons = new List<T>();

            // resources api needs to be used, else it won't detect objects of
            // that have a don't save flag
            GameObject[] gameobjects = Resources.FindObjectsOfTypeAll<GameObject>();
            foreach (var go in gameobjects)
            {
                // a persistent gameobject is something stored in the disk, so
                // usually a prefab... ignore those type of objects
                if(UnityEditor.EditorUtility.IsPersistent(go)) { continue; }

                // get all singleton objects of type T and store it in the list of singletons
                var comps = go.GetComponentsInChildren<T>();
                foreach (var comp in comps)
                {
                    singletons.Add(comp);
                }
            }

            // convert the list to an array, as that's the return type expected
            return singletons.ToArray();

            #else
            
            // only simple gameobject find in scene... To be honest, I don't 
            // see a point in instantiating objects with the Don't save flags 
            // in the build version of a game, so this will do fine
            return FindObjectsOfType<T>();
            
            #endif
        }

        // when the singleton object gets destroyed, we make sure that the
        // static singleton reference is cleared
        protected virtual void OnDestroy()
        {
            // making sure that this object is the static instance, before just 
            if (instance == this)
            {
                instance = null;
            }
        }
    }
}
