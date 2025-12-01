using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.Extensions.Logging;

public class GlobalGrpcExceptionInterceptor : Interceptor
{
    private readonly ILogger<GlobalGrpcExceptionInterceptor> _logger;
    public GlobalGrpcExceptionInterceptor(ILogger<GlobalGrpcExceptionInterceptor> logger)
    {
        _logger = logger;
    }

    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        try
        {
            return await continuation(request, context);
        }
        catch (RpcException)
        {
            throw;
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Resource not found: {Message}", ex.Message);
            throw new RpcException(new Status(StatusCode.NotFound, "Resource not found."));
        }
        catch (ArgumentNullException ex)
        {
            _logger.LogWarning(ex, "Invalid argument: {Message}", ex.Message);
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid data provided."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled gRPC exception occurred.");
            throw new RpcException(new Status(StatusCode.Internal, "An internal server error occurred."));
        }
    }
}