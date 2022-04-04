#pragma once

#include <iostream>

namespace MacOsEncodingPlugin
{

class ITexture2D;

class IGraphicsEncoderDevice
{
public:

    IGraphicsEncoderDevice();
    virtual ~IGraphicsEncoderDevice() = 0;
    
    virtual void* GetEncodeDevicePtr() = 0;
    virtual bool  CopyResourceFromNative(ITexture2D* dest, void* nativeTexturePtr) = 0;
};

} // MacOsEncodingPlugin
