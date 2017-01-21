using UnityEngine;
using System.Collections;

public class PathSectionGenerator:MonoBehaviour{
	public float weight=1f,difficulty=1f;
	public int[] availableTypes;
	//public float minimumJumpLength,maximumJumpLength;

	public virtual Building Apply(ref Building toReturn,WorldGenv2 wg,float[] path,float t,Building previousBuilding){
		//toReturn.typeID=availableTypes[wg.RandomRange(0,availableTypes.Length)];
		toReturn.yRot=toReturn.angle;
		return toReturn;
	}
}