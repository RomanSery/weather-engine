// This code is in the public domain -- castanyo@yahoo.es

#ifndef NV_MATH_COLOR_H
#define NV_MATH_COLOR_H

typedef unsigned char       uint8;
typedef signed char         int8;

typedef unsigned short      uint16;
typedef signed short        int16;

typedef unsigned int        uint32;
typedef signed int          int32;

typedef unsigned __int64    uint64;
typedef signed __int64      int64;
typedef uint32              uint;
#define NULL 0

namespace nv
{
	class Color32
	{
	public:
		Color32() { }
		Color32(const Color32 & c) : u(c.u) { }
		Color32(uint8 R, uint8 G, uint8 B) { setRGBA(R, G, B, 0xFF); }
		Color32(uint8 R, uint8 G, uint8 B, uint8 A) { setRGBA( R, G, B, A); }		
		explicit Color32(uint32 U) : u(U) { }

		void setRGBA(uint8 R, uint8 G, uint8 B, uint8 A)
		{
			r = R;
			g = G;
			b = B;
			a = A;
		}

		void setBGRA(uint8 B, uint8 G, uint8 R, uint8 A = 0xFF)
		{
			r = R;
			g = G;
			b = B;
			a = A;
		}

		operator uint32 () const { return u; }

		union {
			struct {

				uint8 b, g, r, a;
			};
			uint32 u;
		};
	};







}

#endif // NV_MATH_COLOR_H
