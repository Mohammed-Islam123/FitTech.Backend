package com.fittech.shop.repository;

import com.fittech.shop.model.Product;
import org.springframework.data.r2dbc.repository.Query;
import org.springframework.data.repository.reactive.ReactiveCrudRepository;
import reactor.core.publisher.Flux;
import reactor.core.publisher.Mono;

import java.util.UUID;

public interface ProductRepository extends ReactiveCrudRepository<Product, UUID> {

    Flux<Product> findByIsActiveTrueOrderByCreatedAtDesc();

    Mono<Product> findByIdAndIsActiveTrue(UUID id);

    @Query("UPDATE products SET is_active = false, updated_at = NOW() WHERE id = :id")
    Mono<Integer> softDeleteById(UUID id);
}
