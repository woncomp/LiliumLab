// stdafx.h : 标准系统包含文件的包含文件，
// 或是经常使用但不常更改的
// 特定于项目的包含文件

#pragma once


#include "fbxsdk.h"

#ifdef FBXSDK_printf
#undef FBXSDK_printf
#endif
#define FBXSDK_printf __FbxSDK_printf

void __FbxSDK_printf(const char* szFormat, ...);