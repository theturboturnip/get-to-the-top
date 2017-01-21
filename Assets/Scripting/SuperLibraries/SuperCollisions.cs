using UnityEngine;
using System.Collections;

public static class SuperCollisions {

	static Vector3 ClosestPointOnSurface(SphereCollider collider, Vector3 to){
		Vector3 p;

		p = to - collider.transform.position;
		p.Normalize();

		p *= collider.radius * collider.transform.localScale.x;
		p += collider.transform.position;

		return p;
	}

	static Vector3 ClosestPointOnSurface(BoxCollider collider, Vector3 to){
		// Cache the collider transform
		var ct = collider.transform;

		// Firstly, transform the point into the space of the collider
		var local = ct.InverseTransformPoint(to);
		//Debug.Log(collider+","+local);

		// Now, shift it to be in the center of the box
		local -= collider.center;

		// Clamp the points to the collider's extents
		var localNorm =
			new Vector3(
				Mathf.Clamp(local.x/collider.size.x,  -0.5f, 0.5f),
				Mathf.Clamp(local.y/collider.size.y,  -0.5f, 0.5f),
				Mathf.Clamp(local.z/collider.size.z,  -0.5f, 0.5f)
			);

		// Select a face to project on
		if (Mathf.Abs(localNorm.x) > Mathf.Abs(localNorm.y) && Mathf.Abs(localNorm.x) > Mathf.Abs(localNorm.z))
			localNorm.x = Mathf.Sign(localNorm.x)* 0.5f;
		else if (Mathf.Abs(localNorm.y) > Mathf.Abs(localNorm.x) && Mathf.Abs(localNorm.y) > Mathf.Abs(localNorm.z))
			localNorm.y = Mathf.Sign(localNorm.y)* 0.5f;
		else if (Mathf.Abs(localNorm.z) > Mathf.Abs(localNorm.x) && Mathf.Abs(localNorm.z) > Mathf.Abs(localNorm.y))
			localNorm.z = Mathf.Sign(localNorm.z)* 0.5f;

		localNorm.x*=collider.size.x;
		localNorm.y*=collider.size.y;
		localNorm.z*=collider.size.z;

		// Now we undo our transformations
		localNorm += collider.center;

		// Return resulting point
		return ct.TransformPoint(localNorm);
	}

	public static Vector3 ClosestPointOnBoxSurfaceNoVertical(BoxCollider collider, Vector3 to){
		// Cache the collider transform
		var ct = collider.transform;

		// Firstly, transform the point into the space of the collider
		var local = ct.InverseTransformPoint(to);
		//Debug.Log(collider+","+local);

		// Now, shift it to be in the center of the box
		local -= collider.center;

		// Clamp the points to the collider's extents
		var localNorm =
			new Vector3(
				Mathf.Clamp(local.x/collider.size.x,  -0.5f, 0.5f),
				Mathf.Clamp(local.y/collider.size.y,  -0.5f, 0.5f),
				Mathf.Clamp(local.z/collider.size.z,  -0.5f, 0.5f)
			);

		// Select a face to project on
		if (Mathf.Abs(localNorm.x) > Mathf.Abs(localNorm.z))
			localNorm.x = Mathf.Sign(localNorm.x)* 0.5f;
		//else if (Mathf.Abs(localNorm.y) > Mathf.Abs(localNorm.x) && Mathf.Abs(localNorm.y) > Mathf.Abs(localNorm.z))
		//	localNorm.y = Mathf.Sign(localNorm.y)* 0.5f;
		else
			localNorm.z = Mathf.Sign(localNorm.z)* 0.5f;

		localNorm.x*=collider.size.x;
		localNorm.y*=collider.size.y;
		localNorm.z*=collider.size.z;

		// Now we undo our transformations
		localNorm += collider.center;

		// Return resulting point
		return ct.TransformPoint(localNorm);
	}

	static Vector3 ClosestPointOnSurface(CapsuleCollider collider, Vector3 to){
		var ct = collider.transform;
		return ClosestPointOnCapsule(collider.height,collider.radius,ct,to);
	}

