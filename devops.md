# FitTech Backend — Complete Kubernetes Deployment on Minikube (Windows)
## Updated Plan: Redis, gRPC, StatefulSets, PV/PVC, Secrets/ConfigMaps

---

## Stack Summary (Updated)

| Service | Type | HTTP Port | gRPC Port | DB |
|---|---|---|---|---|
| `identity-api` | .NET 10 | 5051 | 8082 | identity-db |
| `membership-api` | .NET 10 | 5121 | — | membershipDb + Redis |
| `payment-api` | .NET 10 | 5246 | — | paymentDb |
| `courses-api` | .NET 10 | 5101 | — | coursesDb |
| `activity-api` | .NET 10 | 5102 | — | activityDb |
| `aggregation-api` | .NET 10 | 5103 | — | aggregationDb |
| `chat-api` | .NET 10 | 5274 | — | chatDb |
| `notification-api` | .NET 10 | 80 | — | — |
| `gateway` | .NET 10 YARP | 5098 | — | — |
| `shop-api` | Spring Boot 4 / Java 21 | 5104 | — | shopDb |
| `workout-logs-api` | Spring Boot 4 / Java 21 | 5105 | — | workoutLogsDb |
| `equipments-api` | Spring Boot 4 / Java 21 | 5106 | — | equipmentsDb |
| `mainDbServer` | PostgreSQL 16 StatefulSet | 5432 | — | all 10 DBs |
| `rabbitmq` | RabbitMQ 3 StatefulSet | 5672/15672 | — | — |
| `membership-cache` | Redis 7 StatefulSet | 6379 | — | — |
| `smtp4dev` | SMTP dev server | 2525/5005 | — | — |

---

## PHASE 1 — Dockerfiles

### 1.1 Create Dockerfiles directory

```powershell
mkdir D:\projects\GYM\FitTech.Backend\FitTech.AppHost\Dockerfiles
```

### 1.2 .NET Dockerfiles

Build context for all .NET services is the **solution root** `D:\projects\GYM\FitTech.Backend`.
The `NuGet.Config` override inside the build stage bypasses the revoked Refit 10.1.6 certificate.

**`FitTech.AppHost\Dockerfiles\identity-api.Dockerfile`**
```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
RUN mkdir -p /root/.nuget/NuGet && \
    printf '<?xml version="1.0" encoding="utf-8"?>\n<configuration>\n  <config>\n    <add key="signatureValidationMode" value="accept" />\n  </config>\n</configuration>' \
    > /root/.nuget/NuGet/NuGet.Config
COPY . .
RUN dotnet restore "Services/Identity/Identity.Api/Identity.Api.csproj"
RUN dotnet publish "Services/Identity/Identity.Api/Identity.Api.csproj" \
    -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 5051
EXPOSE 8082
ENTRYPOINT ["dotnet", "Identity.Api.dll"]
```

> Identity exposes both HTTP (5051) and gRPC (8082). Kestrel reads its endpoint config from
> `appsettings.Production.json` which is created in Phase 2. Do NOT set `ASPNETCORE_URLS`
> for identity-api — Kestrel multi-endpoint config overrides it.

**`FitTech.AppHost\Dockerfiles\membership-api.Dockerfile`**
```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
RUN mkdir -p /root/.nuget/NuGet && \
    printf '<?xml version="1.0" encoding="utf-8"?>\n<configuration>\n  <config>\n    <add key="signatureValidationMode" value="accept" />\n  </config>\n</configuration>' \
    > /root/.nuget/NuGet/NuGet.Config
COPY . .
RUN dotnet restore "Services/Membership/Membership.csproj"
RUN dotnet publish "Services/Membership/Membership.csproj" \
    -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:5121
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 5121
ENTRYPOINT ["dotnet", "Membership.dll"]
```

**`FitTech.AppHost\Dockerfiles\payment-api.Dockerfile`**
```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
RUN mkdir -p /root/.nuget/NuGet && \
    printf '<?xml version="1.0" encoding="utf-8"?>\n<configuration>\n  <config>\n    <add key="signatureValidationMode" value="accept" />\n  </config>\n</configuration>' \
    > /root/.nuget/NuGet/NuGet.Config
COPY . .
RUN dotnet restore "Services/Payment/Payment.csproj"
RUN dotnet publish "Services/Payment/Payment.csproj" \
    -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:5246
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 5246
ENTRYPOINT ["dotnet", "Payment.dll"]
```

**`FitTech.AppHost\Dockerfiles\courses-api.Dockerfile`**
```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
RUN mkdir -p /root/.nuget/NuGet && \
    printf '<?xml version="1.0" encoding="utf-8"?>\n<configuration>\n  <config>\n    <add key="signatureValidationMode" value="accept" />\n  </config>\n</configuration>' \
    > /root/.nuget/NuGet/NuGet.Config
COPY . .
RUN dotnet restore "Services/Courses/Courses.csproj"
RUN dotnet publish "Services/Courses/Courses.csproj" \
    -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:5101
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 5101
ENTRYPOINT ["dotnet", "Courses.dll"]
```

**`FitTech.AppHost\Dockerfiles\activity-api.Dockerfile`**
```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
RUN mkdir -p /root/.nuget/NuGet && \
    printf '<?xml version="1.0" encoding="utf-8"?>\n<configuration>\n  <config>\n    <add key="signatureValidationMode" value="accept" />\n  </config>\n</configuration>' \
    > /root/.nuget/NuGet/NuGet.Config
COPY . .
RUN dotnet restore "Services/Activity/Activity.csproj"
RUN dotnet publish "Services/Activity/Activity.csproj" \
    -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:5102
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 5102
ENTRYPOINT ["dotnet", "Activity.dll"]
```

**`FitTech.AppHost\Dockerfiles\aggregation-api.Dockerfile`**
```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
RUN mkdir -p /root/.nuget/NuGet && \
    printf '<?xml version="1.0" encoding="utf-8"?>\n<configuration>\n  <config>\n    <add key="signatureValidationMode" value="accept" />\n  </config>\n</configuration>' \
    > /root/.nuget/NuGet/NuGet.Config
COPY . .
RUN dotnet restore "Services/Aggregation/Aggregation.csproj"
RUN dotnet publish "Services/Aggregation/Aggregation.csproj" \
    -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:5103
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 5103
ENTRYPOINT ["dotnet", "Aggregation.dll"]
```

