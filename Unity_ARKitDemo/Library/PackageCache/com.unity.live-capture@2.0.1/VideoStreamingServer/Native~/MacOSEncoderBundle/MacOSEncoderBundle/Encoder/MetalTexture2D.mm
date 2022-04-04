#include "MetalTexture2D.hpp"

namespace MacOsEncodingPlugin
{

MetalTexture2D::MetalTexture2D(uint32_t w, uint32_t h, id<MTLTexture> tex) : ITexture2D(w,h)
        , m_Texture(tex)
{
}

MetalTexture2D::~MetalTexture2D()
{
    [m_Texture release];
}

void* MetalTexture2D::GetNativeTexturePtr()
{
    return m_Texture;
}

const void* MetalTexture2D::GetNativeTexturePtr() const
{
    return m_Texture;
}

} // MacOsEncodingPlugin
