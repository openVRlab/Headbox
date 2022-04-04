#pragma once

#include "ITexture2D.hpp"
#import <Metal/Metal.h>

namespace MacOsEncodingPlugin
{
    class MTLTexture;

    struct MetalTexture2D : ITexture2D
    {
        MetalTexture2D(uint32_t w, uint32_t h, id<MTLTexture> tex);
        virtual ~MetalTexture2D();

        virtual void* GetNativeTexturePtr();
        virtual const void* GetNativeTexturePtr() const;
        
    private:
        
        id<MTLTexture> m_Texture;
    };

} //end namespace
