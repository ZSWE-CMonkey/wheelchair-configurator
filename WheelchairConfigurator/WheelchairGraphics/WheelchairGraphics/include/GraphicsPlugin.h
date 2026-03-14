#pragma once

#include <memory>
#include <stdexcept>
#include <string>

#define GP_TOTAL_SUCCES(caller) caller == GP_SUCCESS
#define GP_SUCCEDED(caller) caller >= 0
#define GP_FAILED(caller) caller < 0

#define GP_THROW_IF_FAIL(caller) { GPluginResult res = caller; if (res < 0) throw std::runtime_error("GP returned with error code: " + std::to_string(res)); }

namespace GraphicsPlugin {

	enum GPluginResult {
		//errors
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

		virtual GPluginResult Initialize() = 0;
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