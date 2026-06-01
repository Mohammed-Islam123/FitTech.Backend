package com.fittech.workoutlogs.dto;

import lombok.AllArgsConstructor;
import lombok.Builder;
import lombok.Data;

import java.util.UUID;

@Data
@Builder
@AllArgsConstructor
public class ExerciseResponse {

    private UUID id;

    private String name;

    private String description;

    private Double caloriesBurned;

    private Integer durationMinutes;
}
