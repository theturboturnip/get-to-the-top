using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public struct IKRig{
	public string id;
	public Transform[] boneChain;
	public Transform attachPoint;
	public Vector3 up,forward;
	public bool notNull;
	public Transform[] jointHints;
	public Vector3[] hintCache;
	public IKController controller;
}

public class PlayerIK : MonoBehaviour {
	public Transform leftShoulder,rightShoulder;
	public int armCountDownAmount=3;

	Dictionary<string,IKRig> rigs;

	//Transform[] leftArmChain,rightArmChain;

	IKController controller;
	//Vector3 lHandIK,rHandIK;
	//Vector3 lArmUp,rArmUp;

	//List<Vector3> hintList;

	void Start(){
		rigs=new Dictionary<string,IKRig>();
		AddIKRig("LeftArm",leftShoulder,armCountDownAmount);
		AddIKRig("RightArm",rightShoulder,armCountDownAmount);
	}

	public bool RequestControl(IKController wantsControl,string id){
		IKRig rig=FindRig(id);
		if (!rig.notNull) return false;

		if (rig.controller==null){
			rig.controller=wantsControl;
			ApplyRig(rig);
			return true;
		}
		if (rig.controller.AllowControl(wantsControl,rig)){
			rig.controller=wantsControl;
			ApplyRig(rig);
			return true;
		}
		return false;
	}

	/*
		RIG CREATION
					   */

	IKRig CreateIKRig(string id,Transform start,int length,Transform attach=null){
		IKRig toReturn=new IKRig();
		toReturn.id=id;

		Transform[] chain=new Transform[length];
		chain[0]=start;
		for (int i=1;i<length;i++)
			chain[i]=chain[i-1].GetChild(0);
		toReturn.boneChain=chain;

		toReturn.attachPoint=attach;
		toReturn.notNull=true;
		toReturn.jointHints=new Transform[length-2];
		toReturn.hintCache=new Vector3[length-2];
		return toReturn;
	}

	void AddIKRig(string id,Transform start,int length,Transform attach=null){
		rigs[id]=CreateIKRig(id,start,length,attach);
	}

	/*
		RIG ASSIGNMENT
					    */

	public bool SetIKHandle(IKController ikc,string id,Transform newHandle,bool inheritDirections=true){
		IKRig rig=FindRig(id);
		if (!rig.notNull) return false;
		if (ikc!=rig.controller) return false;

		rig.attachPoint=newHandle;
		if (inheritDirections){
			rig.up=newHandle.up;
			rig.forward=newHandle.forward;
		}

		ApplyRig(rig);
		return true;
	}

	public bool SetIKDirections(IKController ikc,string id,Vector3 up=default(Vector3),Vector3 forward=default(Vector3)){		
		IKRig rig=FindRig(id);
		if (!rig.notNull) return false;
		if (ikc!=rig.controller) return false;

		rig.up=up;
		rig.forward=forward;

		ApplyRig(rig);
		return true;
	}

	public bool SetIKJointHint(IKController ikc,string id,Transform jointHandle,int index=0){		
		IKRig rig=FindRig(id);
		if (!rig.notNull) return false;
		if (ikc!=rig.controller) return false;

		rig.jointHints[index]=jointHandle;

		ApplyRig(rig);
		return true;
	}

	/*
		RIG APPLICATION
						  */

	void ApplyIKToRig(IKRig rig){
		if (!rig.notNull) return;
		if (rig.attachPoint==null) return;

		bool validHints=true;
		for(int i=0;i<rig.jointHints.Length;i++){
			if (rig.jointHints[i]==null){
				validHints=false;
				break;
			}
			rig.hintCache[i]=rig.jointHints[i].position;
		}
		Vector3[] hints=validHints?rig.hintCache:null;
		rig.up=rig.attachPoint.up;
		rig.forward=rig.attachPoint.forward;
		//if (!validHints)
		//	Debug.Log(rig.id+" hints invalud!");
		//Debug.Log(rig.attachPoint);
		InverseKinematics.ApplyInvKinematicsRotation(rig.boneChain,rig.attachPoint.position,upDirection : rig.up,hints:hints);
	}

	void LateUpdate(){
		if (rigs==null) Start();
		foreach(KeyValuePair<string, IKRig> pair in rigs)
			ApplyIKToRig(pair.Value);
	}

	/*
		UTIL
			  */

	public IKRig FindRig(string id){
		if (rigs==null) Start();
		return rigs[id];
	}

	public bool ApplyRig(IKRig toApply){
		rigs[toApply.id]=toApply;
		return false;
	}

}
