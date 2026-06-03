package com.fittech.equipments.dto;

import com.fittech.equipments.model.Equipment;

import java.time.LocalDateTime;
import java.util.UUID;

public record EquipmentResponse(
        UUID id,
        String name,
        String description,
        String category,
        String status,
        String imageUrl,
        LocalDateTime createdAt,
        LocalDateTime updatedAt
) {
    public static EquipmentResponse from(Equipment e) {
        return new EquipmentResponse(
                e.getId(),
                e.getName(),
                e.getDescription(),
                e.getCategory(),
                e.getStatus(),
                e.getImageUrl(),
                e.getCreatedAt(),
                e.getUpdatedAt()
        );
    }
}
