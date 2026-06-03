package com.fittech.shop.service;

import com.fittech.shop.dto.CreateProductRequest;
import com.fittech.shop.dto.ProductResponse;
import com.fittech.shop.dto.UpdateProductRequest;
import com.fittech.shop.model.Product;
import com.fittech.shop.repository.ProductRepository;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.core.io.buffer.DataBufferUtils;
import org.springframework.http.HttpStatus;
import org.springframework.http.codec.multipart.FilePart;
import org.springframework.stereotype.Service;
import org.springframework.web.server.ResponseStatusException;
import reactor.core.publisher.Flux;
import reactor.core.publisher.Mono;

import java.io.IOException;
import java.nio.file.Files;
import java.nio.file.Path;
import java.time.LocalDateTime;
import java.util.UUID;

@Service
public class ProductService {

    private static final Logger log = LoggerFactory.getLogger(ProductService.class);

    private final ProductRepository repository;

    @Value("${app.upload.dir:uploads/images}")
    private String uploadDir;

    public ProductService(ProductRepository repository) {
        this.repository = repository;
    }

    public Flux<ProductResponse> listAll() {
        return repository.findByIsActiveTrueOrderByCreatedAtDesc()
                .map(ProductResponse::from);
    }

    public Mono<ProductResponse> getById(UUID id) {
        return repository.findByIdAndIsActiveTrue(id)
                .map(ProductResponse::from)
                .switchIfEmpty(Mono.error(
                        new ResponseStatusException(HttpStatus.NOT_FOUND, "Product not found")));
    }

    public Mono<ProductResponse> create(CreateProductRequest request, Mono<FilePart> imagePart) {
        var productId = UUID.randomUUID();
        var now = LocalDateTime.now();

        var product = Product.builder()
                .id(productId)
                .name(request.getName())
                .description(request.getDescription())
                .price(request.getPrice())
                .category(request.getCategory())
                .stock(request.getStock() != null ? request.getStock() : 0)
                .isActive(true)
                .createdAt(now)
                .build();

        Mono<String> imageMono = imagePart != null
                ? saveImage(productId, imagePart)
                : Mono.just((String) null);

        return imageMono
                .flatMap(imageUrl -> {
                    if (imageUrl != null) product.setImagePath(imageUrl);
                    return repository.save(product);
                })
                .map(ProductResponse::from);
    }

    public Mono<ProductResponse> update(UUID id, UpdateProductRequest request, Mono<FilePart> imagePart) {
        return repository.findByIdAndIsActiveTrue(id)
                .switchIfEmpty(Mono.error(
                        new ResponseStatusException(HttpStatus.NOT_FOUND, "Product not found")))
                .flatMap(existing -> {
                    if (request.getName() != null) existing.setName(request.getName());
                    if (request.getDescription() != null) existing.setDescription(request.getDescription());
                    if (request.getPrice() != null) existing.setPrice(request.getPrice());
                    if (request.getCategory() != null) existing.setCategory(request.getCategory());
                    if (request.getStock() != null) existing.setStock(request.getStock());
                    existing.setUpdatedAt(LocalDateTime.now());

                    Mono<String> imageMono = imagePart != null
                            ? saveImage(id, imagePart)
                            : Mono.just(existing.getImagePath());

                    return imageMono
                            .flatMap(imageUrl -> {
                                existing.setImagePath(imageUrl);
                                return repository.save(existing);
                            });
                })
                .map(ProductResponse::from);
    }

    public Mono<Void> softDelete(UUID id) {
        return repository.findByIdAndIsActiveTrue(id)
                .switchIfEmpty(Mono.error(
                        new ResponseStatusException(HttpStatus.NOT_FOUND, "Product not found")))
                .flatMap(e -> repository.softDeleteById(e.getId()))
                .then();
    }

    private Mono<String> saveImage(UUID productId, Mono<FilePart> imagePart) {
        return imagePart.flatMap(part -> {
            var ext = extractExtension(part.filename());
            var filename = productId + ext;
            var dest = Path.of(uploadDir, filename);

            return DataBufferUtils
                    .join(part.content())
                    .flatMap(buffer -> {
                        try {
                            Files.createDirectories(dest.getParent());
                            var data = new byte[buffer.readableByteCount()];
                            buffer.read(data);
                            DataBufferUtils.release(buffer);
                            Files.write(dest, data);
                            log.info("Saved image {} ({} bytes)", filename, data.length);
                            return Mono.just("/uploads/" + filename);
                        } catch (IOException e) {
                            DataBufferUtils.release(buffer);
                            return Mono.error(new RuntimeException("Failed to save image", e));
                        }
                    });
        });
    }

    private static String extractExtension(String filename) {
        if (filename == null || !filename.contains(".")) return ".jpg";
        return filename.substring(filename.lastIndexOf('.'));
    }
}
