#pragma once

namespace LiliumFbx {
	public value struct FBXMatrix
	{
	public:
		float M11;
		float M12;
		float M13;
		float M14;
		float M21;
		float M22;
		float M23;
		float M24;
		float M31;
		float M32;
		float M33;
		float M34;
		float M41;
		float M42;
		float M43;
		float M44;
	};

	FBXMatrix ConvertMatrix(FbxAMatrix& pMatrix);
}