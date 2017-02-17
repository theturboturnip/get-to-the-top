using UnityEngine;
using System.Collections;

public class InverseKinematics {
	public static Vector3 TwoBoneInvKinematics(float[] boneRadii,Vector3 targetVector,Vector3 hintDirection,bool hintIsPosition=false){
		//Assuming that target is r2 away from boneChain[1]
		float r1=boneRadii[0],r2=boneRadii[1],d=targetVector.magnitude;
		float s=r1+r2-d;//maximum seperation of two points on the spheres of radius r1 and r2 with positions boneChain[0] and boneChain[2]
		s=Mathf.Max(0,s);
		float m=r1-s/2;//distance between boneChain[0] and the midpoint of two points with maximum seperation on the spheres [...]
		Vector3 midpoint=targetVector.normalized*m;
		float ea=Mathf.Sqrt(r1*r1-(r1-s/2)*(r1-s/2)); //extrusion amount from midpoint
		if (hintIsPosition) hintDirection-=midpoint;
		//for the position of boneChain[1] extrude by ea in hintDirection from midpoint
		// target normal is closest vector to hintDirection which is perpendicular to targetVector
		// D=hintDirection X targetVector
		// target normal= targetVector X D
		// use dot product check for parallelness
		Vector3 targetNormal=Vector3.Cross(targetVector,Vector3.Cross(hintDirection,targetVector));
		if(Vector3.Dot(targetNormal,hintDirection)<0) targetNormal=-targetNormal;
		return midpoint+targetNormal.normalized*ea;
	}

	/*public static Vector3 TwoBoneInvKinematics(float[] boneRadii, Vector3 targetVector, Vector3 hintDirection){
		return TwoBoneInvKinematicsDirectionHint(boneRadii,targetVector,hintDirection);
	}*/

	/*public static Vector3 TwoBoneInvKinematicsPositionHint(float[] boneRadii,Vector3 targetVector,Vector3 hintPosition){
		//Assuming that target is r2 away from boneChain[1]
		float r1=boneRadii[0],r2=boneRadii[1],d=targetVector.magnitude;
		float s=r1+r2-d;//maximum seperation of two points on the spheres of radius r1 and r2 with positions boneChain[0] and boneChain[2]
		s=Mathf.Max(0,s);
		float m=r1-s/2;//distance between boneChain[0] and the midpoint of two points with maximum seperation on the spheres [...]
		Vector3 midpoint=targetVector.normalized*m;
		float ea=Mathf.Sqrt(r1*r1-m*m); //extrusion amount from midpoint
		Vector3 hintDirection=hintPosition-midpoint;
		//for the position of boneChain[1] extrude by ea in hintDirection from midpoint
		// target normal is closest vector to hintDirection which is perpendicular to targetVector
		// D=hintDirection X targetVector
		// target normal= targetVector X D
		// use dot product check for parallelness
		Vector3 targetNormal=Vector3.Cross(targetVector,Vector3.Cross(hintDirection,targetVector));
		if(Vector3.Dot(targetNormal,hintDirection)<0) targetNormal=-targetNormal;
		return midpoint+targetNormal.normalized*ea;
	}*/

	public static Vector3[] NBoneInvKinematics(float[] boneRadii,Vector3 endTargetVector,Vector3[] hints,bool returnDirections=false,bool hintsArePositions=true){
		Vector3[] points=new Vector3[boneRadii.Length],directions=new Vector3[boneRadii.Length]; //The end is the target
		float i,j,d;
		Vector3 targetVector,hintDir=Vector3.zero;

		for (int k=0; k<boneRadii.Length-1; k++){
			i=boneRadii[k];
			j=0;
			for(int b=k+1;b<boneRadii.Length;b++)
				j+=boneRadii[b];
			if (k>0){
				targetVector=(endTargetVector-points[k-1]);
				if (hintsArePositions)
					hintDir=hints[k]-points[k-1];
			}else{
				targetVector=endTargetVector;
				hintDir=hints[k];
			}
			d=Mathf.Max(targetVector.magnitude,j-i);
			directions[k]=TwoBoneInvKinematics(new float[]{i,j},targetVector.normalized*d,hintDir,hintsArePositions);
			points[k]=directions[k];
			if (k>0) points[k]+=points[k-1];
		}
		if(points.Length>=2){
			directions[points.Length-1] = (endTargetVector-points[points.Length-2]).normalized*boneRadii[boneRadii.Length-1];
			points[points.Length-1]= directions[directions.Length-1]+points[points.Length-2];
		}else{
			directions[points.Length-1] = endTargetVector.normalized*boneRadii[boneRadii.Length-1];
			points[points.Length-1]= directions[points.Length-1];
		}

		return returnDirections?directions:points;
	}

