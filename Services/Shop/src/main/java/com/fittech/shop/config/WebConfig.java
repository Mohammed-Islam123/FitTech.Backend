package com.fittech.shop.config;

import jakarta.annotation.PostConstruct;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.context.annotation.Configuration;
import org.springframework.web.reactive.config.ResourceHandlerRegistry;
import org.springframework.web.reactive.config.WebFluxConfigurer;

import java.io.IOException;
import java.nio.file.Files;
import java.nio.file.Path;

@Configuration
public class WebConfig implements WebFluxConfigurer {

    @Value("${app.upload.dir:uploads/images}")
    private String uploadDir;

    @PostConstruct
    void ensureUploadDir() throws IOException {
        Files.createDirectories(Path.of(uploadDir));
    }

    @Override
    public void addResourceHandlers(ResourceHandlerRegistry registry) {
        var location = "file:" + Path.of(uploadDir).toAbsolutePath() + "/";
        registry.addResourceHandler("/uploads/**")
                .addResourceLocations(location);
    }
}
