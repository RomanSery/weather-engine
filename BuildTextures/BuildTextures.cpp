// This is the main DLL file.

#include "stdafx.h"

#include "BuildTextures.h"

namespace BuildTextures {


	void TextureHelper::ProcessTexture(String ^inputFile, String ^diffOutFile, String ^normalMapOutFile)
	{
		Bitmap ^objBitmap = gcnew Bitmap(inputFile);
		nv::Color32 *m_data = GetColorArray(objBitmap);

		if(diffOutFile != nullptr)
			CreateDiffuseTexture(m_data, diffOutFile, objBitmap->Width, objBitmap->Height);
		
		//if(normalMapOutFile != nullptr)
			//CreateNormalMapTexture(m_data, normalMapOutFile, objBitmap->Width, objBitmap->Height);
	}

	array<unsigned char>^ TextureHelper::BitmapDataFromBitmap (Bitmap ^objBitmap)
	{	 
		MemoryStream ^ms = gcnew MemoryStream();
		objBitmap->Save(ms, ImageFormat::Bmp);
		return ms->GetBuffer();     
	}

	nv::Color32* TextureHelper::GetColorArray(Bitmap ^objBitmap)
	{	 
		nv::Color32 *m_data;
		m_data = (nv::Color32 *)realloc(m_data, objBitmap->Width * objBitmap->Height * sizeof(nv::Color32));

		array<unsigned char>^ BitmapData = BitmapDataFromBitmap(objBitmap);
		int nPixels = objBitmap->Width * objBitmap->Height;
		nv::Color32 c;
		int i = 0;
		for( int y=0; y<objBitmap->Height; y++ )
		{
		  for( int x=0; x<objBitmap->Width; x++ )
		  {
			Color col = objBitmap->GetPixel( x, y );
			
			//c.setRGBA(col.R, col.G, col.B, 1);
			uint8 b = col.B;
			uint8 g = col.G;
			uint8 r = col.R;

			c.setBGRA(b, g, r, 255);
			m_data[i] = c;		
			
			i++;
		  }
		}
		
		return m_data;   
	}

	void TextureHelper::CreateDiffuseTexture (nv::Color32 *m_data, String ^outFile, int w, int h)
	{	 
		InputOptions inputOptions;
		inputOptions.setTextureLayout(nvtt::TextureType_2D, w, h);
		inputOptions.setMipmapData(m_data, w, h);			
		
		OutputOptions outputOptions;		
		char* str2 = (char*)(void*)Marshal::StringToHGlobalAnsi(outFile);		
		outputOptions.setFileName(str2);	
		
		CompressionOptions compressionOptions;
		compressionOptions.setFormat(Format_DXT1);		
		compressionOptions.setQuality(Quality::Quality_Fastest);		

		Compressor compressor;
		compressor.process(inputOptions, compressionOptions, outputOptions);   
	}
	void TextureHelper::CreateNormalMapTexture (nv::Color32 *m_data, String ^outFile, int w, int h)
	{	 
		InputOptions inputOptions;
		inputOptions.setTextureLayout(nvtt::TextureType_2D, w, h);
		inputOptions.setMipmapData(m_data, w, h);	
		inputOptions.setConvertToNormalMap(true);
		inputOptions.setHeightEvaluation(-0.3,-0.3,-0.3,-0.3);
		inputOptions.setNormalizeMipmaps(true);		
		inputOptions.setNormalFilter(0.1, 0.2, 0.9, 0.9);
				
		OutputOptions outputOptions;		
		char* str2 = (char*)(void*)Marshal::StringToHGlobalAnsi(outFile);		
		outputOptions.setFileName(str2);	

		CompressionOptions compressionOptions;
		compressionOptions.setFormat(Format_DXT1);				
		compressionOptions.setQuality(Quality::Quality_Fastest);		

		Compressor compressor;
		compressor.process(inputOptions, compressionOptions, outputOptions);   
	}

}