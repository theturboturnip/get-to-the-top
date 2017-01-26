using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class BackTextureImageEffect : MonoBehaviour {
	public Shader shader;

	/*public Texture backTexture{
		get{
			return backTexture;
		}
		set{
			material.SetTexture("_BackTex", value);
			backTexture=value;
		}
	}
	public float depthCutoff{
		get{
			return depthCutoff;
		}
		set{
			material.SetFloat("_DepthCutoff", value);
			depthCutoff=value;
		}
	}*/
	public Texture backTexture;
	//public float depthCutoff;

	Material m_Material;

	void Start(){
			// Disable if we don't support image effects
		if (!SystemInfo.supportsImageEffects){
			enabled = false;
			return;
		}

		// Disable the image effect if the shader can't
		// run on the users graphics card
		if (!shader || !shader.isSupported)
			enabled = false;
	}

	protected Material material{
		get{
			if (m_Material == null){
				m_Material = new Material(shader);
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

	[ImageEffectOpaque]
	void OnRenderImage(RenderTexture src,RenderTexture dest){
		material.SetTexture("_BackTex",backTexture);
		//material.SetFloat("_DepthCutoff",depthCutoff);
		Graphics.Blit(src,dest,material);
	}
}
