// stdafx.h : ��׼ϵͳ�����ļ��İ����ļ���
// ���Ǿ���ʹ�õ��������ĵ�
// �ض�����Ŀ�İ����ļ�

#pragma once


#include "fbxsdk.h"

#ifdef FBXSDK_printf
#undef FBXSDK_printf
#endif
#define FBXSDK_printf __FbxSDK_printf

void __FbxSDK_printf(const char* szFormat, ...);