using System.IO;
using System;
using System.Threading.Tasks;
using Google.Cloud.Storage.V1;
using Google.Apis.Auth.OAuth2;
using UnityEngine;
using QRCoder;
using System.Collections;

public class UploadManager
{
    // 접근 URL의 베이스가 되는 부분
    private static string baseUrl = "https://storage.googleapis.com/kdream";

    private static string _bucketName = "kdream";

    private static StorageClient _storage;

    public static async Task InitAsync()
    {
        try
        {
            // 서비스 계정 키 파일 등록
            string credentialPath = Path.Combine(Application.streamingAssetsPath, "k-dream-xo-a3b409ae62b2.json");
            GoogleCredential credential = CredentialFactory.FromFile<ServiceAccountCredential>(credentialPath).ToGoogleCredential();

            credential = credential.CreateScoped("https://www.googleapis.com/auth/devstorage.full_control");

            _storage = await StorageClient.CreateAsync(credential);
        }
        catch (Exception e)
        {
            NLogManager.Error($"{e}");
        }
    }

    /// <summary>
    /// 로컬 경로에 있는 파일 비동기 업로드 시 사용.
    /// </summary>
    /// <param name="localPath">로컬 파일 경로</param>
    /// <param name="objectName">저장하고자 하는 저장소 폴더명을 포함한 이름</param>
    /// <param name="contentType">http content-type ex) image/png, video/mp4</param>
    public static async Task UploadFileAsync(string localPath, string objectName, string contentType)
    {
        try
        {
            using var fileStream = File.OpenRead(localPath);

            await _storage.UploadObjectAsync(new Google.Apis.Storage.v1.Data.Object
            {
                Bucket = _bucketName,
                Name = objectName,
                ContentType = contentType,
                CacheControl = "max-age=86400"  // 컨텐츠의 변화는 없으므로 하루 동안 캐시
            }, fileStream);

            NLogManager.Info($"Upload Completed: {baseUrl}/{objectName}");
        }
        catch (Exception ex)
        {
            NLogManager.Error($"{ex}");
        }
    }

    /// <summary>
    /// 바이트로 인코딩되어있는 파일 비동기 업로드 시 사용.
    /// </summary>
    /// <param name="localPath">로컬 파일 경로</param>
    /// <param name="objectName">저장하고자 하는 저장소 폴더명을 포함한 이름</param>
    /// <param name="contentType">http content-type ex) image/png, video/mp4</param>
    public static async Task UploadMemoryAsync(byte[] contentBytes, string objectName, string contentType)
    {
        try
        {
            using var memoryStream = new MemoryStream(contentBytes);

            await _storage.UploadObjectAsync(new Google.Apis.Storage.v1.Data.Object
            {
                Bucket = _bucketName,
                Name = objectName,
                ContentType = contentType,
                CacheControl = "max-age=86400"  // 컨텐츠의 변화는 없으므로 하루 동안 캐시
            }, memoryStream);

            NLogManager.Info($"Upload Completed: {baseUrl}/{objectName}");
        }
        catch (Exception ex)
        {
            NLogManager.Error($"{ex}");
        }
    }

    public static Texture2D GenerateQrCodeTexture(string url, int pixelsPerModule = 8)
    {
        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.L);

        var matrix = qrCodeData.ModuleMatrix;
        int moduleCount = matrix.Count;
        int textureSize = moduleCount * pixelsPerModule;

        Texture2D texture = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false);

        Color32 darkColor = new Color32(0, 0, 0, 255);
        Color32 lightColor = new Color32(255, 255, 255, 255);

        for (int y = 0; y < moduleCount; y++)
        {
            BitArray row = matrix[y];

            for (int x = 0; x < moduleCount; x++)
            {
                bool isDark = row[x];
                Color32 color = isDark ? darkColor : lightColor;

                for (int py = 0; py < pixelsPerModule; py++)
                {
                    for (int px = 0; px < pixelsPerModule; px++)
                    {
                        int tx = x * pixelsPerModule + px;
                        int ty = textureSize - 1 - (y * pixelsPerModule + py);
                        texture.SetPixel(tx, ty, color);
                    }
                }
            }
        }

        texture.Apply();
        return texture;
    }
}