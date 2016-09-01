#include "stdafx.h"
#include "FBXMesh.h"


namespace LiliumFbx {

	static void ProcessSkin(FbxMesh* currMesh, array<List<FBXBoneWeight>^>^ outputCtrlPointBoneWeights, List<FBXBone^>^ outputBones)
	{
		unsigned int numOfDeformers = currMesh->GetDeformerCount();

		// A deformer is a FBX thing, which contains some clusters
		// A cluster contains a link, which is basically a joint
		// Normally, there is only one deformer in a mesh
		for (unsigned int deformerIndex = 0; deformerIndex < numOfDeformers; ++deformerIndex)
		{
			// There are many types of deformers in Maya,
			// We are using only skins, so we see if this is a skin
			FbxSkin* currSkin = reinterpret_cast<FbxSkin*>(currMesh->GetDeformer(deformerIndex, FbxDeformer::eSkin));
			if (!currSkin)
			{
				continue;
			}

			unsigned int numOfClusters = currSkin->GetClusterCount();
			for (unsigned int clusterIndex = 0; clusterIndex < numOfClusters; ++clusterIndex)
			{
				FbxCluster* currCluster = currSkin->GetCluster(clusterIndex);
				auto currJointName = currCluster->GetLink()->GetName();
				FbxAMatrix transformMatrix;
				FbxAMatrix transformLinkMatrix;
				FbxAMatrix globalBindposeInverseMatrix;

				currCluster->GetTransformMatrix(transformMatrix);	// The transformation of the mesh at binding time
				currCluster->GetTransformLinkMatrix(transformLinkMatrix);	// The transformation of the cluster(joint) at binding time from joint space to world space
				globalBindposeInverseMatrix = transformLinkMatrix.Inverse() * transformMatrix;

				// Update the information in mSkeleton 
				FBXBone^ bone = gcnew FBXBone();
				bone->NodeName = gcnew String(currJointName);
				bone->OffsetMatrix = ConvertMatrix( globalBindposeInverseMatrix);

				// Associate each joint with the control points it affects
				unsigned int numOfIndices = currCluster->GetControlPointIndicesCount();
				auto ctrlPointIndices = currCluster->GetControlPointIndices();
				auto ctrlPointWeights = currCluster->GetControlPointWeights();
				for (unsigned int i = 0; i < numOfIndices; ++i)
				{
					auto ctrlPointIndex = ctrlPointIndices[i];
					auto ctrlPointWeight = ctrlPointWeights[i];

					if (outputCtrlPointBoneWeights[ctrlPointIndex] == nullptr) outputCtrlPointBoneWeights[ctrlPointIndex] = gcnew List<FBXBoneWeight>();
					auto boneWeights = outputCtrlPointBoneWeights[ctrlPointIndex];
					FBXBoneWeight w;
					w.Index = outputBones->Count;
					w.Weight = (float)ctrlPointWeight;
					boneWeights->Add(w);
				}
				outputBones->Add(bone);
			}
		}
	}

	FBXMesh::FBXMesh(FbxMesh* p)
		:mNativeObject(p)
	{
		Vertices = gcnew List<FBXVertex>();
		Indices = gcnew List<unsigned int>();
		Bones = gcnew List<FBXBone^>();
	}
	
	void FBXMesh::ProcessVertices()
	{
		auto ctrlPointsCount = mNativeObject->GetControlPointsCount();
		array<List<FBXBoneWeight>^>^ ctrlPointBoneWeights = gcnew array<List<FBXBoneWeight>^>(ctrlPointsCount);
		ProcessSkin(mNativeObject, ctrlPointBoneWeights, Bones);
		
		auto polygonCount = mNativeObject->GetPolygonCount();

		mElementNormal = mNativeObject->GetElementNormal(0);
		mElementTangent = mNativeObject->GetElementTangent(0);
		mElementUV = mNativeObject->GetElementUV(0);

		for (int polygonIndex = 0; polygonIndex < polygonCount; ++polygonIndex)
		{
			for (int positionInPolygon = 0; positionInPolygon < 3; ++positionInPolygon)
			{
				FBXVertex vertex;

				auto ctrlPointIndex = mNativeObject->GetPolygonVertex(polygonIndex, positionInPolygon);
				auto pCtrlPoint = mNativeObject->GetControlPointAt(ctrlPointIndex);
				auto vertexCounter = Vertices->Count;

				vertex.Position = ConvertVector(pCtrlPoint);
				if (mElementNormal != NULL) vertex.Normal = ReadNormal(ctrlPointIndex, vertexCounter);
				if (mElementTangent != NULL) vertex.Tangent = ReadTangent(ctrlPointIndex, vertexCounter);
				if (mElementUV != NULL) vertex.TexCoord0 = ReadUV(ctrlPointIndex, mNativeObject->GetTextureUVIndex(polygonIndex, positionInPolygon));
				
				auto boneWeightList = ctrlPointBoneWeights[ctrlPointIndex];
				if (boneWeightList != nullptr)
				{
					if (boneWeightList->Count > 0) vertex.Weight0 = boneWeightList[0];
					if (boneWeightList->Count > 1) vertex.Weight1 = boneWeightList[1];
					if (boneWeightList->Count > 2) vertex.Weight2 = boneWeightList[2];
					if (boneWeightList->Count > 3) vertex.Weight3 = boneWeightList[3];
				}

				Vertices->Add(vertex);
				Indices->Add(vertexCounter);
			}
		}
	}

