using MediatR;

namespace ChatApp.Application.Features.Hello.Queries
{
    public class GetHelloQueries : IRequest<string>
    {
    }
    public class GetHelloQueriesHandler : IRequestHandler<GetHelloQueries, string>
    {
        public Task<string> Handle(GetHelloQueries request, CancellationToken cancellationToken)
        {
            return Task.FromResult("Xin chào! Workflow STALKchat Clean Architecture đã thông suốt! 🚀");        
        }
    }   
}
