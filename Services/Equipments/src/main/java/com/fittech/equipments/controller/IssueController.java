package com.fittech.equipments.controller;

import com.fittech.equipments.dto.CreateIssueRequest;
import com.fittech.equipments.dto.IssueResponse;
import com.fittech.equipments.dto.UpdateIssueStatusRequest;
import com.fittech.equipments.service.IssueService;
import jakarta.validation.Valid;
import org.springframework.http.HttpStatus;
import org.springframework.security.core.annotation.AuthenticationPrincipal;
import org.springframework.security.oauth2.jwt.Jwt;
import org.springframework.web.bind.annotation.*;
import reactor.core.publisher.Flux;
import reactor.core.publisher.Mono;

import java.util.UUID;

@RestController
@RequestMapping("/api/issues")
public class IssueController {

    private final IssueService issueService;

    public IssueController(IssueService issueService) {
        this.issueService = issueService;
    }

    @GetMapping
    public Flux<IssueResponse> listAll() {
        return issueService.listAll();
    }

    @PostMapping
    @ResponseStatus(HttpStatus.CREATED)
    public Mono<IssueResponse> report(
            @Valid @RequestBody CreateIssueRequest request,
            @AuthenticationPrincipal Jwt jwt) {
        var reporterId = UUID.fromString(jwt.getSubject());
        return issueService.report(reporterId, request);
    }

    @PatchMapping("/{id}/status")
    public Mono<IssueResponse> updateStatus(
            @PathVariable UUID id,
            @Valid @RequestBody UpdateIssueStatusRequest request) {
        return issueService.updateStatus(id, request);
    }
}
