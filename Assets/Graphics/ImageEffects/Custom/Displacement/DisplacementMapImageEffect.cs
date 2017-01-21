using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class DisplacementMapImageEffect : MonoBehaviour {
	public Shader displacementShader;

	/*public Texture displacementTexture{
		get{
			return displacementTexture;
		}
		set{
			material.SetTexture("_Displacement",value);
			displacementTexture=value;
		}
	}
	public float displacementAmount{
		get{
			return displacementAmount;
		}
		set{
			material.SetFloat("_DisplacementAmount",value);
			displacementAmount=value;
		}
	}*/
	public Texture displacementTexture;
	public float displacementAmount;

	Material m_Material;


	void Start(){
			// Disable if we don't support image effects
		if (!SystemInfo.supportsImageEffects){
			enabled = false;
			return;
		}

		// Disable the image effect if the displacementShader can't
		// run on the users graphics card
		if (!displacementShader || !displacementShader.isSupported)
			enabled = false;
	}

	protected Material material{
		get{
			if (m_Material == null){
				m_Material = new Material(displacementShader);
				m_Material.hideFlags = HideFlags.HideAndDontSave;
			}
			return m_Material;
		}
	}


	protected virtual void OnDisable(){
		if (m_Material){
			DestroyImmediate(m_Material);
		}
	}

	void OnRenderImage(RenderTexture src,RenderTexture dest){
		material.SetTexture("_Displacement",displacementTexture);
		material.SetFloat("_DisplacementAmount",displacementAmount);
		Graphics.Blit(src,dest,material);
	}
}
