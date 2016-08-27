#pragma once

#include "FBXNode.h"

using namespace System;

namespace LiliumFbx {

	public ref class FBXScene
	{
	public:
		static FBXScene^ Load(String^ filePath);

	private:
		FBXScene(FbxScene* p);

	public:
		!FBXScene();
		~FBXScene();

	public:
		void Test();

		FBXNode^ GetRootNode();

		bool HasCurve(FBXNode^ node);
		__int64 GetAnimStart();
		__int64 GetAnimEnd();

	private:
		FbxScene* mNativeObject;
		FbxAnimStack* mAnimStack;
		FbxAnimLayer* mAnimLayer;
		FbxTakeInfo* mTakeInfo;
	};
}
