module Utils

open Microsoft.Extensions.DependencyInjection

module Settings =
    let requireOptions<'T when 'T: not struct> (section: string) (services: IServiceCollection) =
        services
            .AddOptions<'T>()
            .BindConfiguration(section)
            .ValidateDataAnnotations()
            .ValidateOnStart()
