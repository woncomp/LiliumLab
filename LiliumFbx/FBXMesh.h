#pragma once

#include "FBXVertex.h"
#include "FBXBone.h"

using namespace System::Collections::Generic;

namespace LiliumFbx {
	public ref class FBXMesh
	{
	internal:
		FBXMesh(FbxMesh* p);

	public:
		void ProcessVertices();

	public:
		List<FBXVertex>^ Vertices;
		List<unsigned int>^ Indices;
		List<FBXBone^>^ Bones;

	internal:
		FBXVector3 ReadNormal(int inCtrlPointIndex, int inVertexCounter);
		FBXVector3 ReadTangent(int inCtrlPointIndex, int inVertexCounter);
		FBXVector3 ReadUV(int ctrlPointIndex, int textureUVIndex);

	private:
		FbxMesh* mNativeObject;
		FbxGeometryElementNormal* mElementNormal;
		FbxGeometryElementTangent* mElementTangent;
		FbxGeometryElementUV* mElementUV;
	};

}