namespace Mackiloha.Texture;

public static class TPL
{
    public static void ShuffleBlocks(HMXBitmap bitmap, Span<byte> data)
    {
        var span = (16 * (bitmap.Bpp)) / 8; // 1 block = 16 pixels
        var blocksX = bitmap.Width / 4;
        var blocksY = bitmap.Height / 4;

        ShuffleBlocks(data, blocksX, blocksY, span);
    }

    private static void ShuffleBlocks(Span<byte> data, int bx, int by, int blockSize)
    {
        /*for (var y = 0; y < by; y++)
        {
            for (var x = 0; x < bx; x++)
            {

            }
        }*/

        var totalBlocks = bx * by;
        // var totalByteSize = totalBlocks * blockSize;
        var bytesInLine = bx * blockSize;

        // Group blocks by 2
        var groupByteSize = blockSize * 2;
        var totalGroupedBlocks = (bx * by) / 2;
        var groupBlocksIn2Rows = bx;
        var totalByteSize = (bx * by) * blockSize;

        var workingData = data[..totalByteSize];

        // Create map
        Span<int> map = stackalloc int[groupBlocksIn2Rows];
        for (var i = 0; i < map.Length; i++)
        {
            map[i] = (i / 2) + ((i % 2) * (bx / 2));
        }

        //Array.Sort()

        /*var blocks = Enumerable
            .Range(0, totalBlocks)
            .Select(i => (i, ));*/

        Span<byte> origData = stackalloc byte[groupBlocksIn2Rows * groupByteSize];

        for (var i = 0; i < totalGroupedBlocks; i += 1)
        {
            var o = i / groupBlocksIn2Rows;
            var x = i % groupBlocksIn2Rows;

            var currentWorkingIndex = o * groupBlocksIn2Rows;
            var currentIndex = x * groupByteSize;
            var newIndex = map[x] * groupByteSize;

            // Copy data at start of every 2-row group
            if (x == 0)
            {
                var workingStart = o * origData.Length;
                workingData[workingStart..(workingStart + origData.Length)].CopyTo(origData);
            }

            /*if (currentIndex == newIndex)
            {
                // No need to copy anything
                continue;
            }*/

            Span<byte> currentSpan = origData[currentIndex..(currentIndex + groupByteSize)];
            Span<byte> newSpan = workingData[(currentWorkingIndex + newIndex)..((currentWorkingIndex + newIndex) + groupByteSize)];

            // Copy data
            currentSpan.CopyTo(newSpan);
            //newSpan.CopyTo(currentSpan);
            //buffer.CopyTo(newSpan);
        }
    }
}