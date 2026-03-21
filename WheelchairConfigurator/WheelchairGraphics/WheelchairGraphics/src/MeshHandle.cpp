#include "MeshHandle.h"

VkLoader::MeshHandle::~MeshHandle()
{
	m_entries.clear();
}

VkResult VkLoader::MeshHandle::LoadMesh(std::string const& filename)
{
	const int flags = aiProcess_FlipWindingOrder | aiProcess_Triangulate | aiProcess_PreTransformVertices | aiProcess_CalcTangentSpace | aiProcess_GenSmoothNormals;

#if defined(__ANDROID__)
	AAsset* asset = AAssetManager_open(assetManager, filename.c_str(), AASSET_MODE_STREAMING);
	assert(asset);
	size_t size = AAsset_getLength(asset);

	assert(size > 0);

	void* meshData = malloc(size);
	AAsset_read(asset, meshData, size);
	AAsset_close(asset);

	pScene = Importer.ReadFileFromMemory(meshData, size, flags);

	free(meshData);
#else
	m_scene = m_importer.ReadFile(filename.c_str(), flags);
#endif

	if (!m_scene)
		return VK_ERROR_UNKNOWN;

	m_entries.resize(m_scene->mNumMeshes);

	for (unsigned int i = 0; i < m_entries.size(); i++)
	{
		m_entries[i].vertexBase = m_numVertices;
		m_numVertices += m_scene->mMeshes[i]->mNumVertices;
	}

	for (unsigned int i = 0; i < m_entries.size(); i++)
	{
		const aiMesh* paiMesh = m_scene->mMeshes[i];
		InitMesh(i, paiMesh);
	}

	return VK_SUCCESS;
}

uint32_t VkLoader::MeshHandle::GetEntriesSize() const
{
	return m_entries.size();
}

VkLoader::MeshEntry const& VkLoader::MeshHandle::GetEntry(uint32_t index) const
{
	return m_entries[index];
}

void VkLoader::MeshHandle::InitMesh(unsigned int index, const aiMesh* paiMesh)
{
	m_entries[index].MaterialIndex = paiMesh->mMaterialIndex;

	aiColor3D pColor(0.f, 0.f, 0.f);
	m_scene->mMaterials[paiMesh->mMaterialIndex]->Get(AI_MATKEY_COLOR_DIFFUSE, pColor);

	aiVector3D Zero3D(0.0f, 0.0f, 0.0f);

	for (unsigned int i = 0; i < paiMesh->mNumVertices; i++) {
		aiVector3D* pPos = &(paiMesh->mVertices[i]);
		aiVector3D* pNormal = &(paiMesh->mNormals[i]);
		aiVector3D* pTexCoord;
		if (paiMesh->HasTextureCoords(0))
		{
			pTexCoord = &(paiMesh->mTextureCoords[0][i]);
		}
		else {
			pTexCoord = &Zero3D;
		}
		aiVector3D* pTangent = (paiMesh->HasTangentsAndBitangents()) ? &(paiMesh->mTangents[i]) : &Zero3D;
		aiVector3D* pBiTangent = (paiMesh->HasTangentsAndBitangents()) ? &(paiMesh->mBitangents[i]) : &Zero3D;

		Vertex v(glm::vec3(pPos->x, -pPos->y, pPos->z),
			glm::vec2(pTexCoord->x, pTexCoord->y),
			glm::vec3(pNormal->x, pNormal->y, pNormal->z),
			glm::vec3(pTangent->x, pTangent->y, pTangent->z),
			glm::vec3(pBiTangent->x, pBiTangent->y, pBiTangent->z),
			glm::vec3(pColor.r, pColor.g, pColor.b)
		);

		m_dim.max.x = fmax(pPos->x, m_dim.max.x);
		m_dim.max.y = fmax(pPos->y, m_dim.max.y);
		m_dim.max.z = fmax(pPos->z, m_dim.max.z);

		m_dim.min.x = fmin(pPos->x, m_dim.min.x);
		m_dim.min.y = fmin(pPos->y, m_dim.min.y);
		m_dim.min.z = fmin(pPos->z, m_dim.min.z);

		m_entries[index].Vertices.push_back(v);
	}

	m_dim.size = m_dim.max - m_dim.min;

	for (unsigned int i = 0; i < paiMesh->mNumFaces; i++)
	{
		const aiFace& Face = paiMesh->mFaces[i];
		if (Face.mNumIndices != 3)
			continue;
		m_entries[index].Indices.push_back(Face.mIndices[0]);
		m_entries[index].Indices.push_back(Face.mIndices[1]);
		m_entries[index].Indices.push_back(Face.mIndices[2]);
	}
}
