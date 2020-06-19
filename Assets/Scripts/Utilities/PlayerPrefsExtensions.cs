using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utilities {
    public static class PlayerPrefsExtensions {

        public static void SetBool(string key, bool value) {
            PlayerPrefs.SetInt(key, value ? 1 : 0);
        }

        public static bool GetBool(string key, bool value = false) {
            return PlayerPrefs.GetInt(key, value ? 1 : 0) != 0;
        }
    }
}