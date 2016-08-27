#pragma once

namespace LiliumFbx {
	public value struct FBXVector3
	{
	public:
		float X;
		float Y;
		float Z;
	};

	FBXVector3 ConvertVector(FbxVector4& v);
}