// 这是主 DLL 文件。

#include "stdafx.h"

#include "FBXScene.h"

#ifdef IOS_REF
#undef  IOS_REF
#define IOS_REF (*(pManager->GetIOSettings()))
#endif

using namespace System::Runtime::InteropServices;


static FbxManager* pManager = NULL;

static void _InitializeSdkObjects()
{
	if (pManager != NULL) return;
	//The first thing to do is to create the FBX Manager which is the object allocator for almost all the classes in the SDK
	pManager = FbxManager::Create();
	if (!pManager)
	{
		FBXSDK_printf("Error: Unable to create FBX Manager!\n");
		exit(1);
	}
	else FBXSDK_printf("Autodesk FBX SDK version %s\n", pManager->GetVersion());

	//Create an IOSettings object. This object holds all import/export settings.
	FbxIOSettings* ios = FbxIOSettings::Create(pManager, IOSROOT);
	pManager->SetIOSettings(ios);

	//Load plugins from the executable directory (optional)
	FbxString lPath = FbxGetApplicationDirectory();
	pManager->LoadPluginsDirectory(lPath.Buffer());
}

static bool LoadScene(FbxManager* pManager, FbxDocument* pScene, const char* pFilename)
{
	int lFileMajor, lFileMinor, lFileRevision;
	int lSDKMajor, lSDKMinor, lSDKRevision;
	//int lFileFormat = -1;
	int i, lAnimStackCount;
	bool lStatus;
	char lPassword[1024];

	// Get the file version number generate by the FBX SDK.
	FbxManager::GetFileFormatVersion(lSDKMajor, lSDKMinor, lSDKRevision);

	// Create an importer.
	FbxImporter* lImporter = FbxImporter::Create(pManager, "");

	// Initialize the importer by providing a filename.
	const bool lImportStatus = lImporter->Initialize(pFilename, -1, pManager->GetIOSettings());
	lImporter->GetFileVersion(lFileMajor, lFileMinor, lFileRevision);

	if (!lImportStatus)
	{
		FbxString error = lImporter->GetStatus().GetErrorString();
		FBXSDK_printf("Call to LiliumFbx::Initialize() failed.\n");
		FBXSDK_printf("Error returned: %s\n\n", error.Buffer());

		if (lImporter->GetStatus().GetCode() == FbxStatus::eInvalidFileVersion)
		{
			FBXSDK_printf("FBX file format version for this FBX SDK is %d.%d.%d\n", lSDKMajor, lSDKMinor, lSDKRevision);
			FBXSDK_printf("FBX file format version for file '%s' is %d.%d.%d\n\n", pFilename, lFileMajor, lFileMinor, lFileRevision);
		}

		return false;
	}

	FBXSDK_printf("FBX file format version for this FBX SDK is %d.%d.%d\n", lSDKMajor, lSDKMinor, lSDKRevision);

	if (lImporter->IsFBX())
	{
		FBXSDK_printf("FBX file format version for file '%s' is %d.%d.%d\n\n", pFilename, lFileMajor, lFileMinor, lFileRevision);

		// From this point, it is possible to access animation stack information without
		// the expense of loading the entire file.

		FBXSDK_printf("Animation Stack Information\n");

		lAnimStackCount = lImporter->GetAnimStackCount();

		FBXSDK_printf("    Number of Animation Stacks: %d\n", lAnimStackCount);
		FBXSDK_printf("    Current Animation Stack: \"%s\"\n", lImporter->GetActiveAnimStackName().Buffer());
		FBXSDK_printf("\n");

		for (i = 0; i < lAnimStackCount; i++)
		{
			FbxTakeInfo* lTakeInfo = lImporter->GetTakeInfo(i);

			FBXSDK_printf("    Animation Stack %d\n", i);
			FBXSDK_printf("         Name: \"%s\"\n", lTakeInfo->mName.Buffer());
			FBXSDK_printf("         Description: \"%s\"\n", lTakeInfo->mDescription.Buffer());

			// Change the value of the import name if the animation stack should be imported 
			// under a different name.
			FBXSDK_printf("         Import Name: \"%s\"\n", lTakeInfo->mImportName.Buffer());

			// Set the value of the import state to false if the animation stack should be not
			// be imported. 
			FBXSDK_printf("         Import State: %s\n", lTakeInfo->mSelect ? "true" : "false");
			FBXSDK_printf("\n");
		}

		// Set the import states. By default, the import states are always set to 
		// true. The code below shows how to change these states.
		IOS_REF.SetBoolProp(IMP_FBX_MATERIAL, true);
		IOS_REF.SetBoolProp(IMP_FBX_TEXTURE, true);
		IOS_REF.SetBoolProp(IMP_FBX_LINK, true);
		IOS_REF.SetBoolProp(IMP_FBX_SHAPE, true);
		IOS_REF.SetBoolProp(IMP_FBX_GOBO, true);
		IOS_REF.SetBoolProp(IMP_FBX_ANIMATION, true);
		IOS_REF.SetBoolProp(IMP_FBX_GLOBAL_SETTINGS, true);
	}

	// Import the scene.
	lStatus = lImporter->Import(pScene);

	if (lStatus == false && lImporter->GetStatus().GetCode() == FbxStatus::ePasswordError)
	{
		FBXSDK_printf("Please enter password: ");

		lPassword[0] = '\0';

		FBXSDK_CRT_SECURE_NO_WARNING_BEGIN
			scanf("%s", lPassword);
		FBXSDK_CRT_SECURE_NO_WARNING_END

			FbxString lString(lPassword);

		IOS_REF.SetStringProp(IMP_FBX_PASSWORD, lString);
		IOS_REF.SetBoolProp(IMP_FBX_PASSWORD_ENABLE, true);

		lStatus = lImporter->Import(pScene);

		if (lStatus == false && lImporter->GetStatus().GetCode() == FbxStatus::ePasswordError)
		{
			FBXSDK_printf("\nPassword is wrong, import aborted.\n");
		}
	}

	// Destroy the importer.
	lImporter->Destroy();
	return lStatus;
}

