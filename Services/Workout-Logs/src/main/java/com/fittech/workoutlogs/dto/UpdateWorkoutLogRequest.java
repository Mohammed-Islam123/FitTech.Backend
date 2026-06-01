package com.fittech.workoutlogs.dto;

import lombok.Data;

import java.util.List;

@Data
public class UpdateWorkoutLogRequest {

    private List<ExerciseRequest> exercises;

    private String notes;
}