**`FitTech.AppHost\Dockerfiles\chat-api.Dockerfile`**
```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
RUN mkdir -p /root/.nuget/NuGet && \
    printf '<?xml version="1.0" encoding="utf-8"?>\n<configuration>\n  <config>\n    <add key="signatureValidationMode" value="accept" />\n  </config>\n</configuration>' \
    > /root/.nuget/NuGet/NuGet.Config
COPY . .
RUN dotnet restore "Services/Chat/Chat.csproj"
RUN dotnet publish "Services/Chat/Chat.csproj" \
    -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:5274
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 5274
ENTRYPOINT ["dotnet", "Chat.dll"]
```

**`FitTech.AppHost\Dockerfiles\notification-api.Dockerfile`**
```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
RUN mkdir -p /root/.nuget/NuGet && \
    printf '<?xml version="1.0" encoding="utf-8"?>\n<configuration>\n  <config>\n    <add key="signatureValidationMode" value="accept" />\n  </config>\n</configuration>' \
    > /root/.nuget/NuGet/NuGet.Config
COPY . .
RUN dotnet restore "Services/Notification/Notification.Api/Notification.Api.csproj"
RUN dotnet publish "Services/Notification/Notification.Api/Notification.Api.csproj" \
    -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 80
ENTRYPOINT ["dotnet", "Notification.Api.dll"]
```

**`FitTech.AppHost\Dockerfiles\gateway.Dockerfile`**
```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
RUN mkdir -p /root/.nuget/NuGet && \
    printf '<?xml version="1.0" encoding="utf-8"?>\n<configuration>\n  <config>\n    <add key="signatureValidationMode" value="accept" />\n  </config>\n</configuration>' \
    > /root/.nuget/NuGet/NuGet.Config
COPY . .
RUN dotnet restore "Services/Gateway/Gateway.csproj"
RUN dotnet publish "Services/Gateway/Gateway.csproj" \
    -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:5098
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 5098
ENTRYPOINT ["dotnet", "Gateway.dll"]
```

### 1.3 Java Dockerfiles

Each Java service uses its own directory as build context.
`<java.version>` in `pom.xml` must be `21` — change it in all three before building.

**`Services/Shop/Dockerfile`**
```dockerfile
FROM maven:3.9-amazoncorretto-21 AS build
WORKDIR /app
COPY pom.xml .
RUN mvn dependency:go-offline -B
COPY src ./src
RUN mvn package -DskipTests -B

FROM amazoncorretto:21-alpine
WORKDIR /app
COPY --from=build /app/target/app.jar app.jar
EXPOSE 5104
ENTRYPOINT ["java", "-jar", "app.jar"]
```

**`Services/Workout-Logs/Dockerfile`**
```dockerfile
FROM maven:3.9-amazoncorretto-21 AS build
WORKDIR /app
COPY pom.xml .
RUN mvn dependency:go-offline -B
COPY src ./src
RUN mvn package -DskipTests -B

FROM amazoncorretto:21-alpine
WORKDIR /app
COPY --from=build /app/target/app.jar app.jar
EXPOSE 5105
ENTRYPOINT ["java", "-jar", "app.jar"]
```

**`Services/Equipments/Dockerfile`**
```dockerfile
FROM maven:3.9-amazoncorretto-21 AS build
WORKDIR /app
COPY pom.xml .
RUN mvn dependency:go-offline -B
COPY src ./src
RUN mvn package -DskipTests -B

FROM amazoncorretto:21-alpine
WORKDIR /app
COPY --from=build /app/target/app.jar app.jar
EXPOSE 5106
ENTRYPOINT ["java", "-jar", "app.jar"]
```

### 1.4 Change java.version in all three pom.xml files

In `Services/Shop/pom.xml`, `Services/Workout-Logs/pom.xml`, `Services/Equipments/pom.xml`:

Change:
```xml
<java.version>25</java.version>
```
To:
```xml
<java.version>21</java.version>
```

---

## PHASE 2 — appsettings.Production.json overrides

These files must exist before building images. They are baked into the image at build time.

### 2.1 Identity API — Kestrel multi-endpoint config

`Services/Identity/Identity.Api/appsettings.Production.json`:
```json
{
  "Kestrel": {
    "Endpoints": {
      "Http": {
        "Url": "http://0.0.0.0:5051",
        "Protocols": "Http1"
      },
      "Grpc": {
        "Url": "http://0.0.0.0:8082",
        "Protocols": "Http2"
      }
    }
  }
}
```

### 2.2 Gateway — YARP cluster addresses

`Services/Gateway/appsettings.Production.json`:
```json
{
  "ReverseProxy": {
    "Clusters": {
      "identity-cluster": {
        "Destinations": { "identity-api": { "Address": "http://identity-api:5051" } }
      },
      "membership-cluster": {
        "Destinations": { "membership-api": { "Address": "http://membership-api:5121" } }
      },
      "payment-cluster": {
        "Destinations": { "payment-api": { "Address": "http://payment-api:5246" } }
      },
      "courses-cluster": {
        "Destinations": { "courses-api": { "Address": "http://courses-api:5101" } }
      },
      "activity-cluster": {
        "Destinations": { "activity-api": { "Address": "http://activity-api:5102" } }
      },
      "aggregation-cluster": {
        "Destinations": { "aggregation-api": { "Address": "http://aggregation-api:5103" } }
      },
      "chat-cluster": {
        "Destinations": { "chat-api": { "Address": "http://chat-api:5274" } }
      },
      "workout-logs-cluster": {
        "Destinations": { "workout-logs-api": { "Address": "http://workout-logs-api:5105" } }
      },
      "shop-cluster": {
        "Destinations": { "shop-api": { "Address": "http://shop-api:5104" } }
      },
      "equipments-cluster": {
        "Destinations": { "equipments-api": { "Address": "http://equipments-api:5106" } }
      }
    }
  }
}
```

---

## PHASE 3 — Build and Push All Images

### 3.1 Fix port-forward (critical — use 5000:80 not 57937:80)

Kill any existing port-forward terminal. Open a **dedicated PowerShell window** and run:
```powershell
kubectl port-forward --namespace kube-system service/registry 5000:80
```

