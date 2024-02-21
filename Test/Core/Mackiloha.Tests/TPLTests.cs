using Mackiloha.Texture;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mackiloha.Tests;

public class TPLTests
{
    [Theory]
    [InlineData(new int[] { 0, 4, 1, 5, 2, 6, 3, 7 })]
    public void CreateBlockMap(int[] expectedMap)
    {
        Span<int> actualMap = stackalloc int[expectedMap.Length];
        TPL.CreateBlockMap(actualMap);

        Assert.Equal(expectedMap, actualMap);
    }
}
