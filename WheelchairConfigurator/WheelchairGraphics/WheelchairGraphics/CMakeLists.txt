add_library (
	WheelchairGraphics 
	SHARED
	"src/WheelchairGraphics.cpp" "include/WheelchairGraphics.h")

if (CMAKE_VERSION VERSION_GREATER 3.12)
  set_property(TARGET WheelchairGraphics PROPERTY CXX_STANDARD 20)
endif()