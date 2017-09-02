using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadLevel : MonoBehaviour {

	public GameObject blockPrefab;

	// Use this for initialization
	void Start () {

		CreateBlocks ();

	}

	public void CreateBlocks(){
		foreach (BlockEntry b in XMLManager.inst.blockDB.listBlock) {
			GameObject newBlockGO = (GameObject)Instantiate (blockPrefab);
			newBlockGO.transform.SetParent (transform, false);
			XMLBlock newBlock = newBlockGO.GetComponent<XMLBlock> ();
			newBlock.Initiallize (b);
		}
	}

}
