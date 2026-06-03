package com.fittech.equipments.dto;

import jakarta.validation.constraints.NotBlank;
import jakarta.validation.constraints.NotNull;

import java.util.UUID;

public class CreateIssueRequest {

    @NotNull
    private UUID equipmentId;

    @NotBlank
    private String description;

    public UUID getEquipmentId() { return equipmentId; }
    public void setEquipmentId(UUID equipmentId) { this.equipmentId = equipmentId; }
    public String getDescription() { return description; }
    public void setDescription(String description) { this.description = description; }
}
