#pragma once

#include <iostream>

namespace MacOsEncodingPlugin
{
    class ITexture2D
    {
    public:
        ITexture2D(uint32_t w, uint32_t h) : m_Width(w), m_Height(h) {}
        bool IsSize(uint32_t w, uint32_t h) const { return m_Width == w && m_Height == h; }

        virtual ~ITexture2D() = 0;

        virtual void* GetNativeTexturePtr() = 0;
        virtual const void* GetNativeTexturePtr() const = 0;

        uint32_t GetWidth() const { return m_Width; }
        uint32_t GetHeight()  const { return m_Height; }

    protected:
        uint32_t m_Width;
        uint32_t m_Height;
    };
}
