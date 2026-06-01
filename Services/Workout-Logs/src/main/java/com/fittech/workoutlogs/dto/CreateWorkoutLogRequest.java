package com.fittech.workoutlogs.dto;

import lombok.Data;

import java.util.List;
import java.util.UUID;

@Data
public class CreateWorkoutLogRequest {

    private UUID memberId;

    private UUID activitySessionId;

    private List<ExerciseRequest> exercises;

    private String notes;
}
