#include "../include/WheelchairGraphics.h"

#include <stdexcept>

WG_API int InitializeVulkanGraphics()
{
	//TODO: create graphics plugin and initialize it
	return 0;
}

WG_API int InitializeMoltenVulkanGraphics()
{
	throw std::runtime_error("MoltenVulkan Graphics Plugin NOT Implemented");
}

WG_API int DeinitializeGraphics() {

	return 0;
}
