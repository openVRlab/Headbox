#pragma once

#include "Interfaces/IGraphicsEncoderDevice.hpp"

#include "../Unity/IUnityGraphicsMetal.h"
#include "../Unity/IUnityRenderingExtensions.h"

#include <Metal/Metal.h>

namespace MacOsEncodingPlugin
{

    class MetalGraphicsEncoderDevice : public IGraphicsEncoderDevice
    {
    public:
        MetalGraphicsEncoderDevice(id<MTLDevice> device, IUnityGraphicsMetal* unityGraphicsMetal);
        virtual ~MetalGraphicsEncoderDevice();

        virtual void* GetEncodeDevicePtr() override;
        virtual ITexture2D* CreateDefaultTextureFromNative(uint32_t w, uint32_t h, void* nativeTexturePtr);
        virtual bool CopyResourceFromNative(ITexture2D* dest, void* nativeTexturePtr) override;
        
    private:
        id<MTLDevice>         m_Device;
        IUnityGraphicsMetal*  m_UnityGraphicsMetal;
        
        bool CopyTexture(id<MTLTexture> dest, id<MTLTexture> src);
    };
} // MacOsEncodingPlugin
