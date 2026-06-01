package com.fittech.workoutlogs.dto;

import lombok.AllArgsConstructor;
import lombok.Builder;
import lombok.Data;

import java.time.LocalDateTime;
import java.util.List;
import java.util.UUID;

@Data
@Builder
@AllArgsConstructor
public class WorkoutLogResponse {

    private UUID id;

    private UUID memberId;

    private UUID activitySessionId;

    private List<ExerciseResponse> exercises;

    private String notes;

    private LocalDateTime createdAt;

    private LocalDateTime updatedAt;
}
