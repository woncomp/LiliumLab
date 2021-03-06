#include "stdafx.h"
#include "FBXVector3.h"


namespace LiliumFbx {


	FBXVector3 ConvertVector(FbxVector4& v)
	{
		FBXVector3 output;
		output.X = (float)v.mData[0];
		output.Y = (float)v.mData[1];
		output.Z = (float)v.mData[2];
		return output;
	}

	FBXVector3 ConvertUV(FbxVector2& v)
	{
		FBXVector3 output;
		output.X = (float)v.mData[0];
		output.Y = 1-(float)v.mData[1];
		return output;
	}
}