using System.Text.Json;
using AvMusic.Synology.Dto;
using Xunit;
using AvMusic.Synology.Json;
using AvMusic.Synology.Mapping;

namespace AvMusic.Synology.Tests;

public class DtoDeserializationTests
{
    private static string GetFixturePath(string fileName)
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            var candidate = Path.Combine(dir.FullName, "ApiDocuments", "Response", fileName);
            if (File.Exists(candidate))
            {
                return candidate;
            }

            dir = dir.Parent;
        }

        throw new FileNotFoundException($"找不到 ApiDocuments 样例: {fileName}");
    }

    [Theory]
    [InlineData("04 - 所有音乐.json")]
    [InlineData("06 - 专辑.json")]
    [InlineData("07 - 歌手.json")]
    [InlineData("09 - 类型.json")]
    [InlineData("10 - 播放列表.json")]
    [InlineData("21 - 搜索结果.json")]
    public void Deserialize_fixture_response(string fileName)
    {
        var path = GetFixturePath(fileName);
        var json = File.ReadAllText(path);
        Assert.Contains("\"success\": true", json);

        switch (fileName)
        {
            case "04 - 所有音乐.json":
            {
                var response = JsonSerializer.Deserialize<SynologyResponse<SongListDataDto>>(json, SynologyJsonDefaults.Options);
                Assert.NotNull(response?.Data);
                Assert.True(response.Data.Songs.Count > 0);
                Assert.True(response.Data.Total > 0);
                var song = EntityMapper.ToSong(response.Data.Songs[0]);
                Assert.False(string.IsNullOrEmpty(song.Id));
                break;
            }
            case "06 - 专辑.json":
            {
                var response = JsonSerializer.Deserialize<SynologyResponse<AlbumListDataDto>>(json, SynologyJsonDefaults.Options);
                Assert.NotNull(response?.Data);
                Assert.True(response.Data.Albums.Count > 0);
                break;
            }
            case "07 - 歌手.json":
            {
                var response = JsonSerializer.Deserialize<SynologyResponse<ArtistListDataDto>>(json, SynologyJsonDefaults.Options);
                Assert.NotNull(response?.Data);
                Assert.True(response.Data.Artists.Count > 0);
                break;
            }
            case "09 - 类型.json":
            {
                var response = JsonSerializer.Deserialize<SynologyResponse<GenreListDataDto>>(json, SynologyJsonDefaults.Options);
                Assert.NotNull(response?.Data);
                Assert.True(response.Data.Genres.Count > 0);
                break;
            }
            case "10 - 播放列表.json":
            {
                var response = JsonSerializer.Deserialize<SynologyResponse<PlaylistListDataDto>>(json, SynologyJsonDefaults.Options);
                Assert.NotNull(response?.Data);
                Assert.True(response.Data.Playlists.Count > 0);
                break;
            }
            case "21 - 搜索结果.json":
            {
                var response = JsonSerializer.Deserialize<SynologyResponse<SearchDataDto>>(json, SynologyJsonDefaults.Options);
                Assert.NotNull(response?.Data);
                var result = EntityMapper.ToSearchResult(response.Data);
                Assert.True(result.SongTotal > 0);
                break;
            }
        }
    }
}
