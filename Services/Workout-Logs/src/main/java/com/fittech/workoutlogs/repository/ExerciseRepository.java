package com.fittech.workoutlogs.repository;

import com.fittech.workoutlogs.model.Exercise;
import org.springframework.data.r2dbc.repository.Modifying;
import org.springframework.data.r2dbc.repository.Query;
import org.springframework.data.repository.reactive.ReactiveCrudRepository;
import reactor.core.publisher.Flux;
import reactor.core.publisher.Mono;

import java.util.UUID;

public interface ExerciseRepository extends ReactiveCrudRepository<Exercise, UUID> {

    Flux<Exercise> findAllByWorkoutLogId(UUID workoutLogId);

    @Modifying
    @Query("DELETE FROM exercises WHERE workout_log_id = :workoutLogId")
    Mono<Void> deleteAllByWorkoutLogId(UUID workoutLogId);
}
