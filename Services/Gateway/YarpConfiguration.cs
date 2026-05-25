using Yarp.ReverseProxy.Configuration;
using Yarp.ReverseProxy.Swagger;

namespace Gateway;

public class YarpConfiguration
{
    public static (List<RouteConfig>, List<ClusterConfig>, ReverseProxyDocumentFilterConfig) GetYarpConfiguration()
    {
        var services = new List<(string aspireServiceName, string name)>
        {
            ("identity-api",   "User"),
            ("membership-api", "members"),
            ("payment-api",    "payments"),
            ("chat-api",       "conversations"),
        };

        var sockets = new List<(string aspireServiceName, string name)>
        {
            ("chat-api", "hubs/chat"),
        };

        var serviceRoutes  = GenerateRoutes(services, isSocket: false);
        var socketRoutes   = GenerateRoutes(sockets,  isSocket: true);
        var routes         = serviceRoutes.Concat(socketRoutes).ToList();

        var serviceClusters = GenerateClusters(services, isSocket: false);
        var socketClusters  = GenerateClusters(sockets,  isSocket: true);
        var clusters        = serviceClusters.Concat(socketClusters).ToList();

        var swaggerConfig = new ReverseProxyDocumentFilterConfig
        {
            Swagger = new() { CommonDocumentName = "FitTech-API", IsCommonDocument = true },
            Routes  = routes.ToDictionary(_ => _.RouteId, _ => _),
            Clusters = clusters
                .Where(c => !c.ClusterId.Contains("socket"))
                .Select(cluster => new KeyValuePair<string, ReverseProxyDocumentFilterConfig.Cluster>(
                    cluster.ClusterId,
                    new()
                    {
                        Destinations = cluster.Destinations?.Select(destination =>
                            new KeyValuePair<string, ReverseProxyDocumentFilterConfig.Cluster.Destination>(
                                destination.Key,
                                new()
                                {
                                    Address = destination.Value.Address,
                                    Swaggers =
                                    [
                                        new()
                                        {
                                            PrefixPath = string.Concat("/", cluster.ClusterId.AsSpan("cluster-".Length)),
                                            Paths = ["/openapi/v1.json"]
                                        }
                                    ]
                                }
                            )).ToDictionary()
                    }
                )).ToDictionary()
        };

        return (routes, clusters, swaggerConfig);
    }

    private static List<RouteConfig> GenerateRoutes(
        List<(string aspireServiceName, string name)> services, bool isSocket)
    {
        return services.GroupBy(x => x.name).Select(g =>
        {
            var service = g.First();
            return new RouteConfig
            {
                ClusterId = isSocket ? $"cluster-socket-{service.name}" : $"cluster-{service.name}",
                RouteId   = isSocket ? $"route-socket-{service.name}"  : $"route-{service.name}",
                Match = new RouteMatch
                {
                    Path = isSocket
                        ? $"/{service.name}/{{**catch-all}}"
                        : $"/api/{service.name}/{{**catch-all}}"
                },
                Transforms =
                [
                    new Dictionary<string, string>
                    {
                        { "PathPattern", isSocket
                            ? $"/{service.name}/{{**catch-all}}"
                            : "/api/{**catch-all}" }
                    }
                ],
                CorsPolicy = isSocket ? "socketPolicy" : "default"
            };
        }).ToList();
    }

    private static List<ClusterConfig> GenerateClusters(
        List<(string aspireServiceName, string name)> services, bool isSocket)
    {
        return services.GroupBy(x => x.name).Select(g =>
        {
            var first  = g.First();
            var prefix = isSocket ? "cluster-socket" : "cluster";
            return new ClusterConfig
            {
                ClusterId = $"{prefix}-{first.name}",
                Destinations = g.Select(s =>
                    new KeyValuePair<string, DestinationConfig>(
                        $"destination-{s.aspireServiceName}",
                        new DestinationConfig { Address = $"http://{s.aspireServiceName}/" }
                    )).ToDictionary()
            };
        }).ToList();
    }
}