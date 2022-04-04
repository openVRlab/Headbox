#include "MetalGraphicsEncoderDevice.hpp"
#include "MetalTexture2D.hpp"
#import <Metal/Metal.h>

namespace MacOsEncodingPlugin
{

    MetalGraphicsEncoderDevice::MetalGraphicsEncoderDevice(id<MTLDevice> device,
                                                           IUnityGraphicsMetal* unityGraphicsMetal)
        : m_Device(device)
        , m_UnityGraphicsMetal(unityGraphicsMetal)
    {
    }

    MetalGraphicsEncoderDevice::~MetalGraphicsEncoderDevice()
    {
        m_Device = nullptr;
        m_UnityGraphicsMetal = nullptr;
    }
    
    void* MetalGraphicsEncoderDevice::GetEncodeDevicePtr()
    {
        return m_Device;
    }

    ITexture2D* MetalGraphicsEncoderDevice::CreateDefaultTextureFromNative(uint32_t w,
                                                                           uint32_t h,
                                                                           void* nativeTexturePtr)
    {
        id<MTLTexture> texture = (__bridge id<MTLTexture>)nativeTexturePtr;
        return new MetalTexture2D(w, h, texture);
    }

    bool MetalGraphicsEncoderDevice::CopyResourceFromNative(ITexture2D* dest, void* nativeTexturePtr)
    {
        if(nativeTexturePtr == nullptr)
            return false;
        
        id<MTLTexture> dstTexture = (__bridge id<MTLTexture>)dest->GetNativeTexturePtr();
        id<MTLTexture> srcTexture = (__bridge id<MTLTexture>)nativeTexturePtr;
        return CopyTexture(dstTexture, srcTexture);
    }

    bool MetalGraphicsEncoderDevice::CopyTexture(id<MTLTexture> dest, id<MTLTexture> src)
    {
        if(dest == src)
            return false;

        if(src.pixelFormat != dest.pixelFormat)
            return false;

        m_UnityGraphicsMetal->EndCurrentCommandEncoder();

        id<MTLCommandBuffer> commandBuffer = m_UnityGraphicsMetal->CurrentCommandBuffer();
        id<MTLBlitCommandEncoder> blit = [commandBuffer blitCommandEncoder];
        
        NSUInteger width = src.width;
        NSUInteger height = src.height;

        MTLSize inTxtSize = MTLSizeMake(width, height, 1);
        MTLOrigin inTxtOrigin = MTLOriginMake(0, 0, 0);
        MTLOrigin outTxtOrigin = MTLOriginMake(0, 0, 0);

        [blit copyFromTexture:src
                        sourceSlice:0
                        sourceLevel:0
                        sourceOrigin:inTxtOrigin
                        sourceSize:inTxtSize
                        toTexture:dest
                        destinationSlice:0
                        destinationLevel:0
                        destinationOrigin:outTxtOrigin];
        
        [blit synchronizeResource:dest];
        
        [blit endEncoding];
        blit = nil;
        m_UnityGraphicsMetal->EndCurrentCommandEncoder();

        return true;
    }
}
