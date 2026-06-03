package com.fittech.shop.controller;

import com.fittech.shop.dto.CreateProductRequest;
import com.fittech.shop.dto.ProductResponse;
import com.fittech.shop.dto.UpdateProductRequest;
import com.fittech.shop.service.ProductService;
import org.springframework.http.HttpStatus;
import org.springframework.http.MediaType;
import org.springframework.http.codec.multipart.FilePart;
import org.springframework.http.codec.multipart.FormFieldPart;
import org.springframework.http.codec.multipart.Part;
import org.springframework.web.bind.annotation.*;
import reactor.core.publisher.Flux;
import reactor.core.publisher.Mono;

import java.math.BigDecimal;
import java.util.Map;
import java.util.Optional;
import java.util.UUID;

@RestController
@RequestMapping("/api/products")
public class ProductController {

    private final ProductService productService;

    public ProductController(ProductService productService) {
        this.productService = productService;
    }

    @GetMapping
    public Flux<ProductResponse> listAll() {
        return productService.listAll();
    }

    @GetMapping("/{id}")
    public Mono<ProductResponse> getById(@PathVariable UUID id) {
        return productService.getById(id);
    }

    @PostMapping(consumes = MediaType.MULTIPART_FORM_DATA_VALUE)
    @ResponseStatus(HttpStatus.CREATED)
    public Mono<ProductResponse> create(@RequestBody Flux<Part> parts) {
        return parts.collectMap(Part::name).flatMap(map -> {
            var req = new CreateProductRequest();
            extractFormField(map, "name").ifPresent(req::setName);
            extractFormField(map, "description").ifPresent(req::setDescription);
            extractFormField(map, "category").ifPresent(req::setCategory);

            var priceStr = extractFormField(map, "price");
            if (priceStr.isPresent()) {
                req.setPrice(new BigDecimal(priceStr.get()));
            }

            var stockStr = extractFormField(map, "stock");
            stockStr.ifPresent(s -> req.setStock(Integer.parseInt(s)));

            if (req.getName() == null || req.getName().isBlank()) {
                return Mono.error(new IllegalArgumentException("name is required"));
            }
            if (req.getPrice() == null) {
                return Mono.error(new IllegalArgumentException("price is required"));
            }

            Mono<FilePart> image = extractFilePart(map, "image");
            return productService.create(req, image);
        });
    }

    @PutMapping(value = "/{id}", consumes = MediaType.MULTIPART_FORM_DATA_VALUE)
    public Mono<ProductResponse> update(
            @PathVariable UUID id,
            @RequestBody Flux<Part> parts) {
        return parts.collectMap(Part::name).flatMap(map -> {
            var req = new UpdateProductRequest();
            extractFormField(map, "name").ifPresent(req::setName);
            extractFormField(map, "description").ifPresent(req::setDescription);
            extractFormField(map, "category").ifPresent(req::setCategory);

            var priceStr = extractFormField(map, "price");
            priceStr.ifPresent(s -> req.setPrice(new BigDecimal(s)));

            var stockStr = extractFormField(map, "stock");
            stockStr.ifPresent(s -> req.setStock(Integer.parseInt(s)));

            Mono<FilePart> image = extractFilePart(map, "image");
            return productService.update(id, req, image);
        });
    }

    @DeleteMapping("/{id}")
    @ResponseStatus(HttpStatus.NO_CONTENT)
    public Mono<Void> delete(@PathVariable UUID id) {
        return productService.softDelete(id);
    }

    // --- helpers ---

    private static Optional<String> extractFormField(Map<String, Part> map, String name) {
        var part = map.get(name);
        if (part instanceof FormFieldPart ff) {
            var val = ff.value();
            return val == null || val.isBlank() ? Optional.empty() : Optional.of(val);
        }
        return Optional.empty();
    }

    private static Mono<FilePart> extractFilePart(Map<String, Part> map, String name) {
        var part = map.get(name);
        if (part instanceof FilePart fp) {
            return Mono.just(fp);
        }
        return Mono.empty();
    }
}
