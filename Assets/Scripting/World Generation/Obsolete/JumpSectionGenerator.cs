using UnityEngine;
using System.Collections;

public class JumpSectionGenerator : PathSectionGenerator {
	public float minimumJumpLength,maximumJumpLength;

	public override Building Apply(ref Building toReturn,WorldGenv2 wg,float[] path,float t,Building previous){
		toReturn=base.Apply(ref toReturn,wg,path,t,previous);
		float currentDistance=Mathf.Lerp(path[0],path[1],t);
		toReturn.angle+=SuperMaths.ReverseCosineRule(currentDistance,currentDistance,Random.Range(minimumJumpLength,maximumJumpLength));
		//toReturn.yRot=-toReturn.angle;//wg.RandomRange(0f,360f);
		//toReturn.yRot+=wg.RandomRange(-10f,10f);
		return toReturn;
	}
}
