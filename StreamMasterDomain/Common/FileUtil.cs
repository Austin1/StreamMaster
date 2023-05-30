using System.IO.Compression;
using System.Text;
using System.Text.Json;
using System.Web;

namespace StreamMasterDomain.Common;

public sealed class FileUtil
{
    private static bool setupDirectories = false;

    public static void CreateDirectory(string fileName)
    {
        string? directory = Path.EndsInDirectorySeparator(fileName) ? fileName : Path.GetDirectoryName(fileName);
        if (directory == null || string.IsNullOrEmpty(directory))
        {
            return;
        }

        if (
            !directory.ToLower().StartsWith(Constants.ConfigFolder.ToLower()) &&
            !$"{directory.ToLower()}{Path.DirectorySeparatorChar}".StartsWith(Constants.ConfigFolder.ToLower())
            )
        {
            throw new Exception($"Illegal directory outside of {Constants.ConfigFolder} : {directory}");
        }

        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            _ = Directory.CreateDirectory(directory);
        }
    }

    public static async Task<(bool success, Exception? ex)> DownloadUrlAsync(string url, string fullName, CancellationToken cancellationdefault)
    {
        if (url == null || !url.Contains("://"))
        {
            return (false, null);
        }

        try
        {
            using HttpClient httpClient = new();

            string userAgentString = @"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/110.0.0.0 Safari/537.36 Edg/110.0.1587.57";

            httpClient.DefaultRequestHeaders.Add("User-Agent", userAgentString);

            try
            {
                string? path = Path.GetDirectoryName(fullName);
                if (string.IsNullOrEmpty(path))
                {
                    return (false, null);
                }
                using FileStream fileStream = new(fullName, FileMode.Create);
                using HttpResponseMessage response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationdefault).ConfigureAwait(false);

                _ = response.EnsureSuccessStatusCode();

                Stream stream = await response.Content.ReadAsStreamAsync(cancellationdefault).ConfigureAwait(false);

                if (stream != null)
                {
                    await stream.CopyToAsync(fileStream, cancellationdefault).ConfigureAwait(false);
                    stream.Close();

                    string txtName = Path.Combine(path, Path.GetFileNameWithoutExtension(fullName) + ".url");
                    File.WriteAllText(txtName, url);
                }
                fileStream.Close();

                return (true, null);
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine(ex.ToString());
                return (false, ex);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return (false, ex);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
            return (false, ex);
        }
    }

    public static async Task<string> GetContentType(string Url)
    {
        try
        {
            using HttpClient httpClient = new();
            HttpResponseMessage response = await httpClient.GetAsync(Url, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                return "";
            }

            string sContentType = response.Content.Headers.ContentType?.MediaType ?? "";
            return sContentType;
        }
        catch (Exception)
        {
            return "";
        }
    }

    public static async Task<string> GetFileData(string source)
    {
        string body = "";
        try
        {
            using FileStream fs = File.Open(source, FileMode.Open);
            using GZipStream gzStream = new(fs, CompressionMode.Decompress);
            using MemoryStream outputStream = new();
            gzStream.CopyTo(outputStream);
            byte[] outputBytes = outputStream.ToArray();
            body = Encoding.ASCII.GetString(outputBytes);
            return body;
        }
        catch (Exception ex)
        {
            if (!ex.ToString().Contains("The archive entry was compressed using an unsupported compression method."))
            {
                Console.WriteLine(ex.ToString());
                return "";
            }
        }

        try
        {
            body = await File.ReadAllTextAsync(source).ConfigureAwait(false);
        }
        catch (Exception)
        {
        }
        return body;
    }

    public static async Task<List<TvLogoFile>> GetIconFilesFromDirectory(DirectoryInfo dirInfo, string tvLogosLocation, int startingId, CancellationToken cancellationToken = default)
    {
        Setting setting = FileUtil.GetSetting();
        List<TvLogoFile> ret = new();

        foreach (FileInfo file in dirInfo.GetFiles("*png"))
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            string basename = dirInfo.FullName.Replace(tvLogosLocation, "");
            if (basename.StartsWith(Path.DirectorySeparatorChar))
            {
                basename = basename.Remove(0, 1);
            }

            basename = basename.Replace(Path.DirectorySeparatorChar, '-');
            string name = $"{basename}-{file.Name}";

            TvLogoFile tvLogo = new()
            {
                Id= startingId++,
                Name = Path.GetFileNameWithoutExtension(name),
                FileExists = true,
                ContentType = "image/png",
                LastDownloaded = DateTime.Now,
                Source = $"api/files/{(int)SMFileTypes.TvLogo}/{HttpUtility.UrlEncode(name)}",
                OriginalSource = file.FullName,
                Url = $"{setting.BaseHostURL}api/files/{(int)SMFileTypes.TvLogo}/{HttpUtility.UrlEncode(name)}",
            };

            tvLogo.SetFileDefinition(FileDefinitions.TVLogo);
            tvLogo.FileExtension = ".png";
            ret.Add(tvLogo);
        }

        foreach (DirectoryInfo newDir in dirInfo.GetDirectories())
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }
            List<TvLogoFile> files = await GetIconFilesFromDirectory(newDir, tvLogosLocation, startingId,cancellationToken).ConfigureAwait(false);
            ret = ret.Concat(files).ToList();
        }

        return ret;
    }

    public static Setting GetSetting()
    {
        string jsonString;
        Setting? ret;

        if (File.Exists(Constants.SettingFile))
        {
            jsonString = File.ReadAllText(Constants.SettingFile);
            ret = JsonSerializer.Deserialize<Setting>(jsonString);
            if (ret != null)
            {
                return ret;
            }
        }

        ret = new Setting();
        UpdateSetting(ret);

        return ret;
    }

    public static void SetupDirectories(bool alwaysRun = false)
    {
        if (setupDirectories && !alwaysRun)
        {
            return;
        }
        setupDirectories = true;
        foreach (System.Reflection.FieldInfo field in typeof(Constants).GetFields())
        {
            object? value = field.GetValue(null);

            if (
                value is not null &&
                value is string &&
                value.ToString() is not null &&
                value.ToString()!.StartsWith(Constants.ConfigFolder))
            {
                Console.WriteLine($"Creaing directory for {value}");
                FileUtil.CreateDirectory(value.ToString()!);
            }
        }
    }

    public static void UpdateSetting(Setting setting)
    {
        if (!Directory.Exists(Constants.ConfigFolder))
        {
            _ = Directory.CreateDirectory(Constants.ConfigFolder);
        }
        string jsonString = JsonSerializer.Serialize(setting, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(Constants.SettingFile, jsonString);
    }
}