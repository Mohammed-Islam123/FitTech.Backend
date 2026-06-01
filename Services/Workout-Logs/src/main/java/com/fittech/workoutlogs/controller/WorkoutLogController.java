package com.fittech.workoutlogs.controller;

import com.fittech.workoutlogs.dto.CreateWorkoutLogRequest;
import com.fittech.workoutlogs.dto.UpdateWorkoutLogRequest;
import com.fittech.workoutlogs.dto.WorkoutLogResponse;
import com.fittech.workoutlogs.service.WorkoutLogService;
import lombok.RequiredArgsConstructor;
import org.springframework.format.annotation.DateTimeFormat;
import org.springframework.http.HttpStatus;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;
import reactor.core.publisher.Flux;
import reactor.core.publisher.Mono;

import java.net.URI;
import java.time.LocalDateTime;
import java.util.UUID;

@RestController
@RequestMapping("/api/workout-logs")
@RequiredArgsConstructor
public class WorkoutLogController {

    private final WorkoutLogService workoutLogService;

    @PostMapping
    public Mono<ResponseEntity<WorkoutLogResponse>> create(@RequestBody Mono<CreateWorkoutLogRequest> request) {
        return request
                .flatMap(workoutLogService::create)
                .map(response -> ResponseEntity
                        .created(URI.create("/api/workout-logs/" + response.getId()))
                        .body(response));
    }

    @GetMapping("/{id}")
    public Mono<ResponseEntity<WorkoutLogResponse>> getById(@PathVariable UUID id) {
        return workoutLogService.getById(id)
                .map(ResponseEntity::ok)
                .defaultIfEmpty(ResponseEntity.notFound().build());
    }

    @GetMapping
    public Flux<WorkoutLogResponse> list(
            @RequestParam UUID memberId,
            @RequestParam(required = false) @DateTimeFormat(iso = DateTimeFormat.ISO.DATE_TIME) LocalDateTime from,
            @RequestParam(required = false) @DateTimeFormat(iso = DateTimeFormat.ISO.DATE_TIME) LocalDateTime to,
            @RequestParam(required = false) UUID activitySessionId) {
        return workoutLogService.list(memberId, from, to, activitySessionId);
    }

    @PutMapping("/{id}")
    public Mono<ResponseEntity<WorkoutLogResponse>> update(
            @PathVariable UUID id,
            @RequestBody Mono<UpdateWorkoutLogRequest> request) {
        return request
                .flatMap(req -> workoutLogService.update(id, req))
                .map(ResponseEntity::ok)
                .defaultIfEmpty(ResponseEntity.notFound().build());
    }

    @ExceptionHandler(IllegalArgumentException.class)
    public ResponseEntity<String> handleBadRequest(IllegalArgumentException ex) {
        return ResponseEntity.status(HttpStatus.BAD_REQUEST).body(ex.getMessage());
    }
}