namespace LiliumFbx {
	FBXScene^ FBXScene::Load(String^ filePath)
	{
		FbxScene* lScene = NULL;
		bool lResult;

		// Prepare the FBX SDK.
		_InitializeSdkObjects();

		//Create an FBX scene. This object holds most objects imported/exported from/to files.
		lScene = FbxScene::Create(pManager, "My Scene");
		if (!lScene)
		{
			FBXSDK_printf("Error: Unable to create FBX scene!\n");
			exit(1);
		}
		// Load the scene.

		if (String::IsNullOrEmpty(filePath))
		{
			lResult = false;
			FBXSDK_printf("\n\nUsage: ImportScene <FBX file name>\n\n");
			return nullptr;
		}

		IntPtr ptrToNativeString = Marshal::StringToHGlobalAnsi(filePath);
		char* pszFilePath = static_cast<char*>(ptrToNativeString.ToPointer());

		FBXSDK_printf("\n\nFile: %s\n\n", pszFilePath);
		lResult = LoadScene(pManager, lScene, pszFilePath);


		if (lResult == false)
		{
			FBXSDK_printf("\n\nAn error occurred while loading the scene...");
			return nullptr;
		}
		
		FbxAxisSystem axisSystem(FbxAxisSystem::eYAxis, FbxAxisSystem::eParityOdd, FbxAxisSystem::eRightHanded);
		axisSystem.ConvertScene(lScene);

		return gcnew FBXScene(lScene);
	}

	FBXScene::FBXScene(FbxScene* p)
		:mNativeObject(p)
		, mAnimStack(NULL)
		, mAnimLayer(NULL)
		, mTakeInfo(NULL)
	{
		auto count = mNativeObject->GetSrcObjectCount<FbxAnimStack>();
		if (count > 0) mAnimStack = mNativeObject->GetSrcObject<FbxAnimStack>(0);
		if (count > 0) mAnimStack->GetMemberCount<FbxAnimLayer>();
		if (count > 0) mAnimLayer = mAnimStack->GetMember<FbxAnimLayer>(0);
		if (mAnimStack) mTakeInfo = mNativeObject->GetTakeInfo(mAnimStack->GetName());
	}

	FBXScene::!FBXScene()
	{
		if (mNativeObject != NULL)
		{
			mNativeObject->Destroy();
			mNativeObject = NULL;
		}
	}

	FBXScene::~FBXScene()
	{
	}

	void FBXScene::Test()
	{
		auto pRootNode = mNativeObject->GetRootNode();
		auto pName = pRootNode->GetName();
		FBXSDK_printf(pName);
	}

	FBXNode^ FBXScene::GetRootNode()
	{
		return gcnew FBXNode(mNativeObject->GetRootNode());
	}

	__int64 FBXScene::GetAnimStart()
	{
		if (!mTakeInfo) return 0;
		auto start = mTakeInfo->mLocalTimeSpan.GetStart();
		return (__int64)start.GetFrameCount(FbxTime::eFrames30);
	}

	__int64 FBXScene::GetAnimEnd()
	{
		if (!mTakeInfo) return 0;
		auto stop = mTakeInfo->mLocalTimeSpan.GetStop();
		return (__int64)stop.GetFrameCount(FbxTime::eFrames30);
	}

	bool FBXScene::HasCurve(FBXNode^ node)
	{
		if (!mAnimLayer) return false;
		auto pNode = node->mNativeObject;

		FbxAnimCurve* lAnimCurve = NULL;
		lAnimCurve = pNode->LclTranslation.GetCurve(mAnimLayer, FBXSDK_CURVENODE_COMPONENT_X);
		if (lAnimCurve) return true;
		lAnimCurve = pNode->LclTranslation.GetCurve(mAnimLayer, FBXSDK_CURVENODE_COMPONENT_Y);
		if (lAnimCurve) return true;
		lAnimCurve = pNode->LclTranslation.GetCurve(mAnimLayer, FBXSDK_CURVENODE_COMPONENT_Z);
		if (lAnimCurve) return true;

		lAnimCurve = pNode->LclRotation.GetCurve(mAnimLayer, FBXSDK_CURVENODE_COMPONENT_X);
		if (lAnimCurve) return true;
		lAnimCurve = pNode->LclRotation.GetCurve(mAnimLayer, FBXSDK_CURVENODE_COMPONENT_Y);
		if (lAnimCurve) return true;
		lAnimCurve = pNode->LclRotation.GetCurve(mAnimLayer, FBXSDK_CURVENODE_COMPONENT_Z);
		if (lAnimCurve) return true;

		lAnimCurve = pNode->LclScaling.GetCurve(mAnimLayer, FBXSDK_CURVENODE_COMPONENT_X);
		if (lAnimCurve) return true;
		lAnimCurve = pNode->LclScaling.GetCurve(mAnimLayer, FBXSDK_CURVENODE_COMPONENT_Y);
		if (lAnimCurve) return true;
		lAnimCurve = pNode->LclScaling.GetCurve(mAnimLayer, FBXSDK_CURVENODE_COMPONENT_Z);
		if (lAnimCurve) return true;

		return false;
	}


}