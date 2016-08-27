#include "stdafx.h"
#include "FBXNode.h"

namespace LiliumFbx {

	FBXNode::FBXNode(FbxNode* p)
		:mNativeObject(p)
	{

	}

	FBXNode::~FBXNode()
	{

	}

	FBXNode::!FBXNode()
	{

	}

	int FBXNode::GetChildCount()
	{
		return mNativeObject->GetChildCount();
	}

	FBXNode^ FBXNode::GetChild(int index)
	{
		return gcnew FBXNode(mNativeObject->GetChild(index));
	}

	FBXMatrix FBXNode::GetLocalTransform()
	{
		auto m = mNativeObject->EvaluateLocalTransform();
		return ConvertMatrix(m);
	}

	String^ FBXNode::GetName()
	{
		return gcnew String(mNativeObject->GetName());
	}

	FBXMesh^ FBXNode::GetMesh()
	{
		auto pMesh = mNativeObject->GetMesh();
		if (pMesh != NULL)
		{
			return gcnew FBXMesh(pMesh);
		}
		return nullptr;
	}

	FBXMatrix FBXNode::EvaluateLocalTransform(__int64 frameIndex)
	{
		FbxTime time;
		time.SetFrame(frameIndex, FbxTime::eFrames30);
		auto m = mNativeObject->EvaluateLocalTransform(time);
		return ConvertMatrix(m);
	}

}