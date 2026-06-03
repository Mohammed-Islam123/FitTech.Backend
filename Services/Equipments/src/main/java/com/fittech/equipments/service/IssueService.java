package com.fittech.equipments.service;

import com.fittech.equipments.dto.CreateIssueRequest;
import com.fittech.equipments.dto.IssueResponse;
import com.fittech.equipments.dto.UpdateIssueStatusRequest;
import com.fittech.equipments.messaging.MessagePublisher;
import com.fittech.equipments.model.Issue;
import com.fittech.equipments.repository.EquipmentRepository;
import com.fittech.equipments.repository.IssueRepository;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.http.HttpStatus;
import org.springframework.stereotype.Service;
import org.springframework.web.server.ResponseStatusException;
import reactor.core.publisher.Flux;
import reactor.core.publisher.Mono;

import java.time.LocalDateTime;
import java.util.UUID;

@Service
public class IssueService {

    private static final Logger log = LoggerFactory.getLogger(IssueService.class);

    private static final String ADMIN_EMAIL = "admin@fittech.com";

    private final IssueRepository issueRepository;
    private final EquipmentRepository equipmentRepository;
    private final MessagePublisher messagePublisher;

    public IssueService(IssueRepository issueRepository,
                        EquipmentRepository equipmentRepository,
                        MessagePublisher messagePublisher) {
        this.issueRepository = issueRepository;
        this.equipmentRepository = equipmentRepository;
        this.messagePublisher = messagePublisher;
    }

    public Flux<IssueResponse> listAll() {
        return issueRepository.findAllByOrderByCreatedAtDesc()
                .map(IssueResponse::from);
    }

    public Mono<IssueResponse> report(UUID reporterId, CreateIssueRequest request) {
        // Verify equipment exists and is active
        return equipmentRepository.findByIdAndIsActiveTrue(request.getEquipmentId())
                .switchIfEmpty(Mono.error(
                        new ResponseStatusException(HttpStatus.NOT_FOUND, "Equipment not found or inactive")))
                .flatMap(equipment -> {
                    var now = LocalDateTime.now();
                    var issue = Issue.builder()
                            .id(UUID.randomUUID())
                            .equipmentId(request.getEquipmentId())
                            .reporterId(reporterId)
                            .description(request.getDescription())
                            .status("open")
                            .createdAt(now)
                            .build();

                    return issueRepository.save(issue)
                            .flatMap(saved -> {
                                // Notify admin via email
                                try {
                                    var subject = "Equipment Issue Reported: " + equipment.getName();
                                    var body = buildIssueEmailBody(saved, equipment.getName());
                                    messagePublisher.publishSendEmail(ADMIN_EMAIL, subject, body);
                                } catch (Exception e) {
                                    log.warn("Failed to publish email notification for issue {}", saved.getId(), e);
                                }
                                return Mono.just(saved);
                            });
                })
                .map(IssueResponse::from);
    }

    public Mono<IssueResponse> updateStatus(UUID id, UpdateIssueStatusRequest request) {
        return issueRepository.updateStatus(id, request.getStatus())
                .map(IssueResponse::from)
                .switchIfEmpty(Mono.error(
                        new ResponseStatusException(HttpStatus.NOT_FOUND, "Issue not found")));
    }

    // --- helpers ---

    private String buildIssueEmailBody(Issue issue, String equipmentName) {
        return """
                <h2>New Equipment Issue Reported</h2>
                <table>
                  <tr><td><strong>Equipment:</strong></td><td>%s</td></tr>
                  <tr><td><strong>Issue ID:</strong></td><td>%s</td></tr>
                  <tr><td><strong>Reporter ID:</strong></td><td>%s</td></tr>
                  <tr><td><strong>Description:</strong></td><td>%s</td></tr>
                  <tr><td><strong>Status:</strong></td><td>%s</td></tr>
                  <tr><td><strong>Reported At:</strong></td><td>%s</td></tr>
                </table>
                <p>Please review and take action.</p>
                """
                .formatted(
                        equipmentName,
                        issue.getId(),
                        issue.getReporterId(),
                        issue.getDescription(),
                        issue.getStatus(),
                        issue.getCreatedAt()
                );
    }
}
