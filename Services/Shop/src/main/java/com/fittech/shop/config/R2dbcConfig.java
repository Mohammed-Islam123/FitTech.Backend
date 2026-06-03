package com.fittech.shop.config;

import io.r2dbc.spi.ConnectionFactory;
import io.r2dbc.spi.ConnectionFactoryOptions;
import org.springframework.boot.r2dbc.ConnectionFactoryBuilder;
import org.springframework.context.annotation.Bean;
import org.springframework.context.annotation.Configuration;
import org.springframework.core.io.ClassPathResource;
import org.springframework.r2dbc.connection.init.ConnectionFactoryInitializer;
import org.springframework.r2dbc.connection.init.ResourceDatabasePopulator;

import java.util.HashMap;
import java.util.Map;

@Configuration
public class R2dbcConfig {

    @Bean
    public ConnectionFactory r2dbcConnectionFactory() {
        String connectionString = System.getenv("ConnectionStrings__shopDb");

        if (connectionString == null || connectionString.isBlank()) {
            throw new IllegalStateException("Missing environment variable: ConnectionStrings__shopDb");
        }

        Map<String, String> parts = new HashMap<>();
        for (String pair : connectionString.split(";")) {
            String[] kv = pair.split("=", 2);
            if (kv.length == 2) {
                parts.put(kv[0].trim().toLowerCase(), kv[1].trim());
            }
        }

        String host = require(parts, "host");
        int port = Integer.parseInt(require(parts, "port"));
        String database = require(parts, "database");
        String username = require(parts, "username");
        String password = require(parts, "password");

        var options = ConnectionFactoryOptions.builder()
                .option(ConnectionFactoryOptions.DRIVER, "postgresql")
                .option(ConnectionFactoryOptions.HOST, host)
                .option(ConnectionFactoryOptions.PORT, port)
                .option(ConnectionFactoryOptions.DATABASE, database)
                .option(ConnectionFactoryOptions.USER, username)
                .option(ConnectionFactoryOptions.PASSWORD, password);

        return ConnectionFactoryBuilder.withOptions(options).build();
    }

    @Bean
    public ConnectionFactoryInitializer r2dbcInitializer(ConnectionFactory connectionFactory) {
        var initializer = new ConnectionFactoryInitializer();
        initializer.setConnectionFactory(connectionFactory);

        var populator = new ResourceDatabasePopulator();
        populator.addScript(new ClassPathResource("schema.sql"));
        initializer.setDatabasePopulator(populator);

        return initializer;
    }

    private String require(Map<String, String> parts, String key) {
        String value = parts.get(key);
        if (value == null || value.isBlank()) {
            throw new IllegalStateException("Missing '" + key + "' in ConnectionStrings__shopDb");
        }
        return value;
    }
}
