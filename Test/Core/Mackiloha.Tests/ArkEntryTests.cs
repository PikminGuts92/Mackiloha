using Mackiloha.Ark;

namespace Mackiloha.Tests;

public class ArkEntryTests
{
    [Theory]
    [InlineData("test.dta", default(string))]
    [InlineData("test.dta", ".")]
    public void DirectoryIsRoot(string fileName, string dirPath)
    {
        // Setup
        var entry = new ArkEntry(fileName, dirPath);

        // Act
        var actualDir = entry.Directory;

        // Assert
        Assert.Equal(".", actualDir);
    }

    [Theory]
    [InlineData("test.dta", default(string), "./test.dta")]
    [InlineData("test.dta", ".", "./test.dta")]
    [InlineData("test.dta", "gen", "gen/test.dta")]
    public void FullPath(string fileName, string dirPath, string expectedFullPath)
    {
        // Setup
        var entry = new ArkEntry(fileName, dirPath);

        // Act
        var actualFullPath = entry.FullPath;

        // Assert
        Assert.Equal(expectedFullPath, actualFullPath);
    }
}