#include "stdafx.h"
#include "FBXMatrix.h"

LiliumFbx::FBXMatrix LiliumFbx::ConvertMatrix(FbxAMatrix& m)
{
	FBXMatrix o;
	o.M11 = (float)m[0][0];
	o.M12 = (float)m[0][1];
	o.M13 = (float)m[0][2];
	o.M14 = (float)m[0][3];

	o.M21 = (float)m[1][0];
	o.M22 = (float)m[1][1];
	o.M23 = (float)m[1][2];
	o.M24 = (float)m[1][3];

	o.M31 = (float)m[2][0];
	o.M32 = (float)m[2][1];
	o.M33 = (float)m[2][2];
	o.M34 = (float)m[2][3];

	o.M41 = (float)m[3][0];
	o.M42 = (float)m[3][1];
	o.M43 = (float)m[3][2];
	o.M44 = (float)m[3][3];
	return o;
}
