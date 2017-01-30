using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*class EdgeComparison : IComparer{
	int IComparer.Compare(System.Object x,System.Object y){
		int[] edgeX=(int[])x,edgeY=(int[])y;
		if (edgeX[2]>edgeY[2]) return 1;
		if (edgeX[2]<edgeY[2]) return -1;
		if (edgeX[3]>edgeY[3]) return 1;
		if (edgeX[3]<edgeY[3]) return -1;
		return 0;
	}
}*/

public static class BuildingMeshGenLib {

	public static Mesh GenerateBuildingMesh(Mesh shape_,Vector3 bounds,bool generateUVs,Vector2 sideUVOffsetPerUnit,Vector2 roofUVOffsetPerUnit,bool flatShade=true,bool isTopRelative=false ){
		List<int[]> edges;
		Mesh shape=PrepareBuildingShape(shape_,bounds,out edges,generateUVs,sideUVOffsetPerUnit,roofUVOffsetPerUnit,flatShade);
		//2. Connect edges in triangles
		List<int> triangleList= new List<int>();
		int endOffset=shape.vertexCount;
		foreach(int[] edge in edges){
			if (edge[2]==-1) continue;				
			//We don't need to worry about winding order, because all our normals are correct 
			int[] connectingTriangles=new int[]{edge[1]+endOffset,edge[0]+endOffset,edge[0],edge[0],edge[1],edge[1]+endOffset};
			triangleList.AddRange(connectingTriangles);
		}
		//3. Fill vertex list while adding UVs
		List<Vector3> vertices=new List<Vector3>(),normals=new List<Vector3>();
		List<Vector2> uvs= new List<Vector2>();
		List<Vector3> oldUVs=new List<Vector3>();
		shape.GetUVs(0,oldUVs);
		Vector3 vertexOffset=Vector3.zero;
		if (isTopRelative) vertexOffset=-Vector3.up*bounds.y;
		for(int v=0;v<shape.vertexCount*2;v++){
			if(v>=shape.vertexCount){
				if (isTopRelative)
					vertexOffset=Vector3.zero;
				else
					vertexOffset=Vector3.up*bounds.y;
			}
			vertices.Add(shape.vertices[v%shape.vertexCount]+vertexOffset);
			normals.Add(shape.normals[v%shape.vertexCount]);
			if(generateUVs){
				if(sideUVOffsetPerUnit.y>0)
					uvs.Add(new Vector2(shape.uv[v%shape.vertexCount].x,(int)(vertexOffset.y*sideUVOffsetPerUnit.y)));
				else
					uvs.Add(new Vector2(shape.uv[v%shape.vertexCount].x,(vertexOffset==Vector3.zero)?0:1));
			}
		}

		//Generate roof 
		List<int> secondaryMeshTriangles=new List<int>();
			Mesh roof=GenerateRoofMesh(shape_,bounds,roofUVOffsetPerUnit);
			vertexOffset=Vector3.zero;
			if (isTopRelative) vertexOffset=-Vector3.up*bounds.y;
			for(int v=0;v<roof.vertexCount;v++){
				vertices.Add(roof.vertices[v]+vertexOffset);
				normals.Add(Vector3.up);
				if(generateUVs)
					uvs.Add(roof.uv[v]);
			}
			foreach(int t in roof.triangles)
				secondaryMeshTriangles.Add(t+shape.vertexCount*2);

		Mesh m=new Mesh();
		m.SetVertices(vertices);
		if(generateUVs) m.SetUVs(0,uvs);
			m.subMeshCount=2;
			m.SetTriangles(triangleList,0);
			m.SetTriangles(secondaryMeshTriangles,1);

		//if(window!=null)
		//	m.SetTriangles(secondaryMeshTriangles,1);
		m.SetNormals(normals);
		m.RecalculateBounds();
		return m;
	}

