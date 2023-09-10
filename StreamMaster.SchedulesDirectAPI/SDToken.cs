﻿using StreamMaster.SchedulesDirectAPI.Models;

using StreamMasterDomain.Common;

using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace StreamMaster.SchedulesDirectAPI;

public class SDTokenFile
{
    public string? Token { get; set; }
    public DateTime TokenDateTime { get; set; }
}

public static class SDToken
{
    private static (SDStatus? status, DateTime timestamp)? cacheEntry = null;
    private const string SD_BASE_URL = "https://json.schedulesdirect.org/20141201/";
    private static readonly HttpClient httpClient = CreateHttpClient();
    private static readonly string SD_TOKEN_FILENAME = Path.Combine(BuildInfo.AppDataFolder, "sd_token.json");
    private static string? token;
    private static DateTime tokenDateTime;

    static SDToken()
    {
        LoadToken();
    }



    public static async Task<SDStatus?> GetStatus(CancellationToken cancellationToken)
    {
        (SDStatus? status, bool resetToken) = await GetStatusInternal(cancellationToken);
        if (resetToken)
        {
            if (await ResetToken().ConfigureAwait(false) == null)
            {
                return GetSDStatusOffline();
            }
            return status;
        }

        return status;
    }

    public static async Task<(SDStatus? sdStatus, bool resetToken)> GetStatusInternal(CancellationToken cancellationToken)
    {
        if (cacheEntry.HasValue && (DateTime.UtcNow - cacheEntry.Value.timestamp).TotalMinutes < 10)
        {
            return (cacheEntry.Value.status, false);
        }

        string url = await GetAPIUrl("status", cancellationToken);

        using HttpResponseMessage response = await httpClient.GetAsync(url, cancellationToken).ConfigureAwait(false);
        try
        {
            if (response.StatusCode == HttpStatusCode.Forbidden || response.StatusCode == HttpStatusCode.NotFound)
            {
                return (null, true);
            }

            _ = response.EnsureSuccessStatusCode();
            string responseString = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            if (responseString == null)
            {
                return (null, false);
            }
            SDStatus? result = JsonSerializer.Deserialize<SDStatus>(responseString);
            if (result == null)
            {
                return (null, false);
            }
            cacheEntry = (result, DateTime.UtcNow);
            return (result, false);
        }
        catch (Exception ex)
        {
            return (null, false);
        }
    }

    public static async Task<bool> GetSystemReady(CancellationToken cancellationToken)
    {
        (SDStatus? status, bool resetToken) = await GetStatusInternal(cancellationToken);
        if (resetToken && await ResetToken().ConfigureAwait(false) != null)
        {
            (status, _) = await GetStatusInternal(cancellationToken);
        }

        return status?.systemStatus[0].status?.ToLower() == "online";
    }

    public static async Task<string?> GetToken(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(token) || tokenDateTime.AddHours(23) < DateTime.Now)
        {
            token = await RetrieveToken(cancellationToken);

            if (string.IsNullOrEmpty(token))
            {
                throw new ApplicationException("Unable to get token");
            }

            tokenDateTime = DateTime.Now;
            SaveToken();
        }
        return token;
    }

    public static async Task<string?> ResetToken(CancellationToken cancellationToken = default)
    {
        token = null;

        return await GetToken(cancellationToken);
    }

    private static HttpClient CreateHttpClient()
    {
        Setting setting = FileUtil.GetSetting();
        HttpClient client = new(new HttpClientHandler()
        {
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
            AllowAutoRedirect = true,
        });
        client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
        client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));
        client.DefaultRequestHeaders.UserAgent.ParseAdd(setting.ClientUserAgent);

        return client;
    }

    public static async Task<string> GetAPIUrl(string command, CancellationToken cancellationToken)
    {
        string? token = await GetToken(cancellationToken).ConfigureAwait(false);
        return ConstructAPIUrlWithToken(command, token);
    }

    private static string ConstructAPIUrlWithToken(string command, string? token)
    {
        if (command.Contains("?"))
        {
            return $"{SD_BASE_URL}{command}&token={token}";
        }
        return $"{SD_BASE_URL}{command}?token={token}";
    }

    private static SDStatus GetSDStatusOffline()
    {
        SDStatus ret = new();
        ret.systemStatus.Add(new SDSystemstatus { status = "Offline" });
        return ret;
    }

    private static void LoadToken()
    {
        if (!File.Exists(SD_TOKEN_FILENAME))
        {
            token = null;
            return;
        }

        string jsonString = File.ReadAllText(SD_TOKEN_FILENAME);
        SDTokenFile? result = JsonSerializer.Deserialize<SDTokenFile>(jsonString)!;
        if (result is null)
        {
            token = null;
            return;
        }

        token = result.Token;
        tokenDateTime = result.TokenDateTime;
    }

    private static async Task<string?> RetrieveToken(CancellationToken cancellationToken)
    {
        Setting setting = FileUtil.GetSetting();

        string? sdHashedPassword;
        if (HashHelper.TestSha1HexHash(setting.SDPassword))
        {
            sdHashedPassword = setting.SDPassword;
        }
        else
        {
            sdHashedPassword = setting.SDPassword.GetSHA1Hash();
        }

        if (string.IsNullOrEmpty(sdHashedPassword))
        {
            return null;
        }

        SDGetTokenRequest data = new()
        {
            username = setting.SDUserName,
            password = sdHashedPassword
        };

        string jsonString = JsonSerializer.Serialize(data);
        StringContent content = new(jsonString, Encoding.UTF8, "application/json");

        using HttpResponseMessage response = await httpClient.PostAsync($"{SD_BASE_URL}token", content, cancellationToken).ConfigureAwait(false);
        try
        {
            _ = response.EnsureSuccessStatusCode();
            string responseString = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            SDGetToken? result = JsonSerializer.Deserialize<SDGetToken>(responseString);
            if (result == null || string.IsNullOrEmpty(result.token))
            {
                return null;
            }
            token = result.token;
            return token;
        }
        catch (Exception ex)
        {
            Console.WriteLine("ERROR: ", ex);
            return null;
        }
    }

    private static void SaveToken()
    {
        if (token is null)
        {
            return;
        }

        string jsonString = JsonSerializer.Serialize(new SDTokenFile { Token = token, TokenDateTime = tokenDateTime });
        lock (typeof(SDToken))
        {
            File.WriteAllText(SD_TOKEN_FILENAME, jsonString);
        }
    }
}