	FBXVector3 FBXMesh::ReadNormal(int inCtrlPointIndex, int inVertexCounter)
	{
		auto pElem = mElementNormal;
		int index = 0;
		switch (pElem->GetMappingMode())
		{
		case FbxGeometryElement::eByControlPoint:
			switch (pElem->GetReferenceMode())
			{
			case FbxGeometryElement::eDirect:
				return ConvertVector(pElem->GetDirectArray().GetAt(inCtrlPointIndex));
			case FbxGeometryElement::eIndexToDirect:
				index = pElem->GetIndexArray().GetAt(inCtrlPointIndex);
				return ConvertVector(pElem->GetDirectArray().GetAt(index));
			default:
				throw std::exception("Invalid Reference");
			}
			break;

		case FbxGeometryElement::eByPolygonVertex:
			switch (pElem->GetReferenceMode())
			{
			case FbxGeometryElement::eDirect:
				return ConvertVector(pElem->GetDirectArray().GetAt(inVertexCounter));
			case FbxGeometryElement::eIndexToDirect:
				index = pElem->GetIndexArray().GetAt(inVertexCounter);
				return ConvertVector(pElem->GetDirectArray().GetAt(index));
			default:
				throw std::exception("Invalid Reference");
			}
			break;
		}
		return FBXVector3();
	}

	LiliumFbx::FBXVector3 FBXMesh::ReadTangent(int inCtrlPointIndex, int inVertexCounter)
	{
		auto pElem = mElementTangent;
		int index = 0;
		switch (pElem->GetMappingMode())
		{
		case FbxGeometryElement::eByControlPoint:
			switch (pElem->GetReferenceMode())
			{
			case FbxGeometryElement::eDirect:
				return ConvertVector(pElem->GetDirectArray().GetAt(inCtrlPointIndex));
			case FbxGeometryElement::eIndexToDirect:
				index = pElem->GetIndexArray().GetAt(inCtrlPointIndex);
				return ConvertVector(pElem->GetDirectArray().GetAt(index));
			default:
				throw std::exception("Invalid Reference");
			}
			break;

		case FbxGeometryElement::eByPolygonVertex:
			switch (pElem->GetReferenceMode())
			{
			case FbxGeometryElement::eDirect:
				return ConvertVector(pElem->GetDirectArray().GetAt(inVertexCounter));
			case FbxGeometryElement::eIndexToDirect:
				index = pElem->GetIndexArray().GetAt(inVertexCounter);
				return ConvertVector(pElem->GetDirectArray().GetAt(index));
			default:
				throw std::exception("Invalid Reference");
			}
			break;
		}
		return FBXVector3();
	}

	LiliumFbx::FBXVector3 FBXMesh::ReadUV(int ctrlPointIndex, int textureUVIndex)
	{
		auto pElem = mElementUV;

		switch (pElem->GetMappingMode())
		{
		case FbxGeometryElement::eByControlPoint:
		{
			switch (pElem->GetReferenceMode())
			{
			case FbxGeometryElement::eDirect:
			{
				return ConvertUV(pElem->GetDirectArray().GetAt(ctrlPointIndex));
			}
			break;

			case FbxGeometryElement::eIndexToDirect:
			{
				int id = pElem->GetIndexArray().GetAt(ctrlPointIndex);
				return ConvertUV(pElem->GetDirectArray().GetAt(id));
			}
			break;

			default:
				break;
			}
		}
		break;

		case FbxGeometryElement::eByPolygonVertex:
		{
			switch (pElem->GetReferenceMode())
			{
			case FbxGeometryElement::eDirect:
			case FbxGeometryElement::eIndexToDirect:
			{
				return ConvertUV(pElem->GetDirectArray().GetAt(textureUVIndex));
			}
			break;

			default:
				break;
			}
		}
		break;
		}
		return FBXVector3();
	}
}