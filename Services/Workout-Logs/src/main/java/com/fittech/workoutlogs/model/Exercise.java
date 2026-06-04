package com.fittech.workoutlogs.model;

import lombok.AllArgsConstructor;
import lombok.Builder;
import lombok.Data;
import lombok.NoArgsConstructor;
import org.springframework.data.annotation.Id;
import org.springframework.data.annotation.Transient;
import org.springframework.data.domain.Persistable;
import org.springframework.data.relational.core.mapping.Table;

import java.util.UUID;

@Data
@Builder
@NoArgsConstructor
@AllArgsConstructor
@Table("exercises")
public class Exercise implements Persistable<UUID> {

    @Id
    private UUID id;

    private UUID workoutLogId;

    private String name;

    private String description;

    private Double caloriesBurned;

    private Integer durationMinutes;

    @Transient
    @Builder.Default
    private boolean isNew = false;

    @Override
    public boolean isNew() {
        return isNew;
    }
}
