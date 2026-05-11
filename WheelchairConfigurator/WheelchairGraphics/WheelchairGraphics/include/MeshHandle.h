#pragma once

#include "VulkanCommon.h"
#include <string>

#include <assimp/Importer.hpp> 
#include <assimp/scene.h>     
#include <assimp/postprocess.h>
#include <assimp/cimport.h>

#include <glm/glm.hpp>

#include <vector>


namespace VkLoader {

	struct Vertex
	{
		glm::vec3 m_pos;
		glm::vec2 m_tex;
		glm::vec3 m_normal;
		glm::vec3 m_color;
		glm::vec3 m_tangent;
		glm::vec3 m_binormal;

		Vertex() {}

		Vertex(const glm::vec3& pos, const glm::vec2& tex, const glm::vec3& normal, const glm::vec3& tangent, const glm::vec3& bitangent, const glm::vec3& color)
		{
			m_pos = pos;
			m_tex = tex;
			m_normal = normal;
			m_color = color;
			m_tangent = tangent;
			m_binormal = bitangent;
		}
	};

	struct MeshEntry {
		uint32_t NumIndices;
		uint32_t MaterialIndex;
		uint32_t vertexBase;
		std::vector<Vertex> Vertices;
		std::vector<unsigned int> Indices;
	};

	struct Dimension
	{
		glm::vec3 min = glm::vec3(FLT_MAX);
		glm::vec3 max = glm::vec3(-FLT_MAX);
		glm::vec3 size;
	};

	class MeshHandle {
	public:
		MeshHandle() = default;
		~MeshHandle();

		VkResult LoadMesh(std::string const& filename);
		VkResult LoadMeshFromFile(std::string const& filepath);

		uint32_t GetEntriesSize() const;
		MeshEntry const& GetEntry(uint32_t index) const;

	private:
		void InitMesh(unsigned int index, const aiMesh* paiMesh);

		const aiScene* m_scene;
		Assimp::Importer m_importer;
		std::vector<MeshEntry> m_entries;
		uint32_t m_numVertices = 0;
		Dimension m_dim;
	};
}