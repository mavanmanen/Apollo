using Microsoft.AspNetCore.Builder;

namespace Apollo.Orchestration.API.Routes;

internal abstract class RoutesBase
{
    public void Map(WebApplication app)
    {
        MapGet(app);
        MapPost(app);
        MapPut(app);
        MapDelete(app);
    }

    protected virtual void MapGet(WebApplication app)
    {
    }

    protected virtual void MapPost(WebApplication app)
    {
    }

    protected virtual void MapPut(WebApplication app)
    {
    }

    protected virtual void MapDelete(WebApplication app)
    {
    }
}