Keep this terminal open for the entire build and push phase.

Verify in a separate terminal:
```powershell
curl http://localhost:5000/v2/_catalog
```
Must return `{"repositories":[]}` before proceeding.

### 3.2 Re-push already-built images

These built successfully but failed to push due to the wrong port:
```powershell
cd D:\projects\GYM\FitTech.Backend

docker push localhost:5000/aggregation-api:latest
docker push localhost:5000/chat-api:latest
docker push localhost:5000/notification-api:latest
docker push localhost:5000/gateway:latest
```

### 3.3 Build and push identity-api

```powershell
docker build -t localhost:5000/identity-api:latest `
  -f FitTech.AppHost\Dockerfiles\identity-api.Dockerfile .
docker push localhost:5000/identity-api:latest
```

### 3.4 Build and push the four previously-failing .NET services

```powershell
docker build -t localhost:5000/membership-api:latest `
  -f FitTech.AppHost\Dockerfiles\membership-api.Dockerfile .
docker push localhost:5000/membership-api:latest

docker build -t localhost:5000/payment-api:latest `
  -f FitTech.AppHost\Dockerfiles\payment-api.Dockerfile .
docker push localhost:5000/payment-api:latest

docker build -t localhost:5000/courses-api:latest `
  -f FitTech.AppHost\Dockerfiles\courses-api.Dockerfile .
docker push localhost:5000/courses-api:latest

docker build -t localhost:5000/activity-api:latest `
  -f FitTech.AppHost\Dockerfiles\activity-api.Dockerfile .
docker push localhost:5000/activity-api:latest
```

### 3.5 Build and push Java services

```powershell
docker build -t localhost:5000/shop-api:latest Services/Shop/
docker push localhost:5000/shop-api:latest

docker build -t localhost:5000/workout-logs-api:latest Services/Workout-Logs/
docker push localhost:5000/workout-logs-api:latest

docker build -t localhost:5000/equipments-api:latest Services/Equipments/
docker push localhost:5000/equipments-api:latest
```

### 3.6 Verify all 12 images are in registry

```powershell
curl http://localhost:5000/v2/_catalog
```

Must show all 12 names before proceeding to Phase 4.

---

## PHASE 4 — Create Kubernetes Manifests Directory

```powershell
mkdir D:\projects\GYM\FitTech.Backend\k8s
cd D:\projects\GYM\FitTech.Backend\k8s
```

---

## PHASE 5 — Namespace

`k8s/00-namespace.yaml`:
```yaml
apiVersion: v1
kind: Namespace
metadata:
  name: fittech
```

---

## PHASE 6 — Infrastructure: StatefulSets, PV/PVC, Secrets

### 6.1 PostgreSQL Secret

`k8s/01-postgres-secret.yaml`:
```yaml
apiVersion: v1
kind: Secret
metadata:
  name: postgres-secret
  namespace: fittech
type: Opaque
stringData:
  POSTGRES_PASSWORD: "postgres"
  POSTGRES_USER: "postgres"
```

### 6.2 PostgreSQL StatefulSet with PVC

`k8s/02-postgres-statefulset.yaml`:
```yaml
apiVersion: apps/v1
kind: StatefulSet
metadata:
  name: maindbserver
  namespace: fittech
spec:
  serviceName: maindbserver
  replicas: 1
  selector:
    matchLabels:
      app: maindbserver
  template:
    metadata:
      labels:
        app: maindbserver
    spec:
      containers:
        - name: postgres
          image: postgres:16
          ports:
            - containerPort: 5432
          envFrom:
            - secretRef:
                name: postgres-secret
          volumeMounts:
            - name: postgres-data
              mountPath: /var/lib/postgresql/data
          readinessProbe:
            exec:
              command: ["pg_isready", "-U", "postgres"]
            initialDelaySeconds: 5
            periodSeconds: 5
  volumeClaimTemplates:
    - metadata:
        name: postgres-data
      spec:
        accessModes: ["ReadWriteOnce"]
        resources:
          requests:
            storage: 5Gi
---
# Headless service for stable pod DNS
apiVersion: v1
kind: Service
metadata:
  name: maindbserver-headless
  namespace: fittech
spec:
  clusterIP: None
  selector:
    app: maindbserver
  ports:
    - port: 5432
      targetPort: 5432
---
# ClusterIP service — this is what all application connection strings point to
apiVersion: v1
kind: Service
metadata:
  name: maindbserver
  namespace: fittech
spec:
  selector:
    app: maindbserver
  ports:
    - port: 5432
      targetPort: 5432
```

### 6.3 Database Init Job

`k8s/03-db-init-job.yaml`:
```yaml
apiVersion: batch/v1
kind: Job
metadata:
  name: db-init
  namespace: fittech
spec:
  template:
    spec:
      restartPolicy: OnFailure
      containers:
        - name: db-init
          image: postgres:16
          env:
            - name: PGPASSWORD
              valueFrom:
                secretKeyRef:
                  name: postgres-secret
                  key: POSTGRES_PASSWORD
          command:
            - /bin/sh
            - -c
            - |
              until pg_isready -h maindbserver -p 5432 -U postgres; do
                echo "Waiting for postgres..."; sleep 3;
              done
              psql -h maindbserver -U postgres -c "CREATE DATABASE \"identity-db\";" || true
              psql -h maindbserver -U postgres -c "CREATE DATABASE \"membershipDb\";" || true
              psql -h maindbserver -U postgres -c "CREATE DATABASE \"paymentDb\";" || true
              psql -h maindbserver -U postgres -c "CREATE DATABASE \"coursesDb\";" || true
              psql -h maindbserver -U postgres -c "CREATE DATABASE \"activityDb\";" || true
              psql -h maindbserver -U postgres -c "CREATE DATABASE \"aggregationDb\";" || true
              psql -h maindbserver -U postgres -c "CREATE DATABASE \"chatDb\";" || true
              psql -h maindbserver -U postgres -c "CREATE DATABASE \"workoutLogsDb\";" || true
              psql -h maindbserver -U postgres -c "CREATE DATABASE \"shopDb\";" || true
              psql -h maindbserver -U postgres -c "CREATE DATABASE \"equipmentsDb\";" || true
              echo "All databases created."
