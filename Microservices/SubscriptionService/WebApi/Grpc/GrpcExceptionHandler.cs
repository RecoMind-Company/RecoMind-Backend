using Grpc.Core;
using Grpc.Core.Interceptors;

public class GrpcExceptionHandler : Interceptor
{
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
        catch (Exception ex)
        {            
            throw new RpcException(
                new Status(StatusCode.Internal, ex.Message),
                new Metadata
                {
                    { "exception-type", ex.GetType().Name }
                }
            );
        }
    }
}

