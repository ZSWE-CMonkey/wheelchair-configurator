add_executable (
	WG_Test 
    WIN32
	"src/main.cpp")

if (CMAKE_VERSION VERSION_GREATER 3.12)
  set_property(TARGET WG_Test PROPERTY CXX_STANDARD 20)
endif()

target_link_libraries(WG_Test PRIVATE WheelchairGraphics)

add_custom_command(TARGET WG_Test POST_BUILD
    COMMAND ${CMAKE_COMMAND} -E copy_if_different
        $<TARGET_FILE:WheelchairGraphics>
        $<TARGET_FILE_DIR:WG_Test>
)