using UnityEngine;
using System.Collections;

public class PlayerTrigger : MonoBehaviour {
	public Bounds activateBox;

	Transform _player;
	bool _isInside=false;
	// Use this for initialization
	void Start () {
		_player=GameObject.FindWithTag("Player").transform;
	}
	
	// Update is called once per frame
	public virtual void Update () {
		if (_player==null) return;
		Vector3 localPos=transform.InverseTransformPoint(_player.position);
		if (activateBox.Contains(localPos)){
			if (_isInside) OnPlayerStay(_player);
			if (!_isInside){
				_isInside=true;
				OnPlayerEnter(_player);
			}
		}else if (_isInside){
			_isInside=false;
			OnPlayerLeave(_player);
		}else{
			OnPlayerStayOut(_player);
		}
	}

	public virtual void OnPlayerEnter(Transform player){}

	public virtual void OnPlayerStay(Transform player){}

	public virtual void OnPlayerLeave(Transform player){}

	public virtual void OnPlayerStayOut(Transform player){}

	public virtual Color GetGizmoColor(){
		return Color.green;
	} 

	void OnDrawGizmos(){
		Gizmos.color=GetGizmoColor();
		Gizmos.matrix=transform.localToWorldMatrix;
		Gizmos.DrawWireCube(activateBox.center,activateBox.size);
	}
}