```

### 6.4 RabbitMQ Secret

`k8s/04-rabbitmq-secret.yaml`:
```yaml
apiVersion: v1
kind: Secret
metadata:
  name: rabbitmq-secret
  namespace: fittech
type: Opaque
stringData:
  RABBITMQ_DEFAULT_USER: "fittech"
  RABBITMQ_DEFAULT_PASS: "fittech123"
```

### 6.5 RabbitMQ StatefulSet with PVC

`k8s/05-rabbitmq-statefulset.yaml`:
```yaml
apiVersion: apps/v1
kind: StatefulSet
metadata:
  name: rabbitmq
  namespace: fittech
spec:
  serviceName: rabbitmq
  replicas: 1
  selector:
    matchLabels:
      app: rabbitmq
  template:
    metadata:
      labels:
        app: rabbitmq
    spec:
      containers:
        - name: rabbitmq
          image: rabbitmq:3-management
          ports:
            - containerPort: 5672
            - containerPort: 15672
          envFrom:
            - secretRef:
                name: rabbitmq-secret
          volumeMounts:
            - name: rabbitmq-data
              mountPath: /var/lib/rabbitmq
          readinessProbe:
            exec:
              command: ["rabbitmq-diagnostics", "ping"]
            initialDelaySeconds: 20
            periodSeconds: 10
            timeoutSeconds: 10
  volumeClaimTemplates:
    - metadata:
        name: rabbitmq-data
      spec:
        accessModes: ["ReadWriteOnce"]
        resources:
          requests:
            storage: 2Gi
---
apiVersion: v1
kind: Service
metadata:
  name: rabbitmq-headless
  namespace: fittech
spec:
  clusterIP: None
  selector:
    app: rabbitmq
  ports:
    - name: amqp
      port: 5672
    - name: management
      port: 15672
---
apiVersion: v1
kind: Service
metadata:
  name: rabbitmq
  namespace: fittech
spec:
  selector:
    app: rabbitmq
  ports:
    - name: amqp
      port: 5672
      targetPort: 5672
    - name: management
      port: 15672
      targetPort: 15672
```

### 6.6 Redis StatefulSet with PVC

`k8s/06-redis-statefulset.yaml`:
```yaml
apiVersion: apps/v1
kind: StatefulSet
metadata:
  name: membership-cache
  namespace: fittech
spec:
  serviceName: membership-cache
  replicas: 1
  selector:
    matchLabels:
      app: membership-cache
  template:
    metadata:
      labels:
        app: membership-cache
    spec:
      containers:
        - name: redis
          image: redis:7-alpine
          ports:
            - containerPort: 6379
          command: ["redis-server", "--appendonly", "yes"]
          volumeMounts:
            - name: redis-data
              mountPath: /data
          readinessProbe:
            exec:
              command: ["redis-cli", "ping"]
            initialDelaySeconds: 5
            periodSeconds: 5
  volumeClaimTemplates:
    - metadata:
        name: redis-data
      spec:
        accessModes: ["ReadWriteOnce"]
        resources:
          requests:
            storage: 1Gi
---
apiVersion: v1
kind: Service
metadata:
  name: membership-cache-headless
  namespace: fittech
spec:
  clusterIP: None
  selector:
    app: membership-cache
  ports:
    - port: 6379
---
apiVersion: v1
kind: Service
metadata:
  name: membership-cache
  namespace: fittech
spec:
  selector:
    app: membership-cache
  ports:
    - port: 6379
      targetPort: 6379
```

### 6.7 smtp4dev Deployment and Service

`k8s/07-smtp4dev.yaml`:
```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: smtp4dev
  namespace: fittech
spec:
  replicas: 1
  selector:
    matchLabels:
      app: smtp4dev
  template:
    metadata:
      labels:
        app: smtp4dev
    spec:
      containers:
        - name: smtp4dev
          image: rnwood/smtp4dev
          ports:
            - containerPort: 80
            - containerPort: 25
          env:
            - name: ServerOptions__NumberOfMessagesToKeep
              value: "500"
            - name: ServerOptions__NumberOfSessionsToKeep
              value: "500"
---
apiVersion: v1
kind: Service
metadata:
  name: smtp4dev
  namespace: fittech
spec:
  selector:
    app: smtp4dev
  ports:
    - name: http
      port: 5005
      targetPort: 80
    - name: smtp
      port: 2525
      targetPort: 25
```

---

## PHASE 7 — Application Secrets and ConfigMaps

Every service gets a Secret (sensitive values) and a ConfigMap (non-sensitive values).
Deployments mount both via `envFrom`.

`k8s/10-secrets.yaml`:
```yaml
# ── identity-api ──────────────────────────────────────────────
apiVersion: v1
kind: Secret
metadata:
  name: identity-api-secret
  namespace: fittech
type: Opaque
stringData:
  ConnectionStrings__identity-db: "Host=maindbserver;Port=5432;Database=identity-db;Username=postgres;Password=postgres"
  ConnectionStrings__rabbitmq: "amqp://fittech:fittech123@rabbitmq:5672"
  JwtSettings__SecretKey: "dev-secret-change-in-production-must-be-32-chars"
  ServiceClients__membership-api: "dev-secret-change-in-production"
  ServiceClients__payment-api: "dev-secret-change-in-production"
  ServiceClients__courses-api: "dev-secret-change-in-production"
  ServiceClients__activity-api: "dev-secret-change-in-production"
  ServiceClients__aggregation-api: "dev-secret-change-in-production"
  ServiceClients__chat-api: "dev-secret-change-in-production"
  ServiceClients__notification-api: "dev-secret-change-in-production"
---
# ── membership-api ────────────────────────────────────────────
apiVersion: v1
kind: Secret
metadata:
  name: membership-api-secret
  namespace: fittech
type: Opaque
stringData:
  ConnectionStrings__membershipDb: "Host=maindbserver;Port=5432;Database=membershipDb;Username=postgres;Password=postgres"
  ConnectionStrings__rabbitmq: "amqp://fittech:fittech123@rabbitmq:5672"
  ConnectionStrings__membership-cache: "membership-cache:6379"
  ServiceAuth__ClientSecret: "dev-secret-change-in-production"
---
# ── payment-api ───────────────────────────────────────────────
apiVersion: v1
kind: Secret
metadata:
  name: payment-api-secret
  namespace: fittech
