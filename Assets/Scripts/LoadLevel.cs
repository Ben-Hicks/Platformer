using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadLevel : MonoBehaviour {

	public GameObject blockPrefab;
	public GameObject platformPrefab;

	// Use this for initialization
	void Start () {

		CreateBlocks ();
		CreatePlatforms ();

	}

	public void CreateBlocks(){
		foreach (BlockEntry b in XMLManager.inst.xmlDB.listBlock) {
			GameObject newBlockGO = (GameObject)Instantiate (blockPrefab);
			newBlockGO.transform.SetParent (transform, false);
			XMLBlock newBlock = newBlockGO.GetComponent<XMLBlock> ();
			newBlock.Initiallize (b);
		}
	}

	public void CreatePlatforms(){
		foreach (PlatformEntry p in XMLManager.inst.xmlDB.listPlatform) {
			GameObject newPlatformGO = (GameObject)Instantiate (platformPrefab);
			newPlatformGO.transform.SetParent (transform, false);
			XMLPlatform newPlatform = newPlatformGO.GetComponent<XMLPlatform> ();
			newPlatform.Initiallize (p);
		}
	}

}
