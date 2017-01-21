using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[ExecuteInEditMode]
public class PhysicsDebug : MonoBehaviour {
	//public float radius = 3f; // show penetration into the colliders located inside a sphere of this radius
	//public int maxNeighbours = 16; // maximum amount of neighbours visualised

	//private Collider[] neighbours;
	CapsuleCollider ourCollider;

	public void Start()
	{
		//neighbours = new Collider[maxNeighbours];
		ourCollider=GetComponent<CapsuleCollider>();
		ourCollider.enabled=false;
	}

	public void Update(){
		Collider[] isectArray=Physics.OverlapCapsule(transform.position+Vector3.up*(ourCollider.height/2-ourCollider.radius),transform.position+Vector3.down*(ourCollider.height/2-ourCollider.radius), ourCollider.radius);
		Vector3 totalMoveDelta=Vector3.zero;
		foreach (Collider c in isectArray){
			if (c==ourCollider) continue;
			if (!c.enabled) continue;
			if (c.isTrigger) continue;
		
			Vector3 otherPosition = c.gameObject.transform.position;
			Quaternion otherRotation = c.gameObject.transform.rotation;

			Vector3 direction=Vector3.zero;
			float distance=0;

			ourCollider.enabled=true;
			bool overlapped = Physics.ComputePenetration(
				c, otherPosition, otherRotation,
				ourCollider, transform.position, transform.rotation,  
				out direction, out distance
			);
			ourCollider.enabled=false;

			if (overlapped){
				totalMoveDelta+=direction*distance;
			}
		}
		transform.position+=-totalMoveDelta;
	}

	/*public void OnDrawGizmos()
	{
		var thisCollider = GetComponent<CapsuleCollider>();

		//if (!thisCollider)
		//	return; // nothing to do without a Collider attached
		//Debug.Log(thisCollider.radius);
		int count = Physics.OverlapCapsuleNonAlloc(transform.position+Vector3.up*(thisCollider.height/2-thisCollider.radius),transform.position+Vector3.down*(thisCollider.height/2-thisCollider.radius), thisCollider.radius, neighbours);
		Debug.Log(count);
		bool anyOverlap=false;
		Vector3 totalMove=Vector3.zero;
		for (int i = 0; i < count; ++i)
		{
			var collider = neighbours[i];

			if (collider == thisCollider)
				continue; // skip ourself

			Debug.Log("Checking overlap");
			Vector3 otherPosition = collider.gameObject.transform.position;
			Quaternion otherRotation = collider.gameObject.transform.rotation;

			Vector3 direction=Vector3.zero;
			float distance=0;

			thisCollider.enabled=true;
			bool overlapped = Physics.ComputePenetration(
				collider, otherPosition, otherRotation,
				thisCollider, transform.position, transform.rotation,  
				out direction, out distance
			);
			thisCollider.enabled=false;
			if (overlapped){
				anyOverlap=true;
				totalMove+=direction*distance;
			}

			// draw a line showing the depenetration direction if overlapped
		}
		if (anyOverlap)
			{
				Gizmos.color = Color.red;
				Gizmos.DrawRay(transform.position, -totalMove);
				Gizmos.DrawWireSphere(transform.position-totalMove+Vector3.up*(thisCollider.height/2-thisCollider.radius), thisCollider.radius);
				Gizmos.DrawWireSphere(transform.position-totalMove+Vector3.down*(thisCollider.height/2-thisCollider.radius), thisCollider.radius);
			}
	}*/
}
