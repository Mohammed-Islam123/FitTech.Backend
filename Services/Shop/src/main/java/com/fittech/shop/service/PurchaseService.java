package com.fittech.shop.service;

import com.fittech.shop.dto.PurchaseResponse;
import com.fittech.shop.model.Purchase;
import com.fittech.shop.repository.ProductRepository;
import com.fittech.shop.repository.PurchaseRepository;
import org.springframework.http.HttpStatus;
import org.springframework.stereotype.Service;
import org.springframework.web.server.ResponseStatusException;
import reactor.core.publisher.Flux;
import reactor.core.publisher.Mono;

import java.time.LocalDateTime;
import java.util.UUID;

@Service
public class PurchaseService {

    private final PurchaseRepository purchaseRepository;
    private final ProductRepository productRepository;

    public PurchaseService(PurchaseRepository purchaseRepository,
                           ProductRepository productRepository) {
        this.purchaseRepository = purchaseRepository;
        this.productRepository = productRepository;
    }

    public Flux<PurchaseResponse> listAll() {
        return purchaseRepository.findAllByOrderByPurchasedAtDesc()
                .map(PurchaseResponse::from);
    }

    /**
     * Processes an on-site purchase: finds the product, checks stock,
     * deducts quantity, and saves a purchase record.
     * Member ID is optional (the admin may not associate a member).
     */
    public Mono<PurchaseResponse> process(UUID productId, UUID memberId, int quantity) {
        return productRepository.findByIdAndIsActiveTrue(productId)
                .switchIfEmpty(Mono.error(
                        new ResponseStatusException(HttpStatus.NOT_FOUND, "Product not found")))
                .flatMap(product -> {
                    if (product.getStock() < quantity) {
                        return Mono.error(new ResponseStatusException(
                                HttpStatus.BAD_REQUEST,
                                "Out of stock. Available: " + product.getStock()
                                        + ", requested: " + quantity));
                    }

                    product.setStock(product.getStock() - quantity);
                    product.setUpdatedAt(LocalDateTime.now());

                    var purchase = Purchase.builder()
                            .id(UUID.randomUUID())
                            .memberId(memberId)
                            .productId(productId)
                            .quantity(quantity)
                            .purchasedAt(LocalDateTime.now())
                            .build();

                    return productRepository.save(product)
                            .then(purchaseRepository.save(purchase));
                })
                .map(PurchaseResponse::from);
    }
}
