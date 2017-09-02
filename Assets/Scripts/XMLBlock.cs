using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XMLBlock : MonoBehaviour {

	public void Initiallize(BlockEntry b){
		this.transform.position = b.position;
		this.transform.localScale = b.size;
	}
}