type: Opaque
stringData:
  ConnectionStrings__paymentDb: "Host=maindbserver;Port=5432;Database=paymentDb;Username=postgres;Password=postgres"
  ConnectionStrings__rabbitmq: "amqp://fittech:fittech123@rabbitmq:5672"
  ServiceAuth__ClientSecret: "dev-secret-change-in-production"
---
# ── courses-api ───────────────────────────────────────────────
apiVersion: v1
kind: Secret
metadata:
  name: courses-api-secret
  namespace: fittech
type: Opaque
stringData:
  ConnectionStrings__coursesDb: "Host=maindbserver;Port=5432;Database=coursesDb;Username=postgres;Password=postgres"
  ConnectionStrings__rabbitmq: "amqp://fittech:fittech123@rabbitmq:5672"
  ServiceAuth__ClientSecret: "dev-secret-change-in-production"
---
# ── activity-api ──────────────────────────────────────────────
apiVersion: v1
kind: Secret
metadata:
  name: activity-api-secret
  namespace: fittech
type: Opaque
stringData:
  ConnectionStrings__activityDb: "Host=maindbserver;Port=5432;Database=activityDb;Username=postgres;Password=postgres"
  ConnectionStrings__rabbitmq: "amqp://fittech:fittech123@rabbitmq:5672"
  ServiceAuth__ClientSecret: "dev-secret-change-in-production"
---
# ── aggregation-api ───────────────────────────────────────────
apiVersion: v1
kind: Secret
metadata:
  name: aggregation-api-secret
  namespace: fittech
type: Opaque
stringData:
  ConnectionStrings__aggregationDb: "Host=maindbserver;Port=5432;Database=aggregationDb;Username=postgres;Password=postgres"
  ConnectionStrings__rabbitmq: "amqp://fittech:fittech123@rabbitmq:5672"
---
# ── chat-api ──────────────────────────────────────────────────
apiVersion: v1
kind: Secret
metadata:
  name: chat-api-secret
  namespace: fittech
type: Opaque
stringData:
  ConnectionStrings__chatDb: "Host=maindbserver;Port=5432;Database=chatDb;Username=postgres;Password=postgres"
  ConnectionStrings__rabbitmq: "amqp://fittech:fittech123@rabbitmq:5672"
---
# ── notification-api ──────────────────────────────────────────
apiVersion: v1
kind: Secret
metadata:
  name: notification-api-secret
  namespace: fittech
type: Opaque
stringData:
  ConnectionStrings__rabbitmq: "amqp://fittech:fittech123@rabbitmq:5672"
---
# ── shop-api ──────────────────────────────────────────────────
apiVersion: v1
kind: Secret
metadata:
  name: shop-api-secret
  namespace: fittech
type: Opaque
stringData:
  SPRING_R2DBC_URL: "r2dbc:postgresql://maindbserver:5432/shopDb"
  SPRING_R2DBC_USERNAME: "postgres"
  SPRING_R2DBC_PASSWORD: "postgres"
---
# ── workout-logs-api ──────────────────────────────────────────
apiVersion: v1
kind: Secret
metadata:
  name: workout-logs-api-secret
  namespace: fittech
type: Opaque
stringData:
  SPRING_R2DBC_URL: "r2dbc:postgresql://maindbserver:5432/workoutLogsDb"
  SPRING_R2DBC_USERNAME: "postgres"
  SPRING_R2DBC_PASSWORD: "postgres"
---
# ── equipments-api ────────────────────────────────────────────
apiVersion: v1
kind: Secret
metadata:
  name: equipments-api-secret
  namespace: fittech
type: Opaque
stringData:
  SPRING_R2DBC_URL: "r2dbc:postgresql://maindbserver:5432/equipmentsDb"
  SPRING_R2DBC_USERNAME: "postgres"
  SPRING_R2DBC_PASSWORD: "postgres"
  SPRING_RABBITMQ_USERNAME: "fittech"
  SPRING_RABBITMQ_PASSWORD: "fittech123"
```

`k8s/11-configmaps.yaml`:
```yaml
# ── identity-api ──────────────────────────────────────────────
apiVersion: v1
kind: ConfigMap
metadata:
  name: identity-api-config
  namespace: fittech
data:
  ASPNETCORE_ENVIRONMENT: "Production"
  JwtSettings__Issuer: "http://identity-api:5051"
  JwtSettings__AccessTokenExpirationMinutes: "30"
---
# ── membership-api ────────────────────────────────────────────
apiVersion: v1
kind: ConfigMap
metadata:
  name: membership-api-config
  namespace: fittech
data:
  ASPNETCORE_ENVIRONMENT: "Production"
  ASPNETCORE_URLS: "http://+:5121"
  JwtSettings__Issuer: "http://identity-api:5051"
  ServiceAuth__ClientId: "membership-api"
  services__identity-api__http__0: "http://identity-api:5051"
  services__activity-api__http__0: "http://activity-api:5102"
---
# ── payment-api ───────────────────────────────────────────────
apiVersion: v1
kind: ConfigMap
metadata:
  name: payment-api-config
  namespace: fittech
data:
  ASPNETCORE_ENVIRONMENT: "Production"
  ASPNETCORE_URLS: "http://+:5246"
  JwtSettings__Issuer: "http://identity-api:5051"
  ServiceAuth__ClientId: "payment-api"
  services__identity-api__http__0: "http://identity-api:5051"
---
# ── courses-api ───────────────────────────────────────────────
apiVersion: v1
kind: ConfigMap
metadata:
  name: courses-api-config
  namespace: fittech
data:
  ASPNETCORE_ENVIRONMENT: "Production"
  ASPNETCORE_URLS: "http://+:5101"
  JwtSettings__Issuer: "http://identity-api:5051"
  ServiceAuth__ClientId: "courses-api"
  services__identity-api__http__0: "http://identity-api:5051"
---
# ── activity-api ──────────────────────────────────────────────
apiVersion: v1
kind: ConfigMap
metadata:
  name: activity-api-config
  namespace: fittech
data:
  ASPNETCORE_ENVIRONMENT: "Production"
  ASPNETCORE_URLS: "http://+:5102"
  JwtSettings__Issuer: "http://identity-api:5051"
  ServiceAuth__ClientId: "activity-api"
  services__identity-api__http__0: "http://identity-api:5051"
  services__membership-api__http__0: "http://membership-api:5121"
  services__courses-api__http__0: "http://courses-api:5101"
