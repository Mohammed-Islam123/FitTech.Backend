package com.fittech.equipments.dto;

import com.fittech.equipments.model.Issue;

import java.time.LocalDateTime;
import java.util.UUID;

public record IssueResponse(
        UUID id,
        UUID equipmentId,
        UUID reporterId,
        String description,
        String status,
        LocalDateTime createdAt,
        LocalDateTime updatedAt
) {
    public static IssueResponse from(Issue issue) {
        return new IssueResponse(
                issue.getId(),
                issue.getEquipmentId(),
                issue.getReporterId(),
                issue.getDescription(),
                issue.getStatus(),
                issue.getCreatedAt(),
                issue.getUpdatedAt()
        );
    }
}
