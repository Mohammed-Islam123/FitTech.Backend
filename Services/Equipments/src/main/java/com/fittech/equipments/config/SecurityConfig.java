package com.fittech.equipments.config;

import org.springframework.beans.factory.annotation.Value;
import org.springframework.security.oauth2.jose.jws.SignatureAlgorithm;
import org.springframework.context.annotation.Bean;
import org.springframework.context.annotation.Configuration;
import org.springframework.http.HttpMethod;
import org.springframework.security.config.annotation.web.reactive.EnableWebFluxSecurity;
import org.springframework.security.config.web.server.ServerHttpSecurity;
import org.springframework.security.core.authority.SimpleGrantedAuthority;
import org.springframework.security.oauth2.jwt.JwtValidators;
import org.springframework.security.oauth2.jwt.NimbusReactiveJwtDecoder;
import org.springframework.security.oauth2.jwt.ReactiveJwtDecoder;
import org.springframework.security.oauth2.server.resource.authentication.ReactiveJwtAuthenticationConverter;
import org.springframework.security.web.server.SecurityWebFilterChain;
import reactor.core.publisher.Flux;

import static org.springframework.security.config.Customizer.withDefaults;

@Configuration
@EnableWebFluxSecurity
public class SecurityConfig {

    /**
     * Dotnet ClaimTypes.Role.
     */
    private static final String DOTNET_ROLE_CLAIM = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";

    @Value("${app.jwt.authority}")
    private String jwtAuthority;

    @Value("${app.jwt.issuer}")
    private String jwtIssuer;

    @Bean
    public SecurityWebFilterChain securityWebFilterChain(ServerHttpSecurity http) {
        return http
                .csrf(ServerHttpSecurity.CsrfSpec::disable)
                .authorizeExchange(exchanges -> exchanges
                        // Equipment CRUD — admin only
                        .pathMatchers(HttpMethod.POST, "/api/equipments/**").hasRole("Admin")
                        .pathMatchers(HttpMethod.PUT, "/api/equipments/**").hasRole("Admin")
                        .pathMatchers(HttpMethod.DELETE, "/api/equipments/**").hasRole("Admin")
                        .pathMatchers(HttpMethod.GET, "/api/equipments/**").hasRole("Admin")
                        // Issue fetching + status update — admin only
                        .pathMatchers(HttpMethod.GET, "/api/issues/**").hasRole("Admin")
                        .pathMatchers(HttpMethod.PATCH, "/api/issues/**").hasRole("Admin")
                        // Issue creation — any authenticated member/coach/admin
                        .pathMatchers(HttpMethod.POST, "/api/issues/**").authenticated()
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
        var decoder = NimbusReactiveJwtDecoder
                .withJwkSetUri(jwtAuthority + "/.well-known/jwks")
                .jwsAlgorithm(SignatureAlgorithm.RS256)
                .build();

        decoder.setJwtValidator(JwtValidators.createDefaultWithIssuer(jwtIssuer));

        return decoder;
    }

    /**
     * Extracts the .NET role claim ({@code http://schemas.microsoft.com/ws/2008/06/identity/claims/role})
     * and maps it to Spring Security {@code ROLE_} authorities.
     * <p>
     * The .NET Identity service stores admin/coach/member roles under this claim.
     * Spring Security's hasRole("Admin") expects a GrantedAuthority named "ROLE_Admin".
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