---
# ── aggregation-api ───────────────────────────────────────────
apiVersion: v1
kind: ConfigMap
metadata:
  name: aggregation-api-config
  namespace: fittech
data:
  ASPNETCORE_ENVIRONMENT: "Production"
  ASPNETCORE_URLS: "http://+:5103"
  services__identity-api__http__0: "http://identity-api:5051"
---
# ── chat-api ──────────────────────────────────────────────────
apiVersion: v1
kind: ConfigMap
metadata:
  name: chat-api-config
  namespace: fittech
data:
  ASPNETCORE_ENVIRONMENT: "Production"
  ASPNETCORE_URLS: "http://+:5274"
  Jwt__Authority: "http://identity-api:5051"
  Jwt__Issuer: "http://identity-api:5051"
  services__identity-api__http__0: "http://identity-api:5051"
---
# ── notification-api ──────────────────────────────────────────
apiVersion: v1
kind: ConfigMap
metadata:
  name: notification-api-config
  namespace: fittech
data:
  ASPNETCORE_ENVIRONMENT: "Production"
  ASPNETCORE_URLS: "http://+:80"
  Email__Host: "smtp4dev"
  Email__Port: "2525"
  Email__SenderName: "FitTech"
  Email__SenderEmail: "noreply@fittech.com"
  Email__UseTLS: "false"
  Frontend__BaseUrl: "http://localhost:5173"
  Identity__BaseUrl: "http://identity-api:5051"
---
# ── shop-api ──────────────────────────────────────────────────
apiVersion: v1
kind: ConfigMap
metadata:
  name: shop-api-config
  namespace: fittech
data:
  SERVER_PORT: "5104"
  SPRING_SQL_INIT_MODE: "never"
  LOGGING_LEVEL_ORG_SPRINGFRAMEWORK_DATA_R2DBC: "DEBUG"
  LOGGING_LEVEL_ORG_SPRINGFRAMEWORK_WEB: "DEBUG"
  APP_JWT_AUTHORITY: "http://identity-api:5051"
  APP_JWT_ISSUER: "http://identity-api:5051"
  services__identity-api__http__0: "http://identity-api:5051"
---
# ── workout-logs-api ──────────────────────────────────────────
apiVersion: v1
kind: ConfigMap
metadata:
  name: workout-logs-api-config
  namespace: fittech
data:
  SERVER_PORT: "5105"
  SPRING_SQL_INIT_MODE: "never"
  LOGGING_LEVEL_ORG_SPRINGFRAMEWORK_DATA_R2DBC: "DEBUG"
  services__identity-api__http__0: "http://identity-api:5051"
---
# ── equipments-api ────────────────────────────────────────────
apiVersion: v1
kind: ConfigMap
metadata:
  name: equipments-api-config
  namespace: fittech
data:
  SERVER_PORT: "5106"
  SPRING_SQL_INIT_MODE: "never"
  LOGGING_LEVEL_ORG_SPRINGFRAMEWORK_DATA_R2DBC: "DEBUG"
  APP_JWT_AUTHORITY: "http://identity-api:5051"
  APP_JWT_ISSUER: "http://identity-api:5051"
  SPRING_RABBITMQ_HOST: "rabbitmq"
  SPRING_RABBITMQ_PORT: "5672"
  services__identity-api__http__0: "http://identity-api:5051"
---
# ── gateway ───────────────────────────────────────────────────
apiVersion: v1
kind: ConfigMap
metadata:
  name: gateway-config
  namespace: fittech
data:
  ASPNETCORE_ENVIRONMENT: "Production"
  ASPNETCORE_URLS: "http://+:5098"
  services__identity-api__http__0: "http://identity-api:5051"
  services__membership-api__http__0: "http://membership-api:5121"
  services__payment-api__http__0: "http://payment-api:5246"
  services__courses-api__http__0: "http://courses-api:5101"
  services__activity-api__http__0: "http://activity-api:5102"
  services__aggregation-api__http__0: "http://aggregation-api:5103"
  services__chat-api__http__0: "http://chat-api:5274"
  services__workout-logs-api__http__0: "http://workout-logs-api:5105"
  services__shop-api__http__0: "http://shop-api:5104"
  services__equipments-api__http__0: "http://equipments-api:5106"
```

---

## PHASE 8 — Application Deployments and Services

### identity-api — dual port (HTTP + gRPC)

`k8s/20-identity-api.yaml`:
```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: identity-api
  namespace: fittech
spec:
  replicas: 1
  selector:
    matchLabels:
      app: identity-api
  template:
    metadata:
      labels:
        app: identity-api
    spec:
      containers:
        - name: identity-api
          image: localhost:5000/identity-api:latest
          imagePullPolicy: Always
          ports:
            - containerPort: 5051
              name: http
            - containerPort: 8082
              name: grpc
          envFrom:
            - configMapRef:
                name: identity-api-config
            - secretRef:
                name: identity-api-secret
---
apiVersion: v1
kind: Service
metadata:
  name: identity-api
  namespace: fittech
spec:
  selector:
    app: identity-api
  ports:
    - name: http
      port: 5051
      targetPort: 5051
    - name: grpc
      port: 8082
      targetPort: 8082
```

### membership-api

`k8s/21-membership-api.yaml`:
```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: membership-api
  namespace: fittech
spec:
  replicas: 1
  selector:
    matchLabels:
      app: membership-api
  template:
    metadata:
      labels:
        app: membership-api
    spec:
      containers:
        - name: membership-api
          image: localhost:5000/membership-api:latest
          imagePullPolicy: Always
          ports:
            - containerPort: 5121
          envFrom:
            - configMapRef:
                name: membership-api-config
            - secretRef:
                name: membership-api-secret
---
apiVersion: v1
kind: Service
metadata:
  name: membership-api
  namespace: fittech
spec:
  selector:
    app: membership-api
  ports:
    - port: 5121
      targetPort: 5121
```

### payment-api

`k8s/22-payment-api.yaml`:
```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: payment-api
  namespace: fittech
