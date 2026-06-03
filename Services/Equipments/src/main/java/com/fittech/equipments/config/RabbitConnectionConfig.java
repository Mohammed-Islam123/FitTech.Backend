package com.fittech.equipments.config;

import jakarta.annotation.PostConstruct;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.amqp.rabbit.connection.CachingConnectionFactory;
import org.springframework.amqp.rabbit.connection.ConnectionFactory;
import org.springframework.context.annotation.Bean;
import org.springframework.context.annotation.Configuration;

/**
 * Reads Aspire-injected RabbitMQ connection string from the environment
 * ({@code ConnectionStrings__rabbitmq}) and creates a {@link ConnectionFactory}.
 * <p>
 * Expected format: {@code host=HOST;port=PORT;username=USER;password=PASS}
 * Falls back to Spring Boot auto-configuration (localhost:5672, guest/guest) if not set.
 */
@Configuration
public class RabbitConnectionConfig {

    private static final Logger log = LoggerFactory.getLogger(RabbitConnectionConfig.class);

    private String host = "localhost";
    private int port = 5672;
    private String username = "guest";
    private String password = "guest";

    @PostConstruct
    void init() {
        var connStr = System.getenv("ConnectionStrings__rabbitmq");
        if (connStr != null && !connStr.isBlank()) {
            log.info("Parsing ConnectionStrings__rabbitmq from environment");
            for (var pair : connStr.split(";")) {
                var kv = pair.split("=", 2);
                if (kv.length != 2) continue;
                switch (kv[0].trim().toLowerCase()) {
                    case "host" -> host = kv[1].trim();
                    case "port" -> port = Integer.parseInt(kv[1].trim());
                    case "username" -> username = kv[1].trim();
                    case "password" -> password = kv[1].trim();
                }
            }
        } else {
            log.info("ConnectionStrings__rabbitmq not set — using defaults (localhost:5672)");
        }
    }

    @Bean
    public ConnectionFactory connectionFactory() {
        var factory = new CachingConnectionFactory(host, port);
        factory.setUsername(username);
        factory.setPassword(password);
        log.info("RabbitMQ configured: {}:{} as {}", host, port, username);
        return factory;
    }
}
