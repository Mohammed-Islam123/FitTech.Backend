package com.fittech.equipments.dto;

import jakarta.validation.constraints.NotBlank;

public class UpdateIssueStatusRequest {

    @NotBlank
    private String status;

    public String getStatus() { return status; }
    public void setStatus(String status) { this.status = status; }
}