	static Vector3 ClosestPointOnSurface(CharacterController collider, Vector3 to){
		var ct=collider.transform;
		return ClosestPointOnCapsule(collider.height,collider.radius,ct,to);
	}

	static Vector3 ClosestPointOnCapsule(float height,float radius,Transform ct,Vector3 to){
		var local = ct.InverseTransformPoint(to);
		Vector3 v=ct.position+Vector3.up*height/2,w=ct.position-Vector3.up*height/2;
		float l2=(v-w).magnitude*(v-w).magnitude;
		float t=Mathf.Clamp01(Vector3.Dot(local-v,w-v)/l2);
		Vector3 closestPointOnLine=v+t*(w-v);
		local=closestPointOnLine+Vector3.ClampMagnitude(local-closestPointOnLine,radius);

		return ct.TransformPoint(local);
	}

	static Vector3 ClosestPointOnSurface(MeshCollider collider, Vector3 to){
		Mesh m = collider.sharedMesh;
		var local=collider.transform.InverseTransformPoint(to);
		//Debug.Log(local+"="+InvTransformPoint(collider.transform,to));

		int closestTriangle=0;
		float sqrDistance=-1f;
		Vector3 /*a,b,c,centre,*/closest=Vector3.zero;
		//int[] tris=m.triangles;
		for(int t=0;t<m.triangles.Length;t+=3){
			/*a=m.vertices[tris[t]];
			b=m.vertices[tris[t+1]];
			c=m.vertices[tris[t+2]];
			centre=(a+b+c)/3;*/
			closest=collider.transform.TransformPoint(ClosestPointOnTriangle(m,t,local));
			if ((closest-to).sqrMagnitude<sqrDistance || sqrDistance<0){
				//Debug.Log(closest+","+to);
				sqrDistance=(closest-to).sqrMagnitude;
				closestTriangle=t;
			}
		}
		closest=ClosestPointOnTriangle(m,closestTriangle,local);
		//Debug.Log("Closest to"+to+" is "+collider.transform.TransformPoint(closest));
		return collider.transform.TransformPoint(closest);
	}

	static Vector3 ClosestPointOnTriangle(Mesh m,int closestTriangle, Vector3 local){
		int[] tris=m.triangles;

		Vector3 a=m.vertices[tris[closestTriangle]];
		Vector3 b=m.vertices[tris[closestTriangle+1]];
		Vector3 c=m.vertices[tris[closestTriangle+2]];
		//Vector3 centre=(a+b+c)/3;

		//Debug.Log(centre+" closest to "+local+" bounds are "+m.bounds.size);

		Matrix4x4 mat=new Matrix4x4();//=Matrix4x4.TRS(-a,Quaternion.FromToRotation(b-a,Vector3.right),Vector3.one);
		Vector3 edge1=b-a,edge2=c-a,normal=Vector3.Cross(edge1,edge2).normalized;
		mat.SetColumn(0, new Vector4(edge1.x,edge1.y,edge1.z));
		mat.SetColumn(1, new Vector4(edge2.x,edge2.y,edge2.z));
		mat.SetColumn(2, new Vector4(normal.x,normal.y,normal.z));
		mat.SetColumn(3, new Vector4(a.x,a.y,a.z,1));
		Vector3 triangleLocal=mat.inverse.MultiplyPoint3x4(local);
		//Debug.Log(mat.inverse.MultiplyPoint(a));
		//Debug.Log(mat.inverse.MultiplyPoint(b));
		//Debug.Log(mat.inverse.MultiplyPoint(c));
		//Debug.Log("b is "+mat.inverse.MultiplyPoint(b)+" or "+mat.inverse*b);
		//triangleLocal=Vector3.Scale(triangleLocal,new Vector3(1/(b-a).magnitude,1/(c-a).magnitude,0));
		//Debug.Log(triangleLocal);
		float k=triangleLocal.x+triangleLocal.y-1;
		if (k>0){
			triangleLocal.x-=k/2;
			triangleLocal.y-=k/2;
		}
		triangleLocal.x=Mathf.Clamp01(triangleLocal.x);
		triangleLocal.y=Mathf.Clamp01(triangleLocal.y);

		//Undo transformations
		//triangleLocal=Vector3.Scale(triangleLocal,new Vector3((b-a).magnitude,(c-a).magnitude,0));
		triangleLocal.z=0;
		//Debug.Log(triangleLocal);
		local=mat.MultiplyPoint3x4(triangleLocal);
		return local;
	}

