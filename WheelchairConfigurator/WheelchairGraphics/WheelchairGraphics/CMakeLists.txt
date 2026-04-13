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
 "include/VulkanSwapchain.h" "src/VulkanSwapchain.cpp" "include/VulkanCommon.h" "include/ObjectLoader.h" "src/ObjectLoader.cpp" "include/TextureHandle.h" "src/TextureHandle.cpp" "include/MeshHandle.h" "src/MeshHandle.cpp" "assetResources/assetResource.cpp" "assetResources/assetResource.h" "vulkanAndroid/vulkanandroid.cpp" "vulkanAndroid/vulkanandroid.h")

if (CMAKE_VERSION VERSION_GREATER 3.12)
  set_property(TARGET WheelchairGraphics PROPERTY CXX_STANDARD 20)
endif()

target_include_directories(WheelchairGraphics 
	PUBLIC ${CMAKE_CURRENT_SOURCE_DIR}/includePublic
	PRIVATE ${CMAKE_CURRENT_SOURCE_DIR}/include
)

if(WIN32)
target_compile_definitions(WheelchairGraphics 
	PRIVATE 
		_WG_API_EXPORT_
		VK_USE_PLATFORM_WIN32_KHR
)

find_package(Vulkan REQUIRED)
target_link_libraries(WheelchairGraphics PRIVATE Vulkan::Vulkan)
else()
target_compile_definitions(WheelchairGraphics 
	PRIVATE 
		_WG_API_EXPORT_
		VK_NO_PROTOTYPES
		VK_USE_PLATFORM_ANDROID_KHR
)

target_include_directories(WheelchairGraphics PRIVATE ${Vulkan_INCLUDE_DIRS})
endif()


target_link_libraries(WheelchairGraphics PRIVATE assimp::assimp)

include_directories("C:/tool-glm")
include_directories("C:/tool-gli")