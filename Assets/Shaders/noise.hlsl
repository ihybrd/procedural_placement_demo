float2 randomGradient(float2 p)
{
    p = p % 289;
    float x = (34 * p.x + 1) * p.x % 289 + p.y;
    x = (34 * x + 1) * x % 289;
    x = frac(x / 41) * 2 - 1;
    return normalize(float2(x - floor(x + 0.5), abs(x) - 0.5));
}

float2 quintic(float2 p) {
  return p * p * p * (10.0 + p * (-15.0 + p * 6.0));
}

float perlinNoise(float2 uv, float tiling){
    uv *= tiling;
    float2 gridId = floor(uv);
    float2 gridUv = frac(uv);

    // start by finding the coords of grid corners
    float2 bl = gridId + float2(0.0, 0.0);
    float2 br = gridId + float2(1.0, 0.0);
    float2 tl = gridId + float2(0.0, 1.0);
    float2 tr = gridId + float2(1.0, 1.0);

    // find random gradient for each grid corner
    float2 gradBl = randomGradient(bl);
    float2 gradBr = randomGradient(br);
    float2 gradTl = randomGradient(tl);
    float2 gradTr = randomGradient(tr);

    // find distance from current pixel to each grid corner
    float2 distFromPixelToBl = gridUv - float2(0.0, 0.0);
    float2 distFromPixelToBr = gridUv - float2(1.0, 0.0);
    float2 distFromPixelToTl = gridUv - float2(0.0, 1.0);
    float2 distFromPixelToTr = gridUv - float2(1.0, 1.0);

    // calculate the dot products of gradients + distances
    float dotBl = dot(gradBl, distFromPixelToBl);
    float dotBr = dot(gradBr, distFromPixelToBr);
    float dotTl = dot(gradTl, distFromPixelToTl);
    float dotTr = dot(gradTr, distFromPixelToTr);

    // part 4.4 - smooth out gridUvs
    // gridUv = smoothstep(0.0, 1.0, gridUv);
    // gridUv = cubic(gridUv);
    gridUv = quintic(gridUv);

    // perform linear interpolation between 4 dot products
    float b = lerp(dotBl, dotBr, gridUv.x);
    float t = lerp(dotTl, dotTr, gridUv.x);
    float perlin = lerp(b, t, gridUv.y);

    return perlin;
}