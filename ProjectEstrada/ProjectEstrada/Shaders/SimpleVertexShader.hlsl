struct VertexShaderInput
{
    float2 pos : POSITION;
};

struct PixelShaderInput
{
    float4 pos : SV_POSITION;
};

[numthreads(4, 4, 1)]
PixelShaderInput SimpleVertexShader(VertexShaderInput input)
{
    PixelShaderInput vertexShaderOutput;

    // For this lesson, set the vertex depth value to 0.5, so it is guaranteed to be drawn.
    vertexShaderOutput.pos = float4(input.pos, 0.5f, 1.0f);

    return vertexShaderOutput;
}