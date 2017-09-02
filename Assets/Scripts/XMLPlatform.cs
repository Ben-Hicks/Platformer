using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XMLPlatform : MonoBehaviour {

	public void Initiallize(PlatformEntry p){
		PlatformController pc = GetComponent<PlatformController> ();
		pc.Initiallize (p.size, p.waypoints,
			p.speed, p.cyclic, p.easeAmount, p.waitTime);

	}
}
