#pragma once

#include "FBXMatrix.h"

using namespace System;

namespace LiliumFbx {

	public ref class FBXBone
	{
	public:
		String^ NodeName;
		FBXMatrix OffsetMatrix;
	};

}