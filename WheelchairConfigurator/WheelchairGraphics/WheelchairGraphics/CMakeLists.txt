add_library (
	WheelchairGraphics 
	SHARED
	"src/WheelchairGraphics.cpp" 
	"src/GraphicsPlugin.cpp"
	"src/VulkanGraphicsPlugin.cpp"
	"src/VulkanEngine.cpp"

	"include/GraphicsPlugin.h"
	"include/VulkanGraphicsPlugin.h"
	"include/VulkanEngine.h"

	"includePublic/WheelchairGraphics.h"
 "include/VulkanSwapchain.h" "src/VulkanSwapchain.cpp" "include/VulkanCommon.h" "include/ObjectLoader.h" "src/ObjectLoader.cpp")

if (CMAKE_VERSION VERSION_GREATER 3.12)
  set_property(TARGET WheelchairGraphics PROPERTY CXX_STANDARD 20)
endif()

target_include_directories(WheelchairGraphics 
	PUBLIC ${CMAKE_CURRENT_SOURCE_DIR}/includePublic
	PRIVATE ${CMAKE_CURRENT_SOURCE_DIR}/include
)

target_compile_definitions(WheelchairGraphics 
	PRIVATE 
		_WG_API_EXPORT_
		VK_USE_PLATFORM_WIN32_KHR
)

find_package(Vulkan REQUIRED)
target_link_libraries(WheelchairGraphics PRIVATE Vulkan::Vulkan)

include_directories("C:/tool-Assimp/assimp-include")
target_link_libraries(WheelchairGraphics PRIVATE "C:/tool-Assimp/assimp-lib/assimp/assimp.lib")

include_directories("C:/tool-glm")