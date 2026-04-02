@echo off

del %~dp0\WheelchairGraphics\assetResources\assetResource.cpp
del %~dp0\WheelchairGraphics\assetResources\assetResource.h
python AssetsEmbed.py %~dp0\data\ %~dp0\WheelchairGraphics\assetResources\assetResource