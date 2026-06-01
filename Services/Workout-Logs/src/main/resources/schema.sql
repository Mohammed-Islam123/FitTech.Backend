CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

CREATE TABLE IF NOT EXISTS workout_logs (
    id UUID PRIMARY KEY,
    member_id UUID NOT NULL,
    activity_session_id UUID NOT NULL,
    notes TEXT,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL,
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL
);

CREATE TABLE IF NOT EXISTS exercises (
    id UUID PRIMARY KEY,
    workout_log_id UUID NOT NULL REFERENCES workout_logs(id) ON DELETE CASCADE,
    name VARCHAR(255) NOT NULL,
    description TEXT,
    calories_burned DOUBLE PRECISION,
    duration_minutes INTEGER
);
