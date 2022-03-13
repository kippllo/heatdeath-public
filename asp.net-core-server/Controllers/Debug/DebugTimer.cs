using System;
using Keybored.BackendServer.Logging;

namespace Keybored.BackendServer.Debug {

	public class DebugTimer {

		private long startTime;

		public DebugTimer() {
			startTimer();
		}

		public void startTimer(){
			startTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
		}

		public float time {
			get{
				return (DateTimeOffset.Now.ToUnixTimeMilliseconds() - startTime)/1000.0f;
			}
		}

		//returns the time in seconds and prints it to the log.
		public float stopTimer(string msg){
			float t = (DateTimeOffset.Now.ToUnixTimeMilliseconds() - startTime)/1000.0f;
			printTimeToLog(t, msg);
			return t;
		}

		private void printTimeToLog(float timerLength, string msg) {
			FileLogger.logToFileSynchronous(msg + timerLength +"\n");
		}
	}
}