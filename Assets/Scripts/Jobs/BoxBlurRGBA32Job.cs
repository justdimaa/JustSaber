using Unity.Mathematics;
using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;

// src: https://github.com/andrew-raphael-lukasik/RawTextureDataProcessingExamples/blob/master/Jobs/BoxBlurRGBA32Job.cs
[BurstCompile]
public struct BoxBlurRGBA32Job : IJob
{
    [DeallocateOnJobCompletion]
    NativeArray<RGBA32> copy;
    
    readonly int w, h, r;
    NativeArray<RGBA32> results;

    public BoxBlurRGBA32Job(NativeArray<RGBA32> data, int texture_width, int texture_height, int radius)
    {
        this.results = data;
        this.copy = new NativeArray<RGBA32>(data, Allocator.TempJob);
        this.w = texture_width;
        this.h = texture_height;
        this.r = radius;
    }

    void IJob.Execute()
    {
        BoxBlurHorizontal(results, copy);
        BoxBlurTotal(copy, results);
    }

    void BoxBlurHorizontal(NativeArray<RGBA32> src, NativeArray<RGBA32> dst)
    {
        float iarr = 1f / (float)(r + r + 1);

        for (int i = 0; i < h; i++)
        {
            int ti = i * w;
            int li = ti;
            int ri = ti + r;
            float4 fv = (int4)src[ti];
            float4 lv = (int4)src[ti + w - 1];
            float4 val = (r + 1) * fv;

            for (var j = 0; j < r; j++)
            {
                val += (int4)src[ti + j];
            }

            for (var j = 0; j <= r; j++)
            {
                val += (int4)src[ri++] - fv;
                dst[ti++] = (RGBA32)(int4)math.round(val * iarr);
            }

            for (var j = r + 1; j < w - r; j++)
            {
                val += (int4)src[ri++] - (int4)src[li++];
                dst[ti++] = (RGBA32)(int4)math.round(val * iarr);
            }

            for (var j = w - r; j < w; j++)
            {
                val += lv - (int4)src[li++];
                dst[ti++] = (RGBA32)(int4)math.round(val * iarr);
            }
        }
    }

    void BoxBlurTotal(NativeArray<RGBA32> src, NativeArray<RGBA32> dst)
    {
        float4 iarr = 1f / (float)(r + r + 1);

        for (int i = 0; i < w; i++)
        {
            int ti = i;
            int li = ti;
            int ri = ti + r * w;
            float4 fv = (int4)src[ti];
            float4 lv = (int4)src[ti + w * (h - 1)];
            float4 val = (r + 1) * fv;
            for (var j = 0; j < r; j++)
            {
                val += (int4)src[ti + j * w];
            }

            for (var j = 0; j <= r; j++)
            {
                val += (int4)src[ri] - fv;
                dst[ti] = (RGBA32)(int4)math.round(val * iarr);
                ri += w;
                ti += w;
            }

            for (var j = r + 1; j < h - r; j++)
            {
                val += (int4)src[ri] - (int4)src[li];
                dst[ti] = (RGBA32)(int4)math.round(val * iarr);
                li += w;
                ri += w;
                ti += w;
            }

            for (var j = h - r; j < h; j++)
            {
                val += lv - (int4)src[li];
                dst[ti] = (RGBA32)(int4)math.round(val * iarr);
                li += w;
                ti += w;
            }
        }
    }
}

// src: https://github.com/andrew-raphael-lukasik/RawTextureDataProcessingExamples/blob/master/Structures/RGBA32.cs
public struct RGBA32
{
    public byte R, G, B, A;

    public static explicit operator int4(RGBA32 val) => new int4 { x = val.R, y = val.G, z = val.B, w = val.A };
    public static explicit operator RGBA32(int4 val) => new RGBA32 { R = (byte)val.x, G = (byte)val.y, B = (byte)val.z, A = (byte)val.w };

    public static RGBA32 operator +(RGBA32 lhs, RGBA32 rhs) => (RGBA32)((int4)lhs + (int4)rhs);
    public static RGBA32 operator -(RGBA32 lhs, RGBA32 rhs) => (RGBA32)((int4)lhs - (int4)rhs);
}