	public static Vector3 ClosestPointOnSurface(Collider collider, Vector3 to){
		if (collider is BoxCollider) return ClosestPointOnSurface(collider as BoxCollider,to);
		if (collider is CapsuleCollider) return ClosestPointOnSurface(collider as CapsuleCollider,to);
		if (collider is SphereCollider) return ClosestPointOnSurface(collider as SphereCollider,to);
		if (collider is CharacterController) return ClosestPointOnSurface(collider as CharacterController,to);
		if (collider is MeshCollider) return ClosestPointOnSurface(collider as MeshCollider,to);
		if (collider==null) return to;
		return collider.ClosestPointOnBounds(to);
	}

	static Vector3 ClosestPointInBounds(SphereCollider collider, Vector3 to){
		Vector3 p;

		p = to - collider.transform.position;
		if (p.magnitude>collider.radius*Vector3.Dot(collider.transform.localScale,p))
			p=p.normalized*(collider.radius*Vector3.Dot(collider.transform.localScale,p));

		p += collider.transform.position;

		return p;
	}

	static Vector3 ClosestPointInBounds(BoxCollider collider,Vector3 to){
		// Cache the collider transform
		var ct = collider.transform;

		// Firstly, transform the point into the space of the collider
		var local = ct.InverseTransformPoint(to);
		//Debug.Log(collider+","+local);

		// Now, shift it to be in the center of the box
		local -= collider.center;

		// Clamp the points to the collider's extents
		var localNorm =
			new Vector3(
				Mathf.Clamp(local.x/collider.size.x,  -0.5f, 0.5f),
				Mathf.Clamp(local.y/collider.size.y,  -0.5f, 0.5f),
				Mathf.Clamp(local.z/collider.size.z,  -0.5f, 0.5f)
			);

		localNorm.x*=collider.size.x;
		localNorm.y*=collider.size.y;
		localNorm.z*=collider.size.z;

		// Now we undo our transformations
		localNorm += collider.center;

		// Return resulting point
		return ct.TransformPoint(localNorm);
	}

	static Vector3 ClosestPointInCapsule(float height,float radius,Transform ct,Vector3 to){
		var local=ct.InverseTransformPoint(to);
		local.x=Mathf.Clamp(local.x/radius,-1,1);
		local.y=Mathf.Clamp(local.y/(height/2),-1,1);
		local.z=Mathf.Clamp(local.z/radius,-1,1);
		if (Vector3.Equals(local,Vector3.zero)) local=Vector3.up;
		local=Vector3.Scale(local,new Vector3(radius,height/2,radius));
		return ct.TransformPoint(local);
	}

	static Vector3 ClosestPointInBounds(CapsuleCollider collider, Vector3 to){
		var ct=collider.transform;
		return ClosestPointInCapsule(collider.height,collider.radius,ct,to);
	}

	static Vector3 ClosestPointInBounds(CharacterController collider, Vector3 to){
		var ct=collider.transform;
		return ClosestPointInCapsule(collider.height,collider.radius,ct,to);
	}

	static Vector3 ClosestPointInBounds(MeshCollider collider, Vector3 to){
		return ClosestPointOnSurface(collider,to);
	}

	public static Vector3 ClosestPointInBounds(Collider collider, Vector3 to){
		if (collider is BoxCollider) return ClosestPointInBounds(collider as BoxCollider,to);
		if (collider is CapsuleCollider) return ClosestPointInBounds(collider as CapsuleCollider,to);
		if (collider is SphereCollider) return ClosestPointInBounds(collider as SphereCollider,to);
		if (collider is CharacterController) return ClosestPointInBounds(collider as CharacterController,to);
		if (collider is MeshCollider) return ClosestPointInBounds(collider as MeshCollider,to);
		return collider.ClosestPointOnBounds(to);
	}
}
