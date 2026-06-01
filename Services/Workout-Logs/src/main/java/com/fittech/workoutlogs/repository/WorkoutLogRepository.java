package com.fittech.workoutlogs.repository;

import com.fittech.workoutlogs.model.WorkoutLog;
import org.springframework.data.r2dbc.repository.Query;
import org.springframework.data.repository.reactive.ReactiveCrudRepository;
import reactor.core.publisher.Flux;

import java.time.LocalDateTime;
import java.util.UUID;

public interface WorkoutLogRepository extends ReactiveCrudRepository<WorkoutLog, UUID> {

    Flux<WorkoutLog> findByMemberIdOrderByCreatedAtDesc(UUID memberId);

    @Query("SELECT * FROM workout_logs WHERE member_id = :memberId AND created_at >= :from AND created_at <= :to ORDER BY created_at DESC")
    Flux<WorkoutLog> findByMemberIdAndCreatedAtBetween(UUID memberId, LocalDateTime from, LocalDateTime to);

    @Query("SELECT * FROM workout_logs WHERE activity_session_id = :sessionId ORDER BY created_at DESC")
    Flux<WorkoutLog> findByActivitySessionId(UUID sessionId);
}
