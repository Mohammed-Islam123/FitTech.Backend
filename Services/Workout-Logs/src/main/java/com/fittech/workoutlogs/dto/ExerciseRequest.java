package com.fittech.workoutlogs.dto;

import lombok.Data;

@Data
public class ExerciseRequest {

    private String name;

    private String description;

    private Double caloriesBurned;

    private Integer durationMinutes;
}
