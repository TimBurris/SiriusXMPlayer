namespace Services.Abstractions;
public interface IWebViewCacheManager
{
    void DeleteCache(string cachePath);
}
