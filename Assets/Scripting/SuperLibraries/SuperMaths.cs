using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class SuperMaths {
	public static float CosineRule(float b,float c,float theta,bool degrees=true){
		return Mathf.Sqrt(CosineRuleForASquared(b,c,theta,degrees));
	}

	public static float CosineRuleForASquared(float b,float c,float theta,bool degrees=true){
		theta=degrees?(theta*Mathf.Deg2Rad):theta;
		return (b*b+c*c-2*b*c*Mathf.Cos(theta));
	}

	public static float ReverseCosineRule(float a,float b,float c,bool degrees=true){
		return (degrees?Mathf.Rad2Deg:1)*Mathf.Acos((a*a+b*b-c*c)/(2*a*b));
	}

	public static float RoundToNearest(float target,float roundTo){
		if (roundTo==0) return target;
		return Mathf.Floor(target/roundTo)*roundTo;
	}

	public static float RoundTo(float target,float roundTo){
		return RoundToNearest(target,roundTo);
	}

	public static T RandomChoice<T>(System.Random randomGen,T[] pickFrom){
		return pickFrom[RandomRange(randomGen,0,pickFrom.Length,false)];
	}

	public static T RandomChoice<T>(System.Random randomGen,List<T> pickFrom){
		return pickFrom[RandomRange(randomGen,0,pickFrom.Count,false)];
	}

	public static float Atan3(float x,float y){
		float angle;
		if (Mathf.Equals(y,0) || y==0){
			if(Mathf.Equals(x,0) || x==0)
				return 0;
			angle=90f*Mathf.Sign(x);
		}else angle=Mathf.Atan(x/y)*Mathf.Rad2Deg;
		
		angle%=360;
		if (angle<0) angle+=360;
		/*if (x*y<0) angle+=180f;
		if (y<0) angle+=180f;*/
		if (y<0) angle+=180f;
		else angle+=360f;
		angle%=360;
		return angle;
	}

	public static bool AABB2DOLD(Vector4 a,Vector4 b){
		//Debug.Log(a+","+b);
		a.x-=b.x;
		a.y-=b.y;
		b.x=0;
		b.y=0;
		//Debug.Log(a+","+b);

		if (PointInBounds(a.x,a.y,b)) return true;
		if (PointInBounds(a.x+a.z,a.y+a.w,b)) return true;
		if (PointInBounds(a.x,a.y+a.w,b)) return true;
		if (PointInBounds(a.x+a.z,a.y,b)) return true;
		if (PointInBounds(a.x+a.z/2,a.y+a.w/2,b)) return true;

		if (PointInBounds(b.x+b.z,b.y,a)) return true;
		if (PointInBounds(b.x,b.y+b.w,a)) return true;
		if (PointInBounds(b.x+b.z,b.y+b.w,a)) return true;
		if (PointInBounds(b.x,b.y,a)) return true;
		if (PointInBounds(b.x+b.z/2,b.y+b.w/2,a)) return true;
		//Debug.Log("Negative");
		return false;
	} 

	public static bool AABB2D(Vector4 a,Vector4 b){
		//Debug.Log(a+","+b);

		//if (Mathf.Abs(a.x-b.x)>a.z/2) return false;
		//if (Mathf.Abs(a.y-b.y)>a.w/2) return false;
		if (!((a.x>b.x && a.x<b.x+b.z)||(b.x>a.x && b.x<a.x+a.z))) return false;
		if (!((a.y>b.y && a.y<b.y+b.w)||(b.y>a.y && b.y<a.y+a.w))) return false;
		/*if () return false;
		if () return false;
		if (b.y>a.y && b.y<a.y+a.w) return false;*/
		//Debug.Log("Positive");
		return true;
	}

	public static bool PointInBounds(float x,float y,Vector4 bbox){
		if (x<bbox.x) return false;
		if (x>bbox.x+bbox.z) return false;
		if (y<bbox.y) return false;
		if (y>bbox.y+bbox.w) return false;
		return true;
	}

	public static float AbsAngle(float angle){
		while(angle<0) angle+=360;
		while(angle>360) angle-=360;
		return angle;
	}

	public static float RandomRange(System.Random randomGen,float min,float max){
		return min+((float)randomGen.NextDouble())*(max-min);
	}
	public static int RandomRange(System.Random randomGen,int min,int max,bool print=false){
		int num=randomGen.Next(min,max);
		if (print) Debug.Log(num);
		return num;
	}
	public static int RandomRange(System.Random randomGen,int min,int max){
		return randomGen.Next(min,max);
	}

	public static Vector3 ClosestPointOnLine(Vector3 lineStart,Vector3 lineEnd,Vector3 point){
		//Vector3 v=capsuleStart+transform.position,w=capsuleEnd+transform.position;
		//Debug.Log(v+","+w);
		
		float l2=(lineStart-lineEnd).magnitude*(lineStart-lineEnd).magnitude;
		float t=Mathf.Clamp01(Vector3.Dot(point-lineStart,lineEnd-lineStart)/l2);
		Vector3 closestPointOnLine=lineStart+t*(lineEnd-lineStart);
		return closestPointOnLine;
	}
}
