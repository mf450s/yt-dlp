using System.IO.Abstractions.TestingHelpers;
using FluentAssertions;
using ytdlp.Services;

namespace ytdlp.Tests.Services;

public class ConfigsServicesTests
{
    [Fact]
    public void GetWholeConfigPath_ShouldReturnCorrectPath()
    {
        // Arrange
        var mockFileSystem = new MockFileSystem();
        var sut = new ConfigsServices(mockFileSystem);
        var configName = "testconfig";

        // Act
        var result = sut.GetWholeConfigPath(configName);

        // Assert
        result.Should().Be("../configs/testconfig.conf");
    }

    [Theory]
    [InlineData("music")]
    [InlineData("video")]
    [InlineData("playlist")]
    public void GetWholeConfigPath_WithDifferentNames_ShouldReturnCorrectPath(string configName)
    {
        // Arrange
        var mockFileSystem = new MockFileSystem();
        var sut = new ConfigsServices(mockFileSystem);

        // Act
        var result = sut.GetWholeConfigPath(configName);

        // Assert
        result.Should().Be($"../configs/{configName}.conf");
    }

    [Fact]
    public void GetAllConfigNames_WithMultipleConfigs_ShouldReturnAllNames()
    {
        // Arrange
        var mockFileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { "../configs/music.conf", new MockFileData("") },
            { "../configs/video.conf", new MockFileData("") },
            { "../configs/playlist.conf", new MockFileData("") }
        });
        var sut = new ConfigsServices(mockFileSystem);

        // Act
        var result = sut.GetAllConfigNames();

        // Assert
        result.Should().HaveCount(3);
        result.Should().Contain(new[] { "music", "video", "playlist" });
    }

    [Fact]
    public void GetAllConfigNames_WithNoConfigs_ShouldReturnEmptyList()
    {
        // Arrange
        var mockFileSystem = new MockFileSystem();
        mockFileSystem.AddDirectory("../configs/");
        var sut = new ConfigsServices(mockFileSystem);

        // Act
        var result = sut.GetAllConfigNames();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void GetAllConfigNames_WithMixedFiles_ShouldReturnOnlyConfFiles()
    {
        // Arrange
        var mockFileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { "../configs/music.conf", new MockFileData("") },
            { "../configs/readme.txt", new MockFileData("") },
            { "../configs/video.conf", new MockFileData("") },
            { "../configs/backup.bak", new MockFileData("") }
        });
        var sut = new ConfigsServices(mockFileSystem);

        // Act
        var result = sut.GetAllConfigNames();

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(new[] { "music", "video" });
        result.Should().NotContain("readme");
        result.Should().NotContain("backup");
    }

    [Fact]
    public void GetAllConfigNames_ShouldReturnNamesWithoutExtension()
    {
        // Arrange
        var mockFileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { "../configs/test.conf", new MockFileData("") }
        });
        var sut = new ConfigsServices(mockFileSystem);

        // Act
        var result = sut.GetAllConfigNames();

        // Assert
        result.Should().Contain("test");
        result.Should().NotContain("test.conf");
    }
}
