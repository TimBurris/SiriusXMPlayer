using Microsoft.Extensions.Logging;
using System;
using System.IO;

namespace Services;
public class WebViewCacheManager : Abstractions.IWebViewCacheManager
{
    private readonly ILogger<WebViewCacheManager> _logger;

    public WebViewCacheManager(ILogger<WebViewCacheManager> logger)
    {
        _logger = logger;
    }
    public void DeleteCache(string cachePath)
    {
        if (string.IsNullOrEmpty(cachePath))
        {
            _logger.LogWarning("Cache folder not specified; this likely means a misconfiguration which will result in the cache folder continuing to grow", cachePath);
            return;
        }
        string fullCachePath = System.IO.Path.GetFullPath(cachePath);

        if (!Directory.Exists(fullCachePath))
        {
            _logger.LogWarning("Cache folder '{cachePath}' not found; this likely means a misconfiguration which will result in the cache folder continuing to grow", fullCachePath);
            return;
        }

        foreach (var filePath in Directory.GetFiles(fullCachePath))
        {
            try
            {
                System.IO.File.Delete(filePath);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Unable to remove cache file '{filePath}'", filePath);
            }
        }

        _logger.LogInformation("Completed cleaning of cache '{cachePath}'", fullCachePath);
    }
}
