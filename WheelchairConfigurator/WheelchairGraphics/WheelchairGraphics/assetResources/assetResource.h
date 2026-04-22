#pragma once
#include <cstdint>
#include <string>
#include <unordered_map>

struct EmbeddedAsset {
    const uint8_t* data;
    size_t size;
};

const EmbeddedAsset* getEmbeddedAsset(const std::string& name);
