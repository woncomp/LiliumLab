// stdafx.cpp : ֻ������׼�����ļ���Դ�ļ�
// LiliumFbx.pch ����ΪԤ����ͷ
// stdafx.obj ������Ԥ����������Ϣ

#include "stdafx.h"

#include "Windows.h"

#pragma unmanaged
void __FbxSDK_printf(const char* szFormat, ...)
{
	char szBuff[1024];
	va_list arg;
	va_start(arg, szFormat);
	_vsnprintf(szBuff, sizeof(szBuff), szFormat, arg);
	va_end(arg);

	OutputDebugStringA(szBuff);
}