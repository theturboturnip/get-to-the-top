using UnityEngine;
using System.Collections;

public class LevelCheckpoint : MonoBehaviour {
	//public Transform[] toLoad;
	public Bounds boxBounds;
	public Vector3 spawnLookDir;
	public bool requireShotgun=false;
	public Transform[] toRaise;


	// Use this for initialization
	void Start () {
		if (toRaise==null) return;
		foreach(Transform t in toRaise){
			t.gameObject.SetActive(false);
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (LevelHandler.currentLevel==null) return;

		Vector3 playerPos=transform.InverseTransformPoint(LevelHandler.player.position);
		if (boxBounds.Contains(playerPos)&&(!requireShotgun||LevelHandler.shotgun.gameObject.activeSelf)){
			LevelHandler.currentLevel.ActivateCheckpoint(this);
			this.enabled=false;
		}
	}

	void OnDrawGizmos(){
		if (!this.enabled) return;
		Gizmos.color=Color.green;
		Gizmos.matrix=transform.localToWorldMatrix;
		Gizmos.DrawWireCube(boxBounds.center,boxBounds.size);
		Gizmos.matrix=Matrix4x4.TRS(transform.position,transform.rotation,Vector3.one);
		Gizmos.color=Color.blue;
		Gizmos.DrawLine(boxBounds.center,boxBounds.center+spawnLookDir*2);
		Gizmos.DrawSphere(boxBounds.center+spawnLookDir*2,0.1f);
	}
}
