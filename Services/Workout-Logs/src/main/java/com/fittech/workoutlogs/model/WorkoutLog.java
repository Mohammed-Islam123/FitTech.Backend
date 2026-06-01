package com.fittech.workoutlogs.model;

import lombok.AllArgsConstructor;
import lombok.Builder;
import lombok.Data;
import lombok.NoArgsConstructor;
import org.springframework.data.annotation.Id;
import org.springframework.data.relational.core.mapping.Table;

import java.time.LocalDateTime;
import java.util.UUID;

@Data
@Builder
@NoArgsConstructor
@AllArgsConstructor
@Table("workout_logs")
public class WorkoutLog {

    @Id
    private UUID id;

    private UUID memberId;

    private UUID activitySessionId;

    private String notes;

    private LocalDateTime createdAt;

    private LocalDateTime updatedAt;
}
