using UnityEngine;
using System.Collections;

public class WallrunSectionGenerator : PathSectionGenerator {
	public float minVerticalDisplacement,maxVerticalDisplacement;
	public float minHorizontalDisplacement,maxHorizontalDisplacement;

	public override Building Apply(ref Building toReturn,WorldGenv2 wg,float[] path,float t,Building previous){
		toReturn=base.Apply(ref toReturn,wg,path,t,previous);
		toReturn.distance+=wg.RandomRange(minHorizontalDisplacement,maxHorizontalDisplacement)*(Random.value<0.5f?-1:1);
		toReturn.height+=wg.RandomRange(minVerticalDisplacement,maxVerticalDisplacement);
		//Rotate parallel to path direction
		//toReturn.yRot=90-toReturn.angle;
		return toReturn;
	}
}
