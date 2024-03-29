namespace Mackiloha.Texture;

public static class TPL
{
    const int BLOCK_SIZE = 8; // 1 block = 16 pixels

    public static void TPLToDXT1(int width, int height, Span<byte> data)
    {
        ShuffleBlocks(width, height, data);
        FixIndicies(data);
    }

    public static void DXT1ToTPL(int width, int height, Span<byte> data)
    {
        ShuffleBlocks(width, height, data, true);
        FixIndicies(data);
    }

    internal static void ShuffleBlocks(int width, int height, Span<byte> data, bool inverse = false)
    {
        if (!ShouldShuffleBlocks(width, height)) return;

        var blocksX = width / 4;
        var blocksY = height / 4;

        ShuffleBlocks(data, blocksX, blocksY, BLOCK_SIZE, inverse);
    }

    private static void ShuffleBlocks(Span<byte> data, int bx, int by, int blockSize, bool inverse)
    {
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
        if (inverse)
        {
            CreateBlockMapInverse(map);
        }
        else
        {
            CreateBlockMap(map);
        }

        Span<byte> origData = stackalloc byte[groupBlocksIn2Rows * groupByteSize];

        for (var i = 0; i < totalGroupedBlocks; i++)
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
            Span<byte> newSpan = workingData[((currentWorkingIndex * groupByteSize) + newIndex)..(((currentWorkingIndex * groupByteSize) + newIndex) + groupByteSize)];

            // Copy data
            currentSpan.CopyTo(newSpan);
            //newSpan.CopyTo(currentSpan);
            //buffer.CopyTo(newSpan);
        }

        // Shuffle mip map textures...
        if (data.Length > workingData.Length && bx > 1 && by > 1)
        {
            var mipData = data[workingData.Length..];
            ShuffleBlocks(mipData, bx >> 1, by >> 1, blockSize, inverse);
        }
    }

    internal static void CreateBlockMap(Span<int> map)
    {
        var bx = map.Length;

        for (var i = 0; i < map.Length; i++)
        {
            map[i] = (i / 2) + ((i % 2) * (bx / 2));
        }
    }

    internal static void CreateBlockMapInverse(Span<int> map)
    {
        var halfSize = map.Length / 2;

        for (var i = 0; i < halfSize; i++)
        {
            map[i] = i * 2;
        }

        for (var i = halfSize; i < map.Length; i++)
        {
            map[i] = ((i % halfSize) * 2) + 1;
        }
    }

    public static void FixIndicies(Span<byte> data)
    {
        Span<byte> buffer = stackalloc byte[BLOCK_SIZE];

        for (int i = 0; i < data.Length; i += BLOCK_SIZE)
        {
            // Fix colors
            data[i..(i + 4)].CopyTo(buffer);
            data[i + 0] = buffer[1];
            data[i + 1] = buffer[0];
            data[i + 2] = buffer[3];
            data[i + 3] = buffer[2];

            // Fix indicies
            data[(i + 4)..(i + 8)].CopyTo(buffer);

            data[i + 4] = ReverseIndexRow(buffer[0]);
            data[i + 5] = ReverseIndexRow(buffer[1]);
            data[i + 6] = ReverseIndexRow(buffer[2]);
            data[i + 7] = ReverseIndexRow(buffer[3]);
        }
    }

    private static byte ReverseIndexRow(byte b)
    {
        return (byte)(((b & 0b00_00_00_11) << 6) | ((b & 0b00_00_11_00) << 2) | ((b & 0b00_11_00_00) >> 2) | ((b & 0b11_00_00_00) >> 6));
    }

    private static bool ShouldShuffleBlocks(int w, int h)
        => (w * h) >= 64; // 1 block of 16 pixels x 4 blocks per shuffle = 64 pixels
}