package com.fittech.shop.repository;

import com.fittech.shop.model.Purchase;
import org.springframework.data.repository.reactive.ReactiveCrudRepository;
import reactor.core.publisher.Flux;

import java.util.UUID;

public interface PurchaseRepository extends ReactiveCrudRepository<Purchase, UUID> {

    Flux<Purchase> findByMemberId(UUID memberId);
}
