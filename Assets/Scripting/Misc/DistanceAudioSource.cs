using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class DistanceAudioSource : MonoBehaviour {
	AudioSource source;
	AudioListener listener;
	public float maxDistance;
	float originalVolume;
	// Use this for initialization
	void Start () {
		source=GetComponent<AudioSource>();
		originalVolume=source.volume;
		listener=(FindObjectsOfType(typeof(AudioListener)) as AudioListener[])[0];
	}
	
	// Update is called once per frame
	void Update () {
		float distance=Vector3.Distance(transform.position,listener.transform.position);
		source.volume=Mathf.Clamp01(1-(distance/maxDistance))*originalVolume;
	}
}
