using Duende.AccessTokenManagement;
using FhirServerExporter;
using Hl7.Fhir.Rest;

static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .ConfigureAppConfiguration(
            (config) => config.AddYamlFile("queries.yaml", optional: true, reloadOnChange: true)
        )
        .ConfigureServices(
            (ctx, services) =>
            {
                var appConfig = new AppConfig();
                ctx.Configuration.Bind(appConfig);

                services.Configure<AuthConfig>(ctx.Configuration.GetSection("Auth"));
                services.Configure<AppConfig>(ctx.Configuration);

                services.AddSingleton<IAuthHeaderProvider, AuthHeaderProvider>();
                services.AddSingleton<IFhirResourceCounter>(sp =>
                {
                    if (!string.IsNullOrEmpty(appConfig.FhirServerUrl?.ToString()))
                    {
                        var authHeaderProvider = sp.GetRequiredService<IAuthHeaderProvider>();

                        var fhirClient = new FhirClient(
                            appConfig.FhirServerUrl,
                            settings: new()
                            {
                                Timeout = (int)appConfig.FhirServerTimeout.TotalMilliseconds,
                            }
                        );

                        return new FhirServerResourceCounter(fhirClient, authHeaderProvider);
                    }

                    if (!string.IsNullOrEmpty(appConfig.FhirLakehouse.DatabasePath))
                    {
                        return new LakehouseCounter(appConfig.FhirLakehouse);
                    }

                    throw new InvalidOperationException(
                        "Either the FHIR server url or the FHIR lakehouse database path needs to be set."
                    );
                });

                services.AddHostedService<FhirExporter>();

                // used by the oauth token provider
                services.AddDistributedMemoryCache();

                services
                    .AddClientCredentialsTokenManagement()
                    .AddClient(
                        AuthHeaderProvider.HttpClientName,
                        client =>
                        {
                            var authConfig = appConfig.Auth;

                            if (
                                authConfig.OAuth.TokenUrl is null
                                || authConfig.OAuth.ClientId is null
                                || authConfig.OAuth.ClientSecret is null
                            )
                            {
                                return;
                            }

                            client.TokenEndpoint = authConfig.OAuth.TokenUrl;
                            client.ClientId = ClientId.Parse(authConfig.OAuth.ClientId);
                            client.ClientSecret = ClientSecret.Parse(authConfig.OAuth.ClientSecret);

                            if (authConfig.OAuth.Scope is not null)
                            {
                                client.Scope = Scope.Parse(authConfig.OAuth.Scope);
                            }
                        }
                    );
            }
        )
        .ConfigureLogging(builder =>
            builder.AddSimpleConsole(options =>
            {
                options.UseUtcTimestamp = true;
                options.TimestampFormat = "yyyy-MM-ddTHH:mm:ssZ ";
                options.SingleLine = false;
            })
        );

await CreateHostBuilder(args).Build().RunAsync();
