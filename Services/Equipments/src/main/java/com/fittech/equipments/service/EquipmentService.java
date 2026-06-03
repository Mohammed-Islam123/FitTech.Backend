package com.fittech.equipments.service;

import com.fittech.equipments.dto.CreateEquipmentRequest;
import com.fittech.equipments.dto.EquipmentResponse;
import com.fittech.equipments.dto.UpdateEquipmentRequest;
import com.fittech.equipments.model.Equipment;
import com.fittech.equipments.repository.EquipmentRepository;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.core.io.buffer.DataBufferUtils;
import org.springframework.http.HttpStatus;
import org.springframework.http.codec.multipart.FilePart;
import org.springframework.stereotype.Service;
import org.springframework.web.server.ResponseStatusException;
import reactor.core.publisher.Flux;
import reactor.core.publisher.Mono;

import java.io.IOException;
import java.nio.file.Files;
import java.nio.file.Path;
import java.nio.file.StandardCopyOption;
import java.time.LocalDateTime;
import java.util.UUID;

@Service
public class EquipmentService {

    private static final Logger log = LoggerFactory.getLogger(EquipmentService.class);

    private final EquipmentRepository repository;

    @Value("${app.upload.dir:uploads/images}")
    private String uploadDir;

    public EquipmentService(EquipmentRepository repository) {
        this.repository = repository;
    }

    public Flux<EquipmentResponse> listAll() {
        return repository.findByIsActiveTrueOrderByCreatedAtDesc()
                .map(EquipmentResponse::from);
    }

    public Mono<EquipmentResponse> getById(UUID id) {
        return repository.findByIdAndIsActiveTrue(id)
                .map(EquipmentResponse::from)
                .switchIfEmpty(Mono.error(
                        new ResponseStatusException(HttpStatus.NOT_FOUND, "Equipment not found")));
    }

    public Mono<EquipmentResponse> create(CreateEquipmentRequest request, Mono<FilePart> imagePart) {
        var equipmentId = UUID.randomUUID();
        var now = LocalDateTime.now();

        var equipment = Equipment.builder()
                .id(equipmentId)
                .name(request.getName())
                .description(request.getDescription())
                .category(request.getCategory())
                .status(request.getStatus() != null ? request.getStatus() : "available")
                .isActive(true)
                .createdAt(now)
                .build();

        Mono<String> imageMono = imagePart != null
                ? saveImage(equipmentId, imagePart)
                : Mono.just((String) null);

        return imageMono
                .flatMap(imageUrl -> {
                    if (imageUrl != null) equipment.setImageUrl(imageUrl);
                    return repository.save(equipment);
                })
                .map(EquipmentResponse::from);
    }

    public Mono<EquipmentResponse> update(UUID id, UpdateEquipmentRequest request, Mono<FilePart> imagePart) {
        return repository.findByIdAndIsActiveTrue(id)
                .switchIfEmpty(Mono.error(
                        new ResponseStatusException(HttpStatus.NOT_FOUND, "Equipment not found")))
                .flatMap(existing -> {
                    if (request.getName() != null) existing.setName(request.getName());
                    if (request.getDescription() != null) existing.setDescription(request.getDescription());
                    if (request.getCategory() != null) existing.setCategory(request.getCategory());
                    if (request.getStatus() != null) existing.setStatus(request.getStatus());
                    existing.setUpdatedAt(LocalDateTime.now());

                    Mono<String> imageMono = imagePart != null
                            ? saveImage(id, imagePart)
                            : Mono.just(existing.getImageUrl());

                    return imageMono
                            .flatMap(imageUrl -> {
                                existing.setImageUrl(imageUrl);
                                return repository.save(existing);
                            });
                })
                .map(EquipmentResponse::from);
    }

    public Mono<Void> softDelete(UUID id) {
        return repository.findByIdAndIsActiveTrue(id)
                .switchIfEmpty(Mono.error(
                        new ResponseStatusException(HttpStatus.NOT_FOUND, "Equipment not found")))
                .flatMap(e -> repository.softDeleteById(e.getId()))
                .then();
    }

    // --- helpers ---

    private Mono<String> saveImage(UUID equipmentId, Mono<FilePart> imagePart) {
        return imagePart.flatMap(part -> {
            var ext = extractExtension(part.filename());
            var filename = equipmentId + ext;
            var dest = Path.of(uploadDir, filename);

            return DataBufferUtils
                    .join(part.content())
                    .flatMap(buffer -> {
                        try {
                            Files.createDirectories(dest.getParent());
                            var data = new byte[buffer.readableByteCount()];
                            buffer.read(data);
                            DataBufferUtils.release(buffer);
                            Files.write(dest, data);
                            log.info("Saved image {} ({} bytes)", filename, data.length);
                            return Mono.just("/uploads/" + filename);
                        } catch (IOException e) {
                            DataBufferUtils.release(buffer);
                            return Mono.error(new RuntimeException("Failed to save image", e));
                        }
                    });
        });
    }

    private static String extractExtension(String filename) {
        if (filename == null || !filename.contains(".")) return ".jpg";
        return filename.substring(filename.lastIndexOf('.'));
    }
}
