using Yarp.ReverseProxy.Configuration;
using Yarp.ReverseProxy.Swagger;

namespace Gateway;

/// <summary>
/// Generates the Swagger/OpenAPI document filter config for YARP's Swagger extension.
/// This tells Scalar how to discover and proxy each service's openapi/v1.json.
/// 
/// Routes and clusters are loaded from gateway.yaml — this class ONLY handles the
/// Swagger metadata mapping.
/// </summary>
public static class YarpConfiguration
{
    /// <summary>
    /// Builds the ReverseProxyDocumentFilterConfig that maps each YARP cluster
    /// to its upstream OpenAPI document, exposed under /docs/{service}/openapi/v1.json.
    /// 
    /// How it works:
    ///   1. Each cluster has a Destination with an Address (e.g. "http+https://identity-api").
    ///   2. The Swagger extension discovers the OpenAPI doc at the destination by calling
    ///      {Address}/openapi/v1.json (or the configured Paths).
    ///   3. The PrefixPath determines where the doc is exposed on the gateway
    ///      (e.g. cluster "identity-cluster" → PrefixPath "/identity" → /docs/identity/openapi/v1.json).
    ///   4. Scalar UI (configured in Program.cs) maps each document to its gateway path.
    /// </summary>
    public static ReverseProxyDocumentFilterConfig GetSwaggerConfig()
    {
        var services = new[]
        {
            ("identity-cluster",    "identity"),
            ("membership-cluster",  "membership"),
            ("payment-cluster",     "payment"),
            ("courses-cluster",     "courses"),
            ("activity-cluster",    "activity"),
            ("aggregation-cluster", "aggregation"),
            ("chat-cluster",        "chat"),
        };

        return new ReverseProxyDocumentFilterConfig
        {
            Swagger = new()
            {
                CommonDocumentName = "FitTech-API",
                IsCommonDocument = true
            },
            Clusters = services.ToDictionary(
                s => s.Item1,
                s => new ReverseProxyDocumentFilterConfig.Cluster
                {
                    Destinations = new Dictionary<string, ReverseProxyDocumentFilterConfig.Cluster.Destination>
                    {
                        [$"destination-{s.Item1}"] = new()
                        {
                            Address = $"http+https://{s.Item1.Replace("-cluster", "-api")}",
                            Swaggers =
                            [
                                new()
                                {
                                    PrefixPath = $"/{s.Item2}",
                                    Paths = ["/openapi/v1.json"]
                                }
                            ]
                        }
                    }
                })
        };
    }
}
