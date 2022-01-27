using Unity.Mathematics;
using Unity.Collections;
using Unity.Burst;
using Unity.Jobs;

// src: https://github.com/andrew-raphael-lukasik/RawTextureDataProcessingExamples/blob/master/Jobs/BoxBlurRGB24Job.cs
[BurstCompile]
public struct BoxBlurRGB24Job : IJob
{
    [DeallocateOnJobCompletion]
    NativeArray<RGB24> copy;

    readonly int w, h, r;
    NativeArray<RGB24> results;

    public BoxBlurRGB24Job(NativeArray<RGB24> data, int textureWidth, int textureHeight, int radius)
    {
        this.results = data;
        this.copy = new NativeArray<RGB24>(data, Allocator.TempJob);
        this.w = textureWidth;
        this.h = textureHeight;
        this.r = radius;
    }

    void IJob.Execute()
    {
        BoxBlurHorizontal(results, copy);
        BoxBlurTotal(copy, results);
    }

    void BoxBlurHorizontal(NativeArray<RGB24> src, NativeArray<RGB24> dst)
    {
        float iarr = 1f / (float)(r + r + 1);

        for (int i = 0; i < h; i++)
        {
            int ti = i * w;
            int li = ti;
            int ri = ti + r;
            float3 fv = (int3)src[ti];
            float3 lv = (int3)src[ti + w - 1];
            float3 val = (r + 1) * fv;

            for (var j = 0; j < r; j++)
            {
                val += (int3)src[ti + j];
            }

            for (var j = 0; j <= r; j++)
            {
                val += (int3)src[ri++] - fv;
                dst[ti++] = (RGB24)(int3)math.round(val * iarr);
            }

            for (var j = r + 1; j < w - r; j++)
            {
                val += (int3)src[ri++] - (int3)src[li++];
                dst[ti++] = (RGB24)(int3)math.round(val * iarr);
            }

            for (var j = w - r; j < w; j++)
            {
                val += lv - (int3)src[li++];
                dst[ti++] = (RGB24)(int3)math.round(val * iarr);
            }
        }
    }

    void BoxBlurTotal(NativeArray<RGB24> src, NativeArray<RGB24> dst)
    {
        float3 iarr = 1f / (float)(r + r + 1);

        for (int i = 0; i < w; i++)
        {
            int ti = i;
            int li = ti;
            int ri = ti + r * w;
            float3 fv = (int3)src[ti];
            float3 lv = (int3)src[ti + w * (h - 1)];
            float3 val = (r + 1) * fv;
            for (var j = 0; j < r; j++)
            {
                val += (int3)src[ti + j * w];
            }

            for (var j = 0; j <= r; j++)
            {
                val += (int3)src[ri] - fv;
                dst[ti] = (RGB24)(int3)math.round(val * iarr);
                ri += w;
                ti += w;
            }

            for (var j = r + 1; j < h - r; j++)
            {
                val += (int3)src[ri] - (int3)src[li];
                dst[ti] = (RGB24)(int3)math.round(val * iarr);
                li += w;
                ri += w;
                ti += w;
            }

            for (var j = h - r; j < h; j++)
            {
                val += lv - (int3)src[li];
                dst[ti] = (RGB24)(int3)math.round(val * iarr);
                li += w;
                ti += w;
            }
        }
    }
}

// src: https://github.com/andrew-raphael-lukasik/RawTextureDataProcessingExamples/blob/master/Structures/RGB24.cs
public struct RGB24
{
    public byte R, G, B;

    public static explicit operator int3(RGB24 val) => new int3 { x = val.R, y = val.G, z = val.B };
    public static explicit operator RGB24(int3 val) => new RGB24 { R = (byte)val.x, G = (byte)val.y, B = (byte)val.z };

    public static RGB24 operator +(RGB24 lhs, RGB24 rhs) => (RGB24)((int3)lhs + (int3)rhs);
    public static RGB24 operator -(RGB24 lhs, RGB24 rhs) => (RGB24)((int3)lhs - (int3)rhs);
}
