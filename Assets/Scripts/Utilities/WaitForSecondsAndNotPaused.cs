using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utilities {
    public class WaitForSecondsAndNotPaused : CustomYieldInstruction {
        
        float seconds;
        Func<bool> isPaused;
        float initialTime;
        
        bool paused {
            get {
                if(isPaused()) {
                    seconds -= Mathf.Max(deltaTime, 0);
                    initialTime = Time.time;
                }
                return isPaused(); 
            }
        }

        float deltaTime {
            get { return Time.time - initialTime; }
        }

        public override bool keepWaiting {
            get
            {
                return paused || (deltaTime < seconds);
            }
        }

        public WaitForSecondsAndNotPaused(
            float seconds, Func<bool> isPaused
        ) {
           this.seconds = seconds;
           this.isPaused = isPaused;
           initialTime = Time.time;
        }
    }
}
