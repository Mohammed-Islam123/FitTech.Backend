package com.fittech.workoutlogs.service;

import com.fittech.workoutlogs.dto.*;
import com.fittech.workoutlogs.model.Exercise;
import com.fittech.workoutlogs.model.WorkoutLog;
import com.fittech.workoutlogs.repository.ExerciseRepository;
import com.fittech.workoutlogs.repository.WorkoutLogRepository;
import lombok.RequiredArgsConstructor;
import org.springframework.stereotype.Service;
import reactor.core.publisher.Flux;
import reactor.core.publisher.Mono;

import java.time.LocalDateTime;
import java.util.List;
import java.util.UUID;

@Service
@RequiredArgsConstructor
public class WorkoutLogService {

    private final WorkoutLogRepository workoutLogRepository;
    private final ExerciseRepository exerciseRepository;

    public Mono<WorkoutLogResponse> create(CreateWorkoutLogRequest request) {
        var now = LocalDateTime.now();
        var logId = UUID.randomUUID();

        var workoutLog = WorkoutLog.builder()
                .id(logId)
                .memberId(request.getMemberId())
                .activitySessionId(request.getActivitySessionId())
                .notes(request.getNotes())
                .createdAt(now)
                .updatedAt(now)
                .isNew(true)
                .build();

        var exercises = request.getExercises().stream()
                .map(e -> Exercise.builder()
                        .id(UUID.randomUUID())
                        .workoutLogId(logId)
                        .name(e.getName())
                        .description(e.getDescription())
                        .caloriesBurned(e.getCaloriesBurned())
                        .durationMinutes(e.getDurationMinutes())
                        .isNew(true)
                        .build())
                .toList();

        return workoutLogRepository.save(workoutLog)
                .thenMany(exerciseRepository.saveAll(exercises))
                .collectList()
                .thenReturn(buildResponse(workoutLog, exercises));
    }

    public Mono<WorkoutLogResponse> getById(UUID id) {
        return workoutLogRepository.findById(id)
                .flatMap(log -> exerciseRepository.findAllByWorkoutLogId(id)
                        .collectList()
                        .map(exercises -> buildResponse(log, exercises)));
    }

    public Flux<WorkoutLogResponse> list(UUID memberId, LocalDateTime from, LocalDateTime to, UUID activitySessionId) {
        Flux<WorkoutLog> logs;

        if (from != null && to != null) {
            logs = workoutLogRepository.findByMemberIdAndCreatedAtBetween(memberId, from, to);
        } else if (activitySessionId != null) {
            logs = workoutLogRepository.findByActivitySessionId(activitySessionId);
        } else {
            logs = workoutLogRepository.findByMemberIdOrderByCreatedAtDesc(memberId);
        }

        return logs.flatMap(log -> exerciseRepository.findAllByWorkoutLogId(log.getId())
                .collectList()
                .map(exercises -> buildResponse(log, exercises)));
    }

    public Mono<WorkoutLogResponse> update(UUID id, UpdateWorkoutLogRequest request) {
        return workoutLogRepository.findById(id)
                .flatMap(log -> {
                    var updatedLog = WorkoutLog.builder()
                            .id(log.getId())
                            .memberId(log.getMemberId())
                            .activitySessionId(log.getActivitySessionId())
                            .notes(request.getNotes())
                            .createdAt(log.getCreatedAt())
                            .updatedAt(LocalDateTime.now())
                            .build();

                    var exercises = request.getExercises().stream()
                            .map(e -> Exercise.builder()
                                    .id(UUID.randomUUID())
                                    .workoutLogId(log.getId())
                                    .name(e.getName())
                                    .description(e.getDescription())
                                    .caloriesBurned(e.getCaloriesBurned())
                                    .durationMinutes(e.getDurationMinutes())
                                    .isNew(true)
                                    .build())
                            .toList();

                    return exerciseRepository.deleteAllByWorkoutLogId(id)
                            .then(workoutLogRepository.save(updatedLog))
                            .thenMany(exerciseRepository.saveAll(exercises))
                            .collectList()
                            .thenReturn(buildResponse(updatedLog, exercises));
                });
    }

    private WorkoutLogResponse buildResponse(WorkoutLog log, List<Exercise> exercises) {
        var exerciseResponses = exercises.stream()
                .map(e -> ExerciseResponse.builder()
                        .id(e.getId())
                        .name(e.getName())
                        .description(e.getDescription())
                        .caloriesBurned(e.getCaloriesBurned())
                        .durationMinutes(e.getDurationMinutes())
                        .build())
                .toList();

        return WorkoutLogResponse.builder()
                .id(log.getId())
                .memberId(log.getMemberId())
                .activitySessionId(log.getActivitySessionId())
                .exercises(exerciseResponses)
                .notes(log.getNotes())
                .createdAt(log.getCreatedAt())
                .updatedAt(log.getUpdatedAt())
                .build();
    }
}
