using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PhysicsObjectHandler : MonoBehaviour {
	public static PhysicsObjectHandler currentHandler;
	public int maxObjects=10;
	public float maxLifeTime=50;//In seconds
	List<GameObject> objects;
	//Dictionary<GameObject,float> lifeSpans;

	//List<GameObject> objects;
	// Use this for initialization
	void Start () {
		if(currentHandler!=null){
			Debug.LogError("Another PhysicsObjectHandler already exists!");
			this.enabled=false;
			return;
		}
		objects=new List<GameObject>();
		currentHandler=this;	
	}
	
	// Update is called once per frame
	void Update () {
		while (objects.Count>maxObjects&&maxObjects>0){
			Destroy(objects[0]);
			objects.RemoveAt(0);
		}
		foreach(GameObject g in objects){
			if (g==null){
				objects.Remove(g);
				break;
			}
			if (g.transform.position.y<-100){
				objects.Remove(g);
				Destroy(g);
				break;
			}
		}
		/*foreach(GameObject g in keys){
			//Debug.Log(g);
			lifeSpans[g]+=Time.deltaTime;
			if(lifeSpans[g]>maxLifeTime){
				lifeSpans.Remove(g);
				Destroy(g);
				break;
			}
		}*/
	}

	public GameObject CreateObject(GameObject physicsObject,Vector3 position,Quaternion rotation,Vector3 velocity,Vector3 torque){
		GameObject po=Instantiate(physicsObject,position,rotation) as GameObject;
		/*Rigidbody r=po.GetComponent<Rigidbody>();
		if(r==null)
			r=po.AddComponent<Rigidbody>();
		r.AddForce(velocity,ForceMode.VelocityChange);
		r.AddRelativeTorque(torque,ForceMode.Force);*/
		PhysicsJunk pj=po.GetComponent<PhysicsJunk>();
		if(pj==null) pj=po.AddComponent<PhysicsJunk>();
		pj.velocity=velocity;
		pj.torque=torque;
		po.transform.parent=transform;
		objects.Add(po);
		Destroy(po,maxLifeTime);
		return po;
		//objects.Add(po);
	}	
}
