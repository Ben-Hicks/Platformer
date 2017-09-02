using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

public class XMLManager : MonoBehaviour {

	//bad singleton pattern
	public static XMLManager inst;
	public bool shouldLoad;
	public string path;

	void Awake(){
		inst = this;
		if(shouldLoad)LoadItems ();
	}

	public BlockDatabase blockDB;

	public void SaveBlocks(){
		XMLBlock[] blocks= Object.FindObjectsOfType<XMLBlock>();
		foreach(XMLBlock block in blocks){
			BlockEntry be = new BlockEntry (block.transform.position, block.transform.localScale);
			inst.blockDB.listBlock.Add (be);
		}

		XmlSerializer serializer = new XmlSerializer (typeof(BlockDatabase));
		FileStream fstream = new FileStream (Application.dataPath + "/StreamingAssets/XML/" + path, 
			FileMode.Create);
		serializer.Serialize (fstream, blockDB);
		fstream.Close ();
	}

	public void LoadItems(){
		XmlSerializer serializer = new XmlSerializer (typeof(BlockDatabase));
		FileStream fstream = new FileStream (Application.dataPath + "/StreamingAssets/XML/" + path, 
			FileMode.Open);
		blockDB = serializer.Deserialize (fstream) as BlockDatabase;
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

//probably do a seperate one for moving platforms
//then a seperate one for enemies

[System.Serializable]
public class BlockDatabase{

	[XmlArray("Blocks")]
	public HashSet<BlockEntry> listBlock = new HashSet<BlockEntry>();

}