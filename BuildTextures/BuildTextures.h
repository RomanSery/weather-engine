// BuildTextures.h

#pragma once
#include "nvtt/nvtt.h"
#include "Color.h"
#include "Image.h"
#include <msclr/marshal.h>

using namespace System;
using namespace System::IO;
using namespace System::Drawing;
using namespace System::Drawing::Imaging;
using namespace msclr::interop;
using namespace System::Runtime::InteropServices;
using namespace nvtt;

namespace BuildTextures {

	public ref class TextureHelper
	{
	public:
		static void ProcessTexture(String ^inputFile, String ^diffOutFile, String ^normalMapOutFile);
	private:
		static array<unsigned char>^ BitmapDataFromBitmap (Bitmap ^objBitmap);
		static nv::Color32* GetColorArray (Bitmap ^objBitmap);

		static void CreateDiffuseTexture (nv::Color32 *m_data, String ^outFile, int w, int h);
		static void CreateNormalMapTexture (nv::Color32 *m_data, String ^outFile, int w, int h);
	};
}
