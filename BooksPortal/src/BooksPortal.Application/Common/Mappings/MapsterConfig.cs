using Mapster;

namespace BooksPortal.Application.Common.Mappings;

public static class MapsterConfig
{
    public static void Configure()
    {
        TypeAdapterConfig.GlobalSettings.Default
            .NameMatchingStrategy(NameMatchingStrategy.Flexible);
    }
}
