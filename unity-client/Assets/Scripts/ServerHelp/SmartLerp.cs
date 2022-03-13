using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading.Tasks;

public static class SmartLerp {

	private static float _cachedPing;
	private const float _speedbaseThreshold = 0.25f; //0.25f; //1; //This number is the number that, multiplied by dt, will be 1 or great so it can be used for a full lerp!

	public static async void reset(){ //Call on the main menu or somewhere before a match!
		_cachedPing = await ping();
		_cachedPing = Mathf.Clamp(_cachedPing, 0.001f, 100);
	}

	public static float networkLerp {
		get {
			return _speedbaseThreshold / _cachedPing;
		}
	}
	
	private static async Task<float> ping(){
		long startTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();

		//Just some server path to test...
		//dynamic serverInfo = 
		await serverCtrl.GetServerInfo();

		float endTime = (DateTimeOffset.Now.ToUnixTimeMilliseconds() - startTime)/1000.0f;
		return endTime;
	}
}
