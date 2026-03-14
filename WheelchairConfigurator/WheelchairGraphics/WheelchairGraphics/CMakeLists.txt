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
)

if (CMAKE_VERSION VERSION_GREATER 3.12)
  set_property(TARGET WheelchairGraphics PROPERTY CXX_STANDARD 20)
endif()

target_include_directories(WheelchairGraphics 
	PUBLIC ${CMAKE_CURRENT_SOURCE_DIR}/includePublic
	PRIVATE ${CMAKE_CURRENT_SOURCE_DIR}/include
)

target_compile_definitions(WheelchairGraphics PRIVATE _WG_API_EXPORT_)