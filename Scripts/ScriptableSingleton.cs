using UnityEngine;

namespace Hibzz.Singletons
{
	// NOTE: For ScriptableSingleton to work, an instance of the singleton must be added to the 
	//       "Resources/Singletons" folder... Furthermore, there must be exactly 1
	//       ScriptableSingleton of a given type, else, it'll pick the first one in the list
	public class ScriptableSingleton<T> : ScriptableObject where T : ScriptableSingleton<T>
	{
		private static T instance;
		public static T Instance
		{
			get
			{
				if(instance == null) { return RequestInstance(); }

				return instance;
			}
		}

		public static T RequestInstance()
		{
			var objects = Resources.LoadAll<T>("Singletons");
			if(objects.Length == 0)
			{
				Debug.LogError($"Cannot find object of type {typeof(T)} in the Resources/Singletons folder. Please add one.");
				return null;
			}
			else if(objects.Length > 1)
			{
				Debug.LogWarning($"More than one object of type {typeof(T)} found in the Resources/Singletons folder. " +
					$"Returning the first retrieved object, however resolving the conflict is recommended.");
			}

			return objects[0];
		}
	}
}
