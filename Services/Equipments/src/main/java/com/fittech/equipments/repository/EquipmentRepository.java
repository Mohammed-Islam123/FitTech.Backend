package com.fittech.equipments.repository;

import com.fittech.equipments.model.Equipment;
import org.springframework.data.r2dbc.repository.Query;
import org.springframework.data.repository.reactive.ReactiveCrudRepository;
import reactor.core.publisher.Flux;
import reactor.core.publisher.Mono;

import java.util.UUID;

public interface EquipmentRepository extends ReactiveCrudRepository<Equipment, UUID> {

    Flux<Equipment> findByIsActiveTrueOrderByCreatedAtDesc();

    Mono<Equipment> findByIdAndIsActiveTrue(UUID id);

    @Query("UPDATE equipment SET is_active = false, updated_at = NOW() WHERE id = :id")
    Mono<Integer> softDeleteById(UUID id);

    @Query("UPDATE equipment SET status = :status, updated_at = NOW() WHERE id = :id")
    Mono<Integer> updateStatus(UUID id, String status);
}
