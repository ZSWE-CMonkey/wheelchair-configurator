#include "MeshHandle.h"
#include "../assetResources/assetResource.h"

VkLoader::MeshHandle::~MeshHandle()
{
	m_entries.clear();
}

VkResult VkLoader::MeshHandle::LoadMesh(std::string const& filename)
{
	const int flags = aiProcess_FlipWindingOrder | aiProcess_Triangulate | aiProcess_PreTransformVertices | aiProcess_CalcTangentSpace | aiProcess_GenSmoothNormals;

	auto model = getEmbeddedAsset(filename);

	m_scene = m_importer.ReadFileFromMemory(model->data, model->size, flags, "dae");

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

VkResult VkLoader::MeshHandle::LoadMeshFromFile(std::string const& filepath, float scale,
	float anchorX, float anchorY, float anchorZ,
	float rotationX, float rotationY, float rotationZ)
{
	const int flags = aiProcess_FlipWindingOrder | aiProcess_Triangulate | aiProcess_PreTransformVertices | aiProcess_CalcTangentSpace | aiProcess_GenSmoothNormals | aiProcess_GlobalScale | aiProcess_FlipUVs;

	glm::mat4 rotMat = glm::rotate(glm::mat4(1.0f), glm::radians(rotationX), glm::vec3(1.0f, 0.0f, 0.0f))
		* glm::rotate(glm::mat4(1.0f), glm::radians(rotationY), glm::vec3(0.0f, 1.0f, 0.0f))
		* glm::rotate(glm::mat4(1.0f), glm::radians(rotationZ), glm::vec3(0.0f, 0.0f, 1.0f));
	m_postRotMat = glm::mat3(rotMat);
	m_postAnchor = glm::vec3(anchorX, anchorY, anchorZ);

	m_importer.SetPropertyFloat(AI_CONFIG_GLOBAL_SCALE_FACTOR_KEY, scale);
	m_scene = m_importer.ReadFile(filepath, flags);

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

bool VkLoader::MeshHandle::TryGetEmbeddedTexture(const uint8_t** outData, size_t* outSize) const
{
	if (!m_scene || m_scene->mNumTextures == 0 || !m_scene->mTextures || !m_scene->mTextures[0])
		return false;
	const aiTexture* tex = m_scene->mTextures[0];
	if (tex->mHeight != 0 || tex->pcData == nullptr)
		return false; // not a compressed embedded blob
	*outData = reinterpret_cast<const uint8_t*>(tex->pcData);
	*outSize = static_cast<size_t>(tex->mWidth);
	return true;
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

		glm::vec3 pos = m_postRotMat * glm::vec3(pPos->x, -pPos->y, pPos->z) + m_postAnchor;
		glm::vec3 nrm = m_postRotMat * glm::vec3(pNormal->x, pNormal->y, pNormal->z);
		glm::vec3 tan = m_postRotMat * glm::vec3(pTangent->x, pTangent->y, pTangent->z);
		glm::vec3 btn = m_postRotMat * glm::vec3(pBiTangent->x, pBiTangent->y, pBiTangent->z);

		Vertex v(pos,
			glm::vec2(pTexCoord->x, pTexCoord->y),
			nrm,
			tan,
			btn,
			glm::vec3(pColor.r, pColor.g, pColor.b)
		);

		m_dim.max.x = fmax(pos.x, m_dim.max.x);
		m_dim.max.y = fmax(pos.y, m_dim.max.y);
		m_dim.max.z = fmax(pos.z, m_dim.max.z);

		m_dim.min.x = fmin(pos.x, m_dim.min.x);
		m_dim.min.y = fmin(pos.y, m_dim.min.y);
		m_dim.min.z = fmin(pos.z, m_dim.min.z);

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
