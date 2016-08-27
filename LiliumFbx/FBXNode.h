#pragma once

#include "FBXMatrix.h"
#include "FBXMesh.h"

using namespace System;

namespace LiliumFbx {
	public ref class FBXNode
	{
	internal:
		FBXNode(FbxNode* p);

	public:
		~FBXNode();
		!FBXNode();

		int GetChildCount();
		FBXNode^ GetChild(int index);

		String^ GetName();

		FBXMatrix GetLocalTransform();
		FBXMatrix EvaluateLocalTransform(__int64 frameIndex);

		FBXMesh^ GetMesh();

	internal:
		FbxNode* mNativeObject;
	};

}