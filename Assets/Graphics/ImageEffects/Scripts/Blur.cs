using System;
using UnityEngine;

namespace UnityStandardAssets.ImageEffects
{
	[ExecuteInEditMode]
	[AddComponentMenu("Image Effects/Blur/Blur")]
	public class Blur : MonoBehaviour
	{
		/// Blur iterations - larger number means more blur.
		[Range(0,10)]
		public int iterations = 3;

		/// Blur spread for each iteration. Lower values
		/// give better looking blur, but require more iterations to
		/// get large blurs. Value is usually between 0.5 and 1.0.
		[Range(0.0f,1.0f)]
		public float blurSpread = 0.6f;

		public float blurTime=0.1f;
		public bool blurring=false;
		float blurStart=-1,blurTarget=0;

		// --------------------------------------------------------
		// The blur iteration shader.
		// Basically it just takes 4 texture samples and averages them.
		// By applying it repeatedly and spreading out sample locations
		// we get a Gaussian blur approximation.

		public Shader blurShader = null;

		static Material m_Material = null;
		protected Material material {
			get {
				if (m_Material == null) {
					m_Material = new Material(blurShader);
					m_Material.hideFlags = HideFlags.DontSave;
				}
				return m_Material;
			}
		}

		protected void OnDisable() {
			if ( m_Material ) {
				DestroyImmediate( m_Material );
			}
		}

		// --------------------------------------------------------

		protected void Start()
		{
			// Disable if we don't support image effects
			if (!SystemInfo.supportsImageEffects) {
				enabled = false;
				return;
			}
			// Disable if the shader can't run on the users graphics card
			if (!blurShader || !material.shader.isSupported) {
				enabled = false;
				return;
			}
		}

		public void StartBlur(){
			blurStart=Time.unscaledTime;
			blurTarget=1;
			blurring=true;
		}

		public void EndBlur(){
			blurStart=Time.unscaledTime;
			blurTarget=0;
			blurring=false;
		}

		// Performs one blur iteration.
		public void FourTapCone (RenderTexture source, RenderTexture dest, int iteration, float actualBlurSpread)
		{
			float off = 0.5f + iteration*blurSpread;
			Graphics.BlitMultiTap (source, dest, material,
								   new Vector2(-off, -off),
								   new Vector2(-off,  off),
								   new Vector2( off,  off),
								   new Vector2( off, -off)
				);
		}

		// Downsamples the texture to a quarter resolution.
		private void DownSample4x (RenderTexture source, RenderTexture dest)
		{
			float off = 1.0f;
			Graphics.BlitMultiTap (source, dest, material,
								   new Vector2(-off, -off),
								   new Vector2(-off,  off),
								   new Vector2( off,  off),
								   new Vector2( off, -off)
				);
		}

		// Called by the camera to apply the image effect
		void OnRenderImage (RenderTexture source, RenderTexture destination) {
			Graphics.Blit(source,destination);
			if (blurStart<0) return;
			float blurProg=Mathf.Clamp01((Time.unscaledTime-blurStart)/blurTime);
			if (!blurring)
				blurProg=1-blurProg;
			if (!blurring&&blurProg==0) return;

			int rtW = source.width/4;
			int rtH = source.height/4;
			RenderTexture buffer = RenderTexture.GetTemporary(rtW, rtH, 0);

			// Copy source to the 4x4 smaller texture.
			DownSample4x (source, buffer);

			// Blur the small texture
			int actualIters=Mathf.RoundToInt(iterations*blurProg);
			float actualBlurSpread=blurSpread*blurProg;
			for(int i = 0; i < actualIters; i++)
			{
				RenderTexture buffer2 = RenderTexture.GetTemporary(rtW, rtH, 0);
				FourTapCone (buffer, buffer2, i, actualBlurSpread);
				RenderTexture.ReleaseTemporary(buffer);
				buffer = buffer2;
			}
			Graphics.Blit(buffer, destination);

			RenderTexture.ReleaseTemporary(buffer);
		}
	}
}
