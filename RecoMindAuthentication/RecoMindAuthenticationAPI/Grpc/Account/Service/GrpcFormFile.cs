
namespace RecoMindAuthenticationAPI.Grpc.Account.Service;

public class GrpcFormFile : IFormFile
{
    private readonly byte[] _content;
    private readonly string _fileName;
    public GrpcFormFile(byte[] content, string fileName)
    {
        _content = content;
        _fileName = fileName;
    }
    public long Length => _content.Length;
    public string FileName => _fileName;

    #region Not Implemented
    public string ContentType => throw new NotImplementedException();

    public string ContentDisposition => throw new NotImplementedException();

    public IHeaderDictionary Headers => throw new NotImplementedException();
    public string Name => throw new NotImplementedException();
    #endregion

    public Stream OpenReadStream() => new MemoryStream(_content);
    public void CopyTo(Stream target) => OpenReadStream().CopyTo(target);
    public async Task CopyToAsync(Stream target, CancellationToken cancellationToken = default)
    {
        await OpenReadStream().CopyToAsync(target, cancellationToken);
    }

}
