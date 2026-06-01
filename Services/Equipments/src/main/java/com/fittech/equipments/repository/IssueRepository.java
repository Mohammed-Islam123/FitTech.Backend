package com.fittech.equipments.repository;

import com.fittech.equipments.model.Issue;
import org.springframework.data.repository.reactive.ReactiveCrudRepository;

import java.util.UUID;

public interface IssueRepository extends ReactiveCrudRepository<Issue, UUID> {
}