	public static void ApplyInvKinematicsRotation(Transform[] bones,Vector3 endTargetVector,float[] boneRadii=null,Vector3[] hints=null,bool endTargetIsDirection=false,Vector3 upDirection=default(Vector3)){
		if(bones.Length<2) return;

		//WARNING! THIS FUNCTION DOESN'T PLAY WELL WITH SCALING. USE MODELS WITH UNIFORM SCALE (1,1,1) 
		
		if(boneRadii==null){
			boneRadii=new float[bones.Length-1];
			//Ignore first bone
			for(int i=1;i<bones.Length;i++){
				boneRadii[i-1]=(bones[i].position-bones[i-1].position).magnitude;
			}
		}
		if(!endTargetIsDirection){
			endTargetVector-=bones[0].position;
		}
		Vector3[] oldPositions=new Vector3[bones.Length-1];
		for(int i=0;i<bones.Length-1;i++)
			oldPositions[i]=bones[i+1].position-bones[0].position;
		if (hints==null)
			hints=oldPositions;
		else
			hints[0]-=bones[0].position;
		Debug.Log("IK Prep Dump");
		Debug.Log("End Dir: "+endTargetVector);
		foreach(Vector3 hint in hints){
			Debug.Log(hint);
		}
		Vector3[] newDirections=NBoneInvKinematics(boneRadii,endTargetVector,hints,true,true);
		
		//int upCompensationMode=0;
		for(int i=0;i<newDirections.Length;i++){
			//Generate a rotation that puts relative position of the next bone at newDirections[i]
			Vector3 localPos=bones[i+1].localPosition;
			Quaternion intendedRot=Quaternion.FromToRotation(localPos.normalized,newDirections[i].normalized);
			//Sum all rotations from parent
			bones[i].rotation=Quaternion.identity;//Inverse(bones[i].root.rotation);
			//Debug.Log(bones[i].localRotation==Quaternion.identity);
			bones[i].rotation=intendedRot;
			//Now we have the rotation, rotate it around the local z so that their up vector is as close to up as possible
			//Quaternion q does this
			//q*bones.local*Vector3.up . upDirection is at maximum;
			//transform updir into local space
			//A=boneUp.x*upDir.x+boneUp.y*Updir.y (boneUp is 0,1,0 => A=upDir.y)
			//B=boneUp.y*upDir.x-boneUp.x*updir.y (boneUp is 0,1,0 => B=upDir.x)
			//rotate by atan2(B,A) around local Z
			if (upDirection.magnitude==0) continue;
			Vector3 localUpDir=bones[i].InverseTransformDirection(upDirection).normalized;
			float turnAngle=-Mathf.Rad2Deg*Mathf.Atan2(localUpDir.z,localUpDir.x);
			bones[i].Rotate(Vector3.up*turnAngle,Space.Self);
			if (Vector3.Dot(bones[i].up,upDirection)<0)
				bones[i].Rotate(Vector3.up*180,Space.Self);
		}
	}

	static Vector3 UnScale(Vector3 target,Vector3 scalingFactor){
		target.x/=scalingFactor.x;
		target.y/=scalingFactor.y;
		target.z/=scalingFactor.z;
		return target;
	}
}
