using System;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Threading.Tasks;
using Godot;
using HttpClient = System.Net.Http.HttpClient;

namespace Nullun.Scripts.Utils;

public static class ChartDownloader
{
    private static readonly HttpClient http = new(new HttpClientHandler()
    {
        ServerCertificateCustomValidationCallback = (_, _, _, _) => true,
        CheckCertificateRevocationList = false,
        SslProtocols = System.Security.Authentication.SslProtocols.Tls12 | System.Security.Authentication.SslProtocols.Tls13
    });

    
    
    [Export]
    public static string ServerBaseUrl { get; set; } = "https://localhost:7107";

    // 将 zip 解压到 user://Chart/{folderName}
    public static async Task<bool> DownloadAndExtractAsync(string folderName)
    {
        if (string.IsNullOrWhiteSpace(folderName))
        {
            GD.PrintErr("folderName 不能为空");
            return false;
        }

        var url = $"{ServerBaseUrl.TrimEnd('/')}/api/chart/{Uri.EscapeDataString(folderName)}";
        GD.Print($"Downloading {url} ...");

        try
        {
            using var resp = await http.GetAsync(url);
            if (!resp.IsSuccessStatusCode)
            {
                GD.PrintErr($"Download failed: {resp.StatusCode}");
                return false;
            }

            var bytes = await resp.Content.ReadAsByteArrayAsync();

            // 将 user://Chart 转为绝对路径用于 System.IO 操作
            var userChartVirtual = $"user://Chart";
            var userChartAbs = ProjectSettings.GlobalizePath(userChartVirtual);
            Directory.CreateDirectory(userChartAbs);

            // 保存 zip 到 user://Chart/{folderName}.zip
            var zipPath = Path.Combine(userChartAbs, $"{folderName}.zip");
            await File.WriteAllBytesAsync(zipPath, bytes);
            GD.Print($"Saved zip to {zipPath}");

            // 解压到 user://Chart/{folderName}
            var extractDir = Path.Combine(userChartAbs, folderName);
            if (Directory.Exists(extractDir))
                Directory.Delete(extractDir, true);
            await Task.Run((() =>
            {
                ZipFile.ExtractToDirectory(zipPath, extractDir);
            }));
            
            GD.Print($"Extracted to {extractDir}");
            
            // // 可选：删除 zip 文件
            try { File.Delete(zipPath); } catch { /* ignore */ }

            return true;
        }
        catch (Exception ex)
        {
            GD.PrintErr($"Exception downloading/extracting chart: {ex}");
            return false;
        }
    }
}