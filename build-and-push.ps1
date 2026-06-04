# FitTech Build and Push Script
# Ensure Minikube registry port-forward is running: 
# kubectl port-forward --namespace kube-system service/registry 5000:80

Write-Host "Starting Build and Push for .NET Services..." -ForegroundColor Cyan

# .NET Services
$netServices = @(
    @{ name="identity-api"; dockerfile="FitTech.AppHost\Dockerfiles\identity-api.Dockerfile" },
    @{ name="membership-api"; dockerfile="FitTech.AppHost\Dockerfiles\membership-api.Dockerfile" },
    @{ name="payment-api"; dockerfile="FitTech.AppHost\Dockerfiles\payment-api.Dockerfile" },
    @{ name="courses-api"; dockerfile="FitTech.AppHost\Dockerfiles\courses-api.Dockerfile" },
    @{ name="activity-api"; dockerfile="FitTech.AppHost\Dockerfiles\activity-api.Dockerfile" },
    @{ name="aggregation-api"; dockerfile="FitTech.AppHost\Dockerfiles\aggregation-api.Dockerfile" },
    @{ name="chat-api"; dockerfile="FitTech.AppHost\Dockerfiles\chat-api.Dockerfile" },
    @{ name="notification-api"; dockerfile="FitTech.AppHost\Dockerfiles\notification-api.Dockerfile" },
    @{ name="gateway"; dockerfile="FitTech.AppHost\Dockerfiles\gateway.Dockerfile" }
)

foreach ($svc in $netServices) {
    Write-Host "Building and Pushing: $($svc.name)..." -ForegroundColor Yellow
    docker build -t "localhost:5000/$($svc.name):latest" -f $svc.dockerfile .
    docker push "localhost:5000/$($svc.name):latest"
}

Write-Host "Starting Build and Push for Java Services..." -ForegroundColor Cyan

# Java Services
$javaServices = @(
    @{ name="shop-api"; path="Services/Shop/" },
    @{ name="workout-logs-api"; path="Services/Workout-Logs/" },
    @{ name="equipments-api"; path="Services/Equipments/" }
)

foreach ($svc in $javaServices) {
    Write-Host "Building and Pushing: $($svc.name)..." -ForegroundColor Yellow
    docker build -t "localhost:5000/$($svc.name):latest" $svc.path
    docker push "localhost:5000/$($svc.name):latest"
}

Write-Host "Verifying Registry Catalog..." -ForegroundColor Cyan
curl http://localhost:5000/v2/_catalog

Write-Host "Phase 3 Complete!" -ForegroundColor Green
