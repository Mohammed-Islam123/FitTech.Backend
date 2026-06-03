package com.fittech.equipments.dto;

import jakarta.validation.constraints.NotBlank;

public class CreateEquipmentRequest {

    @NotBlank
    private String name;

    private String description;

    private String category;

    private String status;

    public String getName() { return name; }
    public void setName(String name) { this.name = name; }
    public String getDescription() { return description; }
    public void setDescription(String description) { this.description = description; }
    public String getCategory() { return category; }
    public void setCategory(String category) { this.category = category; }
    public String getStatus() { return status; }
    public void setStatus(String status) { this.status = status; }
}
