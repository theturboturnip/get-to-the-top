using UnityEngine;
using System.Collections;

/*public class AdaptiveViewDist : MonoBehaviour {
	public float padding=10f;
	Camera cam;
	void OnEnable(){
		cam=GetComponent<Camera>();
	}

	void OnPreCull(){
		//islandend=position+forward*r
		//dist(islandend,zero)=worlddata.radius
		//get any point on islandend-centre
		/*Vector3 centreToTransform=transform.position;
		//forward vector = the edge vector with a negative dot of 
		float hFOV=Mathf.Rad2Deg * 2 * Mathf.Atan(Mathf.Tan(cam.fieldOfView * Mathf.Deg2Rad / 2) * cam.aspect);
		Vector3 f1=Quaternion.Euler(Vector3.up*hFOV/2)*transform.forward,f2=Quaternion.Euler(Vector3.up*-hFOV/2)*transform.forward;
		float a=1,b=(-2*centreToTransform.magnitude*Vector3.Dot(centreToTransform.normalized,-transform.forward)),b1=(-2*centreToTransform.magnitude*Vector3.Dot(centreToTransform.normalized,-f1)),b2=(-2*centreToTransform.magnitude*Vector3.Dot(centreToTransform.normalized,-f2)),c=(centreToTransform.magnitude*centreToTransform.magnitude-WorldData.worldRadius*WorldData.worldRadius);
		float r1=(-b1+Mathf.Sqrt(b1*b1-4*a*c))/(2*a),r2=(-b2+Mathf.Sqrt(b2*b2-4*a*c))/(2*a);
		float r=Mathf.Max(transform.position.y,r1,r2);
		cam.farClipPlane=r+padding;
	}
}*/
