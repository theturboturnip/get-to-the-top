using UnityEngine;
using System.Collections;

public class IKTester : MonoBehaviour {
	public Transform[] boneChain,exampleBoneChain;
	public bool testWithPosition=false;
	public float[] boneRadii;
	public float targetCameraDepth;
	public Vector3 currentTarget;
	Vector3 elbowPos;
	Vector3[] lookPositions,startDirections;

	void Start(){
		if(testWithPosition){
			lookPositions=new Vector3[exampleBoneChain.Length];
			for(int i=0;i<exampleBoneChain.Length;i++)
				lookPositions[i]=exampleBoneChain[i].position;
		}else{
			lookPositions=new Vector3[boneChain.Length];
			for(int i=0;i<boneChain.Length;i++)
				lookPositions[i]=boneChain[i].position;
		}
		startDirections=new Vector3[boneChain.Length];
		for(int i=1;i<startDirections.Length;i++){
			startDirections[i-1]=(boneChain[i].position-boneChain[i-1].position).normalized;
		}
	}

	void OldUpdate(){
		Vector3 currentTarget=Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x,Input.mousePosition.y,targetCameraDepth));
		Vector3 boneEndPosition=boneChain[2].position;//boneChain[1].position+boneChain[1].forward*bone2Length;
		//
		elbowPos=InverseKinematics.TwoBoneInvKinematics(boneRadii,currentTarget-boneChain[0].position,Vector3.down);
		//USE FROM TO ROTATION WITH ORIGINAL BONE DIRECTIONS INSTEAD OF LOOKAT
		//LOOKAT ASSUMES DIFFERENT ROTATIONS
		//boneChain[0].LookAt(Vector3.one);
		boneChain[0].LookAt(elbowPos);
		//boneChain[1].LookAt(currentTarget);
		if((currentTarget-elbowPos).magnitude>boneRadii[1])
			currentTarget=elbowPos+(currentTarget-elbowPos).normalized*boneRadii[1];
		exampleBoneChain[0].position=boneChain[0].position;
		//exampleBoneChain[1].position=elbowPos;
		exampleBoneChain[1].position=currentTarget;
		//mousePointer.position=elbowPos;
	}

	void Update(){
		currentTarget=Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x,Input.mousePosition.y,targetCameraDepth));
		//Vector3 boneEndPosition=boneChain[2].position;
		InverseKinematics.ApplyInvKinematicsRotation(boneChain,currentTarget,/*boneRadii*/null,lookPositions);
		
		lookPositions=InverseKinematics.NBoneInvKinematics(boneRadii,currentTarget,lookPositions);
		if(testWithPosition){
			exampleBoneChain[0].position=Vector3.zero;
			for(int b=1;b<exampleBoneChain.Length;b++){
				exampleBoneChain[b].position=lookPositions[b-1];
			}
		}
	}
}
