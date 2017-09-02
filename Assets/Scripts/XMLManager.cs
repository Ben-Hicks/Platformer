using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using System.Xml.Serialization;
using System.IO;


/* CURRENT PROBLEMS
 *  Platform starting locations are always 0,0 which makes sense as coded
 *  need some other way to store initial position nicely
 * I'm also not really even sure if it's worht having XML saving
 * - might just try to mess around with Scene transitions
 */

public class XMLManager : MonoBehaviour {

	//bad singleton pattern
	public static XMLManager inst;
	public bool shouldLoad;
	public string path;

	void Awake(){
		inst = this;
		if(shouldLoad)LoadItems ();
	}

	public XMLDatabase xmlDB;

	public void SaveBlocks(){
		XMLBlock[] blocks= Object.FindObjectsOfType<XMLBlock>();
		foreach(XMLBlock block in blocks){
			BlockEntry be = new BlockEntry (block.transform.position, block.transform.localScale);
			inst.xmlDB.listBlock.Add (be);
		}
			
		XMLPlatform[] platforms= Object.FindObjectsOfType<XMLPlatform>();
		foreach(XMLPlatform platform in platforms){
			//not sure if this works
			PlatformController pc = platform.GetComponent<PlatformController> ();
			PlatformEntry pe = new PlatformEntry (platform.transform.localScale,
				                   pc.localWaypoints, pc.speed, pc.cyclic, pc.easeAmount, pc.waitTime);
			inst.xmlDB.listPlatform.Add (pe);
		}


		XmlSerializer serializer = new XmlSerializer (typeof(XMLDatabase));
		FileStream fstream = new FileStream (Application.dataPath + "/StreamingAssets/XML/" + path, 
			FileMode.Create);
		serializer.Serialize (fstream, xmlDB);
		fstream.Close ();
	}

	public void LoadItems(){
		XmlSerializer serializer = new XmlSerializer (typeof(XMLDatabase));
		FileStream fstream = new FileStream (Application.dataPath + "/StreamingAssets/XML/" + path, 
			FileMode.Open);
		xmlDB = serializer.Deserialize (fstream) as XMLDatabase;
		fstream.Close ();
	}
}

[System.Serializable]
public class BlockEntry{
	public Vector3 position;
	public Vector3 size;
	public BlockEntry(){}
	public BlockEntry(Vector3 _position, Vector3 _size){
		position = _position;
		size = _size;
	}
}

[System.Serializable]
public class PlatformEntry{
	public Vector3 size;
	public Vector3[] waypoints;
	public float speed;
	public bool cyclic;
	public float easeAmount;
	public float waitTime;
	public PlatformEntry(){}
	public PlatformEntry(Vector3 _size, Vector3[] _waypoints,
		float _speed, bool _cyclic, float _easeAmount, float _waitTime){
		size = _size;
		waypoints = _waypoints;
		speed = _speed;
		cyclic = _cyclic;
		easeAmount = _easeAmount;
		waitTime = _waitTime;
	}
}

//probably do a seperate one for moving platforms
//then a seperate one for enemies

[System.Serializable]
public class XMLDatabase{

	[XmlArray("Blocks")]
	public HashSet<BlockEntry> listBlock = new HashSet<BlockEntry>();

	[XmlArray("Platforms")]
	public HashSet<PlatformEntry> listPlatform = new HashSet<PlatformEntry>();

}