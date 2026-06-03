package com.fittech.equipments.repository;

import com.fittech.equipments.model.Issue;
import org.springframework.data.r2dbc.repository.Query;
import org.springframework.data.repository.reactive.ReactiveCrudRepository;
import reactor.core.publisher.Flux;
import reactor.core.publisher.Mono;

import java.util.UUID;

public interface IssueRepository extends ReactiveCrudRepository<Issue, UUID> {

    Flux<Issue> findAllByOrderByCreatedAtDesc();

    @Query("UPDATE issues SET status = :status, updated_at = NOW() WHERE id = :id RETURNING *")
    Mono<Issue> updateStatus(UUID id, String status);
}