spec:
  replicas: 1
  selector:
    matchLabels:
      app: payment-api
  template:
    metadata:
      labels:
        app: payment-api
    spec:
      containers:
        - name: payment-api
          image: localhost:5000/payment-api:latest
          imagePullPolicy: Always
          ports:
            - containerPort: 5246
          envFrom:
            - configMapRef:
                name: payment-api-config
            - secretRef:
                name: payment-api-secret
---
apiVersion: v1
kind: Service
metadata:
  name: payment-api
  namespace: fittech
spec:
  selector:
    app: payment-api
  ports:
    - port: 5246
      targetPort: 5246
```

### courses-api

`k8s/23-courses-api.yaml`:
```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: courses-api
  namespace: fittech
spec:
  replicas: 1
  selector:
    matchLabels:
      app: courses-api
  template:
    metadata:
      labels:
        app: courses-api
    spec:
      containers:
        - name: courses-api
          image: localhost:5000/courses-api:latest
          imagePullPolicy: Always
          ports:
            - containerPort: 5101
          envFrom:
            - configMapRef:
                name: courses-api-config
            - secretRef:
                name: courses-api-secret
---
apiVersion: v1
kind: Service
metadata:
  name: courses-api
  namespace: fittech
spec:
  selector:
    app: courses-api
  ports:
    - port: 5101
      targetPort: 5101
```

### activity-api

`k8s/24-activity-api.yaml`:
```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: activity-api
  namespace: fittech
spec:
  replicas: 1
  selector:
    matchLabels:
      app: activity-api
  template:
    metadata:
      labels:
        app: activity-api
    spec:
      containers:
        - name: activity-api
          image: localhost:5000/activity-api:latest
          imagePullPolicy: Always
          ports:
            - containerPort: 5102
          envFrom:
            - configMapRef:
                name: activity-api-config
            - secretRef:
                name: activity-api-secret
---
apiVersion: v1
kind: Service
metadata:
  name: activity-api
  namespace: fittech
spec:
  selector:
    app: activity-api
  ports:
    - port: 5102
      targetPort: 5102
```

### aggregation-api

`k8s/25-aggregation-api.yaml`:
```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: aggregation-api
  namespace: fittech
spec:
  replicas: 1
  selector:
    matchLabels:
      app: aggregation-api
  template:
    metadata:
      labels:
        app: aggregation-api
    spec:
      containers:
        - name: aggregation-api
          image: localhost:5000/aggregation-api:latest
          imagePullPolicy: Always
          ports:
            - containerPort: 5103
          envFrom:
            - configMapRef:
                name: aggregation-api-config
            - secretRef:
                name: aggregation-api-secret
---
apiVersion: v1
kind: Service
metadata:
  name: aggregation-api
  namespace: fittech
spec:
  selector:
    app: aggregation-api
  ports:
    - port: 5103
      targetPort: 5103
```

### chat-api

`k8s/26-chat-api.yaml`:
```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: chat-api
  namespace: fittech
spec:
  replicas: 1
  selector:
    matchLabels:
      app: chat-api
  template:
    metadata:
      labels:
        app: chat-api
    spec:
      containers:
        - name: chat-api
          image: localhost:5000/chat-api:latest
          imagePullPolicy: Always
          ports:
            - containerPort: 5274
          envFrom:
            - configMapRef:
                name: chat-api-config
            - secretRef:
                name: chat-api-secret
---
apiVersion: v1
kind: Service
metadata:
  name: chat-api
  namespace: fittech
spec:
  selector:
    app: chat-api
  ports:
    - port: 5274
      targetPort: 5274
```

### notification-api

`k8s/27-notification-api.yaml`:
```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: notification-api
  namespace: fittech
spec:
  replicas: 1
  selector:
    matchLabels:
      app: notification-api
  template:
    metadata:
      labels:
        app: notification-api
    spec:
      containers:
        - name: notification-api
          image: localhost:5000/notification-api:latest
          imagePullPolicy: Always
          ports:
            - containerPort: 80
          envFrom:
            - configMapRef:
                name: notification-api-config
            - secretRef:
                name: notification-api-secret
---
apiVersion: v1
kind: Service
metadata:
  name: notification-api
  namespace: fittech
spec:
  selector:
    app: notification-api
  ports:
    - port: 80
      targetPort: 80
```

### shop-api

`k8s/28-shop-api.yaml`:
```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: shop-api
  namespace: fittech
spec:
  replicas: 1
  selector:
    matchLabels:
      app: shop-api
  template:
    metadata:
      labels:
        app: shop-api
    spec:
      containers:
        - name: shop-api
          image: localhost:5000/shop-api:latest
          imagePullPolicy: Always
          ports:
            - containerPort: 5104
          envFrom:
            - configMapRef:
                name: shop-api-config
            - secretRef:
                name: shop-api-secret
---
apiVersion: v1
kind: Service
metadata:
  name: shop-api
  namespace: fittech
spec:
  selector:
    app: shop-api
  ports:
    - port: 5104
      targetPort: 5104
```

### workout-logs-api

`k8s/29-workout-logs-api.yaml`:
```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: workout-logs-api
  namespace: fittech
spec:
  replicas: 1
  selector:
    matchLabels:
      app: workout-logs-api
  template:
    metadata:
      labels:
        app: workout-logs-api
    spec:
      containers:
        - name: workout-logs-api
          image: localhost:5000/workout-logs-api:latest
          imagePullPolicy: Always
          ports:
            - containerPort: 5105
          envFrom:
            - configMapRef:
                name: workout-logs-api-config
            - secretRef:
                name: workout-logs-api-secret
---
apiVersion: v1
kind: Service
metadata:
  name: workout-logs-api
  namespace: fittech
spec:
  selector:
    app: workout-logs-api
  ports:
    - port: 5105
      targetPort: 5105
```

### equipments-api

`k8s/30-equipments-api.yaml`:
```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: equipments-api
  namespace: fittech
spec:
  replicas: 1
  selector:
    matchLabels:
      app: equipments-api
  template:
    metadata:
      labels:
        app: equipments-api
    spec:
      containers:
        - name: equipments-api
          image: localhost:5000/equipments-api:latest
          imagePullPolicy: Always
          ports:
            - containerPort: 5106
          envFrom:
            - configMapRef:
                name: equipments-api-config
            - secretRef:
                name: equipments-api-secret
---
apiVersion: v1
kind: Service
metadata:
  name: equipments-api
  namespace: fittech
