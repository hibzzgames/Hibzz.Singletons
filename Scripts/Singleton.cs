using UnityEngine;

namespace Hibzz.Singletons
{
	public class Singleton<T> : MonoBehaviour where T : Component
	{
		private static T instance;
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

		// Used to create a new instance of the singleton
		private static T RequestNewInstance()
		{
			T[] items = FindObjectsOfType<T>();
			if (items.Length == 0)
			{
				Singleton<T> t = new Singleton<T>();
				return t.CreateNewInstance();
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
		protected virtual T CreateNewInstance()
		{
			GameObject obj = new GameObject();
			obj.name = typeof(T).Name + "Object";
			return obj.AddComponent<T>();
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
