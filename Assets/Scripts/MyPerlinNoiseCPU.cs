using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Demo_ProceduralPlacement
{
    public class MyPerlinNoiseCPU
    {
        public MyPerlinNoiseCPU()
        {
        }

        Vector2 randomGradient(Vector2 p)
        {
            p = new Vector2(p.x % 289, p.y % 289);
            float x = (34 * p.x + 1) * p.x % 289 + p.y;
            x = (34 * x + 1) * x % 289;
            x = (x / 41 - Mathf.Floor(x / 41)) * 2 - 1;
            return (new Vector2(x - Mathf.Floor(x + 0.5f), Mathf.Abs(x) - 0.5f)).normalized;
        }

        Vector2 quintic(Vector2 p)
        {
            return p * p * p * (new Vector2(10.0f, 10.0f) + p * (new Vector2(-15.0f, -15.0f) + p * (float)6.0));
        }

        public float perlinNoise(Vector2 uv, float tiling)
        {
            uv *= tiling;
            Vector2 gridId = new Vector2(Mathf.Floor(uv.x), Mathf.Floor(uv.y));
            Vector2 gridUv = uv - gridId;// frac(uv);

            // start by finding the coords of grid corners
            Vector2 bl = gridId + new Vector2(0.0f, 0.0f);
            Vector2 br = gridId + new Vector2(1.0f, 0.0f);
            Vector2 tl = gridId + new Vector2(0.0f, 1.0f);
            Vector2 tr = gridId + new Vector2(1.0f, 1.0f);

            // find random gradient for each grid corner
            Vector2 gradBl = randomGradient(bl);
            Vector2 gradBr = randomGradient(br);
            Vector2 gradTl = randomGradient(tl);
            Vector2 gradTr = randomGradient(tr);

            // find distance from current pixel to each grid corner
            Vector2 distFromPixelToBl = gridUv - new Vector2(0.0f, 0.0f);
            Vector2 distFromPixelToBr = gridUv - new Vector2(1.0f, 0.0f);
            Vector2 distFromPixelToTl = gridUv - new Vector2(0.0f, 1.0f);
            Vector2 distFromPixelToTr = gridUv - new Vector2(1.0f, 1.0f);

            // calculate the dot products of gradients + distances
            float dotBl = Vector2.Dot(gradBl, distFromPixelToBl);
            float dotBr = Vector2.Dot(gradBr, distFromPixelToBr);
            float dotTl = Vector2.Dot(gradTl, distFromPixelToTl);
            float dotTr = Vector2.Dot(gradTr, distFromPixelToTr);

            // part 4.4 - smooth out gridUvs
            gridUv = quintic(gridUv);

            // perform linear interpolation between 4 dot products
            float b = Mathf.Lerp(dotBl, dotBr, gridUv.x);
            float t = Mathf.Lerp(dotTl, dotTr, gridUv.x);
            float perlin = Mathf.Lerp(b, t, gridUv.y);

            return perlin;
        }
    }
}