spec:
  selector:
    app: equipments-api
  ports:
    - port: 5106
      targetPort: 5106
```

### gateway — NodePort for external access

`k8s/31-gateway.yaml`:
```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: gateway
  namespace: fittech
spec:
  replicas: 1
  selector:
    matchLabels:
      app: gateway
  template:
    metadata:
      labels:
        app: gateway
    spec:
      containers:
        - name: gateway
          image: localhost:5000/gateway:latest
          imagePullPolicy: Always
          ports:
            - containerPort: 5098
          envFrom:
            - configMapRef:
                name: gateway-config
---
apiVersion: v1
kind: Service
metadata:
  name: gateway
  namespace: fittech
spec:
  type: NodePort
  selector:
    app: gateway
  ports:
    - port: 5098
      targetPort: 5098
      nodePort: 30098
```

---

## PHASE 9 — Apply All Manifests

Run from `D:\projects\GYM\FitTech.Backend`. Keep port-forward terminal open.

```powershell
# Namespace
kubectl apply -f k8s/00-namespace.yaml

# Infrastructure secrets
kubectl apply -f k8s/01-postgres-secret.yaml
kubectl apply -f k8s/04-rabbitmq-secret.yaml

# StatefulSets
kubectl apply -f k8s/02-postgres-statefulset.yaml
kubectl apply -f k8s/05-rabbitmq-statefulset.yaml
kubectl apply -f k8s/06-redis-statefulset.yaml
kubectl apply -f k8s/07-smtp4dev.yaml

# Wait for postgres readiness before running init job
kubectl wait --for=condition=ready pod -l app=maindbserver -n fittech --timeout=180s

# Create all databases
kubectl apply -f k8s/03-db-init-job.yaml
kubectl wait --for=condition=complete job/db-init -n fittech --timeout=120s

# Application config
kubectl apply -f k8s/10-secrets.yaml
kubectl apply -f k8s/11-configmaps.yaml

# Applications — identity first, gateway last
kubectl apply -f k8s/20-identity-api.yaml
kubectl apply -f k8s/21-membership-api.yaml
kubectl apply -f k8s/22-payment-api.yaml
kubectl apply -f k8s/23-courses-api.yaml
kubectl apply -f k8s/24-activity-api.yaml
kubectl apply -f k8s/25-aggregation-api.yaml
kubectl apply -f k8s/26-chat-api.yaml
kubectl apply -f k8s/27-notification-api.yaml
kubectl apply -f k8s/28-shop-api.yaml
kubectl apply -f k8s/29-workout-logs-api.yaml
kubectl apply -f k8s/30-equipments-api.yaml
kubectl apply -f k8s/31-gateway.yaml
```

---

## PHASE 10 — Verify

```powershell
# All pods must show Running 1/1
kubectl get pods -n fittech

# All services must exist
kubectl get services -n fittech

# All StatefulSets must show READY 1/1
kubectl get statefulsets -n fittech

# All PVCs must show Bound
kubectl get pvc -n fittech
```

Get the gateway URL:
```powershell
minikube service gateway -n fittech --url
```

Test identity endpoint:
```powershell
curl http://<URL-from-above>/api/User/login
```

---

## PHASE 11 — Debugging Reference

### Check pod logs
```powershell
kubectl logs -n fittech deployment/<service-name> --tail=100
```

### Check env vars received by a pod
```powershell
kubectl exec -n fittech deployment/identity-api -- env | Sort-Object
```

### Check StatefulSet pod
```powershell
kubectl logs -n fittech statefulset/maindbserver
kubectl logs -n fittech statefulset/rabbitmq
kubectl logs -n fittech statefulset/membership-cache
```

### Pod in CrashLoopBackOff — .NET service
Most common causes in order:
1. DB migration running before DB is ready — guard with `IsDevelopment()` check
2. Wrong connection string key — check with `kubectl exec -- env`
3. JWT issuer mismatch — must be `http://identity-api:5051` everywhere

### Pod in CrashLoopBackOff — Spring Boot service
Most common causes in order:
1. `SPRING_R2DBC_URL` format wrong — must be `r2dbc:postgresql://maindbserver:5432/dbname`
2. RabbitMQ not ready yet — pod will restart and succeed once RabbitMQ is up
3. Java version mismatch — verify `<java.version>21</java.version>` in pom.xml

### gRPC not reachable
Verify identity-api Service exposes port 8082:
```powershell
kubectl get service identity-api -n fittech -o yaml
```
Verify Kestrel config baked into image:
```powershell
kubectl exec -n fittech deployment/identity-api -- cat /app/appsettings.Production.json
```

### ImagePullBackOff
```powershell
# Verify registry proxy pods are running
kubectl get pods -n kube-system | Select-String "registry"
# Both registry-* and registry-proxy-* must be Running
```

### Re-push a single image after a code change
```powershell
# Keep port-forward running: kubectl port-forward --namespace kube-system service/registry 5000:80
docker build -t localhost:5000/identity-api:latest -f FitTech.AppHost\Dockerfiles\identity-api.Dockerfile .
docker push localhost:5000/identity-api:latest
kubectl rollout restart deployment/identity-api -n fittech
kubectl rollout status deployment/identity-api -n fittech
```

---

## Complete Execution Checklist

- [ ] Change `<java.version>25</java.version>` → `21` in all 3 Java pom.xml files
- [ ] Create `Services/Identity/Identity.Api/appsettings.Production.json` (Kestrel config)
- [ ] Create `Services/Gateway/appsettings.Production.json` (YARP cluster addresses)
- [ ] Create all 9 .NET Dockerfiles with NuGet.Config override
- [ ] Create all 3 Java Dockerfiles
- [ ] Port-forward running on `5000:80`
- [ ] Re-push the 4 already-built images (aggregation, chat, notification, gateway)
- [ ] Build and push identity-api (new — with gRPC port)
- [ ] Build and push membership, payment, courses, activity (with NuGet fix)
- [ ] Build and push shop, workout-logs, equipments (with Java 21)
- [ ] Verify all 12 images in registry catalog
- [ ] Create `k8s/` directory and all manifest files
- [ ] Apply manifests in order (Phase 9)
- [ ] Verify all pods Running, all PVCs Bound
- [ ] Access via `minikube service gateway -n fittech --url`
