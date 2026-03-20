#pragma once

#include <memory>
#include <stdexcept>
#include <string>

#define GP_TOTAL_SUCCES(caller) (caller) == GP_SUCCESS
#define GP_SUCCEDED(caller) (caller) >= 0
#define GP_FAILED(caller) (caller) < 0

#define GP_THROW_IF_FAIL(caller) { GPluginResult res = (caller); if (res < 0) throw std::runtime_error("GP returned with error code: " + std::to_string(res)); }

namespace GraphicsPlugin {

	enum GPluginResult {
		//errors
		GP_INITIALIZATION_FAILED = -3,
		GP_NOT_IMPLEMENTED = -2,
		GP_UNKNOW_ERROR = -1,
		//--------------------//

		GP_SUCCESS = 0,

		//statuses
		//--------------------//
	};

	class IGraphicsPlugin {
	public:
		~IGraphicsPlugin() = default;

#ifdef _WIN32
		virtual void SetHandles(void* platformHandle, void* platformWindow) = 0;
#elif __ANDROID__
		virtual void SetHandles(ANativeWindow* window) = 0;
#else
		virtual void SetHandles() = 0;
#endif


		virtual GPluginResult Initialize(std::string appName, uint32_t width, uint32_t height) = 0;
		virtual GPluginResult SetObject(/*TODO: parameter/s*/) = 0;
		virtual GPluginResult Render() = 0;
		virtual GPluginResult DeInitialize() = 0;
	};

	using GraphicsPluginPtr = std::unique_ptr<IGraphicsPlugin>;

	class GraphicsPluginFactory {
	public:
		static GraphicsPluginPtr CreateVulkanGraphicsPlugin();
		static GraphicsPluginPtr CreateMoltenVulkanGraphicsPlugin();
		//...other graphics plugins...//
	};

}