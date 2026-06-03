package com.fittech.equipments.controller;

import com.fittech.equipments.dto.EquipmentResponse;
import com.fittech.equipments.dto.CreateEquipmentRequest;
import com.fittech.equipments.dto.UpdateEquipmentRequest;
import com.fittech.equipments.service.EquipmentService;
import org.springframework.http.HttpStatus;
import org.springframework.http.MediaType;
import org.springframework.http.codec.multipart.FilePart;
import org.springframework.http.codec.multipart.FormFieldPart;
import org.springframework.http.codec.multipart.Part;
import org.springframework.web.bind.annotation.*;
import reactor.core.publisher.Flux;
import reactor.core.publisher.Mono;

import java.util.Map;
import java.util.UUID;

@RestController
@RequestMapping("/api/equipments")
public class EquipmentController {

    private final EquipmentService equipmentService;

    public EquipmentController(EquipmentService equipmentService) {
        this.equipmentService = equipmentService;
    }

    @GetMapping
    public Flux<EquipmentResponse> listAll() {
        return equipmentService.listAll();
    }

    @GetMapping("/{id}")
    public Mono<EquipmentResponse> getById(@PathVariable UUID id) {
        return equipmentService.getById(id);
    }

    @PostMapping(consumes = MediaType.MULTIPART_FORM_DATA_VALUE)
    @ResponseStatus(HttpStatus.CREATED)
    public Mono<EquipmentResponse> create(@RequestBody Flux<Part> parts) {
        return parts.collectMap(Part::name).flatMap(map -> {
            var req = new CreateEquipmentRequest();
            extractFormField(map, "name").ifPresent(req::setName);
            extractFormField(map, "description").ifPresent(req::setDescription);
            extractFormField(map, "category").ifPresent(req::setCategory);
            extractFormField(map, "status").ifPresent(req::setStatus);

            if (req.getName() == null || req.getName().isBlank()) {
                return Mono.error(new IllegalArgumentException("name is required"));
            }

            Mono<FilePart> image = extractFilePart(map, "image");
            return equipmentService.create(req, image);
        });
    }

    @PutMapping(value = "/{id}", consumes = MediaType.MULTIPART_FORM_DATA_VALUE)
    public Mono<EquipmentResponse> update(
            @PathVariable UUID id,
            @RequestBody Flux<Part> parts) {
        return parts.collectMap(Part::name).flatMap(map -> {
            var req = new UpdateEquipmentRequest();
            extractFormField(map, "name").ifPresent(req::setName);
            extractFormField(map, "description").ifPresent(req::setDescription);
            extractFormField(map, "category").ifPresent(req::setCategory);
            extractFormField(map, "status").ifPresent(req::setStatus);

            Mono<FilePart> image = extractFilePart(map, "image");
            return equipmentService.update(id, req, image);
        });
    }

    @DeleteMapping("/{id}")
    @ResponseStatus(HttpStatus.NO_CONTENT)
    public Mono<Void> delete(@PathVariable UUID id) {
        return equipmentService.softDelete(id);
    }

    // --- helpers ---

    private static java.util.Optional<String> extractFormField(Map<String, Part> map, String name) {
        var part = map.get(name);
        if (part instanceof FormFieldPart ff) {
            var val = ff.value();
            return val == null || val.isBlank() ? java.util.Optional.empty() : java.util.Optional.of(val);
        }
        return java.util.Optional.empty();
    }

    private static Mono<FilePart> extractFilePart(Map<String, Part> map, String name) {
        var part = map.get(name);
        if (part instanceof FilePart fp) {
            return Mono.just(fp);
        }
        return Mono.empty();
    }
}