	static Mesh GenerateRoofMesh(Mesh shape_,Vector3 bounds,Vector2 roofUVOffsetPerUnit){
		Vector3 relativeScaling=new Vector3(bounds.x/shape_.bounds.size.x,0,bounds.z/shape_.bounds.size.z);
		Mesh roof=new Mesh();
		List<Vector3> resizedVerts=new List<Vector3>();
		foreach(Vector3 v in shape_.vertices)
			resizedVerts.Add(Vector3.Scale(relativeScaling,v)+Vector3.up*bounds.y);
		roof.SetVertices(resizedVerts);
		List<Vector2> uvs=new List<Vector2>();
		float roofX,roofY;
		foreach(Vector3 v in resizedVerts){
			roofX=Mathf.Round(roofUVOffsetPerUnit.x*v.x);
			roofY=Mathf.Round(roofUVOffsetPerUnit.y*v.z);
			uvs.Add(new Vector2(roofX,roofY));
		}
		roof.uv=uvs.ToArray();
		roof.triangles=shape_.triangles;
		return roof;
	}

	static Mesh PrepareBuildingShape(Mesh shape_,Vector3 bounds,out List<int[]> edges,bool generateUVs,Vector2 sideUVOffsetPerUnit,Vector2 roofUVOffsetPerUnit,bool flatShade){
		Vector3 relativeScaling=new Vector3(bounds.x/shape_.bounds.size.x,0,bounds.z/shape_.bounds.size.z);
		Mesh shape=new Mesh();
		//1. Identify edges
		edges=IdentifyShapeEdges(shape_);
		List<Vector3> resizedVerts=new List<Vector3>();
		if(flatShade){
			//generate new set of triangles
			List<int[]> newEdges=new List<int[]>();
			
			foreach(int[] edge in edges){
				if(edge[2]>-1){
					newEdges.Add(new int[]{resizedVerts.Count,resizedVerts.Count+1,resizedVerts.Count,resizedVerts.Count+1});
					resizedVerts.Add(Vector3.Scale(relativeScaling,shape_.vertices[edge[0]]));
					resizedVerts.Add(Vector3.Scale(relativeScaling,shape_.vertices[edge[1]]));
				}
			}
			edges=newEdges;
			edges=SortEdges(edges);

			for(int e=0;e<edges.Count;e++){
				//Vertex normal is -(sum of edges) = sum of -edges where edges go away from point 
				//So sum all edges that go towards point (A-B=BA)
				if(e<edges.Count-1){
					if (edges[e][2]==edges[e+1][2]&&edges[e][3]==edges[e+1][3]){
						edges[e][2]=-1;
						edges[e+1][2]=-1;
					}
				}
			}
		}else{
			foreach(Vector3 v in shape_.vertices)
				resizedVerts.Add(Vector3.Scale(relativeScaling,v));
		}
		shape.SetVertices(resizedVerts);

		
		//Calculate "outward normal" for each vertex 
		Vector3[] baseOutNormals;
		if(flatShade)
			baseOutNormals=new Vector3[shape_.vertexCount*2+1];
		else
			baseOutNormals=new Vector3[shape_.vertexCount+1];
		Populate<Vector3>(baseOutNormals,Vector3.zero);
		Vector3 v0,v1,centre=Vector3.zero;

		foreach(Vector3 point in resizedVerts)
			centre=(centre+point);
		centre/=shape_.vertexCount;

		//Generate 'edge normals' for all outside edges
		Vector3[] edgeNormals=new Vector3[edges.Count];
		Populate<Vector3>(edgeNormals,Vector3.zero);
		for(int e=0;e<edgeNormals.Length;e++){
			if (edges[e][2]<0) continue;
			v0=resizedVerts[edges[e][0]];
			v1=resizedVerts[edges[e][1]];
			edgeNormals[e]=SwapXY(v1-v0);
			//Assume centre is 0,0
			if(Vector3.Dot(edgeNormals[e],centre-(v0+v1)/2)>0)
				edgeNormals[e]=-edgeNormals[e];
			baseOutNormals[edges[e][0]]+=edgeNormals[e];
			baseOutNormals[edges[e][1]]+=edgeNormals[e];
		}
		//Normalize outward normals
		for(int v=0;v<shape_.vertexCount;v++)
			baseOutNormals[v]=baseOutNormals[v].normalized;

		resizedVerts.Add(resizedVerts[0]);
		baseOutNormals[resizedVerts.Count-1]=baseOutNormals[0];
		shape.SetVertices(resizedVerts);
		shape.normals=baseOutNormals;
		shape.triangles=new int[]{};
		//shape.triangles=shape_.triangles;
		if (generateUVs){
			List<Vector3> calculatedUvs=new List<Vector3>();
			calculatedUvs.Add(new Vector3(0,roofUVOffsetPerUnit.x*shape.vertices[0].x/shape.bounds.size.x,roofUVOffsetPerUnit.y*shape.vertices[0].z/shape.bounds.size.z));//The uv coords for vertex 0 are always zero
			//Generate shape uvs by tracing from vertex 0
			//Use distace between adjacent verts to calculate uv x
			//uv yz is used to store roof uv data
			float uvx,roofX,roofY;
			for(int v=1;v<shape.vertexCount;v++){
				uvx=SuperMaths.RoundTo((shape.vertices[v]-shape.vertices[v-1]).magnitude*sideUVOffsetPerUnit.x+calculatedUvs[v-1].x,1);
				roofX=roofUVOffsetPerUnit.x*shape.vertices[v].x;
				roofY=roofUVOffsetPerUnit.y*shape.vertices[v].z;
				calculatedUvs.Add(new Vector3(uvx,roofX,roofY));
			}
			shape.SetUVs(0,calculatedUvs);
		}

		foreach(int[] edge in edges){
			if(edge[1]==0)
				edge[1]=shape.vertexCount-1;
		}
		return shape;
	}
	
