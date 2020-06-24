using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utilities {
	public class SingletonMonoBehaviour<T> :
		MonoBehaviour where T : Component {

		static T _ins;

		public static T instance {
			get { return _ins; }
		}

		public virtual void Awake() {
			if (_ins == null) {
				_ins = this as T;
			} else {
				Destroy(this);
			}
		}
	}
}