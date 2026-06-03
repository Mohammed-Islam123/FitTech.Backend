package com.fittech.shop.config;

import org.springframework.beans.factory.annotation.Value;
import org.springframework.context.annotation.Bean;
import org.springframework.context.annotation.Configuration;
import org.springframework.http.HttpMethod;
import org.springframework.security.config.annotation.web.reactive.EnableWebFluxSecurity;
import org.springframework.security.config.web.server.ServerHttpSecurity;
import org.springframework.security.core.authority.SimpleGrantedAuthority;
import org.springframework.security.oauth2.jwt.ReactiveJwtDecoder;
import org.springframework.security.oauth2.jwt.ReactiveJwtDecoders;
import org.springframework.security.oauth2.server.resource.authentication.ReactiveJwtAuthenticationConverter;
import org.springframework.security.web.server.SecurityWebFilterChain;
import reactor.core.publisher.Flux;

@Configuration
@EnableWebFluxSecurity
public class SecurityConfig {

    /**
     * .NET ClaimTypes.Role.
     */
    private static final String DOTNET_ROLE_CLAIM =
            "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";

    @Value("${app.jwt.authority}")
    private String jwtAuthority;

    @Bean
    public SecurityWebFilterChain securityWebFilterChain(ServerHttpSecurity http) {
        return http
                .csrf(ServerHttpSecurity.CsrfSpec::disable)
                .authorizeExchange(exchanges -> exchanges
                        // Product CRUD — GET is public, mutations are admin-only
                        .pathMatchers(HttpMethod.GET, "/api/products/**").permitAll()
                        .pathMatchers(HttpMethod.POST, "/api/products/**").hasRole("Admin")
                        .pathMatchers(HttpMethod.PUT, "/api/products/**").hasRole("Admin")
                        .pathMatchers(HttpMethod.DELETE, "/api/products/**").hasRole("Admin")
                        // Purchases — admin only
                        .pathMatchers("/api/purchases/**").hasRole("Admin")
                        // Health check — open
                        .pathMatchers("/actuator/health").permitAll()
                        // Static files — open (images served via /uploads/**)
                        .pathMatchers("/uploads/**").permitAll()
                        .anyExchange().authenticated()
                )
                .oauth2ResourceServer(oauth2 -> oauth2
                        .jwt(jwt -> jwt
                                .jwtAuthenticationConverter(jwtAuthenticationConverter())
                        )
                )
                .build();
    }

    @Bean
    public ReactiveJwtDecoder reactiveJwtDecoder() {
        return ReactiveJwtDecoders.fromIssuerLocation(jwtAuthority);
    }

    /**
     * Extracts the .NET role claim and maps it to Spring Security ROLE_ authorities.
     */
    private ReactiveJwtAuthenticationConverter jwtAuthenticationConverter() {
        var converter = new ReactiveJwtAuthenticationConverter();
        converter.setJwtGrantedAuthoritiesConverter(jwt -> {
            var roleClaim = jwt.getClaimAsStringList(DOTNET_ROLE_CLAIM);
            if (roleClaim == null) {
                return Flux.empty();
            }
            return Flux.fromIterable(
                    roleClaim.stream()
                            .map(role -> new SimpleGrantedAuthority("ROLE_" + role))
                            .toList()
            );
        });
        return converter;
    }
}
