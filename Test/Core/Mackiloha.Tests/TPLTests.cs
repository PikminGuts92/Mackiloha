using AutoFixture;
using Mackiloha.Texture;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mackiloha.Tests;

public class TPLTests
{
    private readonly Fixture _fixture = new Fixture();

    [Theory]
    [InlineData(new int[] { 0, 4, 1, 5, 2, 6, 3, 7 })]
    public void CreateBlockMap(int[] expectedMap)
    {
        Span<int> actualMap = stackalloc int[expectedMap.Length];
        TPL.CreateBlockMap(actualMap);

        Assert.Equal(expectedMap, actualMap);
    }

    [Theory]
    [InlineData(new int[] { 0, 2, 4, 6, 1, 3, 5, 7 })]
    public void CreateBlockMapInverse(int[] expectedMap)
    {
        Span<int> actualMap = stackalloc int[expectedMap.Length];
        TPL.CreateBlockMapInverse(actualMap);

        Assert.Equal(expectedMap, actualMap);
    }

    [Theory]
    [InlineData(  64,   64)]
    [InlineData( 128,  128)]
    [InlineData( 256,  256)]
    [InlineData( 128,  256)]
    [InlineData( 256,  128)]
    public void DXT1ToTPL(int width, int height)
    {
        // 4bpp image
        var origData = _fixture
            .CreateMany<byte>((width * height) / 2)
            .ToArray();

        var newData = origData.ToArray();
        TPL.TPLToDXT1(width, height, newData);
        TPL.DXT1ToTPL(width, height, newData);

        Assert.Equal(origData, newData);
    }
}
