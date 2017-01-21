using UnityEngine;
using UnityEditor;
using System.Collections;

public class ModelImportMaterialFix : AssetPostprocessor {

	public void OnPreprocessModel() { 
		ModelImporter modelImporter = (ModelImporter) assetImporter; 
		//modelImporter.globalScale = 1; 
		modelImporter.importMaterials = false;
	} 
}
