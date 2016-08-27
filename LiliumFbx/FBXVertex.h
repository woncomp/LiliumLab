#pragma once

#include "FBXVector3.h"

namespace LiliumFbx {

	public value struct FBXBoneWeight
	{
	public:
		int Index;
		float Weight;
	};

	public value struct FBXVertex
	{
	public:
		FBXVector3 Position;
		FBXVector3 Normal;
		FBXVector3 Tangent;
		FBXVector3 TexCoord0;
		FBXBoneWeight Weight0;
		FBXBoneWeight Weight1;
		FBXBoneWeight Weight2;
		FBXBoneWeight Weight3;
	};
}