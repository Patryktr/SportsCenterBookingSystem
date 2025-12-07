namespace SportsCenter.API.Extentions
{
    public interface IEndpointDefinition
    {
        void RegisterEndpoints(IEndpointRouteBuilder app);
    }
}