	// Update is called once per frame
	public static List<int[]> IdentifyShapeEdges (Mesh shape,int vStart=0,int vEnd=-1) {
		//Identify edges
		List<int[]> edges=new List<int[]>();
		//int[] basicEdge=new int[4]{-1,-1,-1,-1};
		int t,e;
		if (vEnd<0)
			vEnd=shape.vertexCount;
		for(t=0;t<shape.triangles.Length;t+=3){
			if (IsOutside(shape.triangles[t],vStart,vEnd)) continue;
			if (IsOutside(shape.triangles[t+1],vStart,vEnd)) continue;
			if (IsOutside(shape.triangles[t+2],vStart,vEnd)) continue;
			edges.Add(Organize(new int[4]{shape.triangles[t],shape.triangles[t+1],0,0}));
			edges.Add(Organize(new int[4]{shape.triangles[t+1],shape.triangles[t+2],0,0}));
			edges.Add(Organize(new int[4]{shape.triangles[t+2],shape.triangles[t],0,0}));
		}
		//foreach(int[] edge in edges) Organize(edge);

		edges=SortEdges(edges);

		for(e=0;e<edges.Count;e++){
			//Vertex normal is -(sum of edges) = sum of -edges where edges go away from point 
			//So sum all edges that go towards point (A-B=BA)
			if(e<edges.Count-1){
				if (edges[e][2]==edges[e+1][2]&&edges[e][3]==edges[e+1][3]){
					edges[e][2]=-1;
					edges[e+1][2]=-1;
				}
			}
		}
		return edges;
	}

	static bool IsOutside(int i,int start,int end){
		return (i<start)||(i>end);
	}

	static List<int[]> SortEdges(List<int[]> toSort){
		for(int i=0;i<toSort.Count-1;i++){
			if (EdgeComp(toSort[i+1],toSort[i])<0){
				int[] storage=toSort[i];
				toSort[i]=toSort[i+1];
				toSort[i+1]=storage;
				return SortEdges(toSort);
			}
		}
		return toSort;
	}

	static int EdgeComp(int[] edgeX,int[] edgeY){
		if (edgeX[2]>edgeY[2]) return 1;
		if (edgeX[2]<edgeY[2]) return -1;
		if (edgeX[3]>edgeY[3]) return 1;
		if (edgeX[3]<edgeY[3]) return -1;
		return 0;
	}

	static int[] Organize(int[] toOrg){
		if (toOrg[0]>toOrg[1]){
			toOrg[3]=toOrg[0];
			toOrg[2]=toOrg[1];
		}else{
			toOrg[2]=toOrg[0];
			toOrg[3]=toOrg[1];
		}
		return toOrg;
	}

	static void Populate<T>(T[] arr,T value){
		for (int i=0;i<arr.Length;i++)
			arr[i]=value;
	}

	static Vector3 SwapXY(Vector3 input){
		return new Vector3(input.z,-input.y,input.x);
	}
}
