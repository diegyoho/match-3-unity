using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utilities {
	public abstract class SingletonScriptableObject<T> :
		ScriptableObject where T : ScriptableObject {
		static T _instance = null;
		public static T instance {
			get {
				if (!_instance)
					_instance = Resources.FindObjectsOfTypeAll<T>()
								.FirstOrDefault();
				return _instance;
			}
		}
	}
}