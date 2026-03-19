CREATE TABLE users (
    id uuid PRIMARY KEY,
    full_name varchar(150) NOT NULL,
    email varchar(200) NOT NULL UNIQUE,
    password_hash varchar(256) NOT NULL,
    created_at_utc timestamptz NOT NULL,
    updated_at_utc timestamptz NULL
);

CREATE TABLE resumes (
    id uuid PRIMARY KEY,
    user_id uuid NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    file_name varchar(260) NOT NULL,
    content_type varchar(150) NOT NULL,
    storage_path varchar(400) NOT NULL,
    extracted_text text NOT NULL,
    target_job_role varchar(120) NULL,
    status integer NOT NULL,
    created_at_utc timestamptz NOT NULL,
    updated_at_utc timestamptz NULL
);

CREATE TABLE resume_analysis (
    id uuid PRIMARY KEY,
    resume_id uuid NOT NULL UNIQUE REFERENCES resumes(id) ON DELETE CASCADE,
    score integer NOT NULL,
    summary text NOT NULL,
    overall_feedback text NOT NULL,
    skills jsonb NOT NULL,
    missing_skills jsonb NOT NULL,
    suggestions jsonb NOT NULL,
    matched_job_roles jsonb NOT NULL,
    raw_ai_response jsonb NOT NULL,
    created_at_utc timestamptz NOT NULL,
    updated_at_utc timestamptz NULL
);

CREATE INDEX ix_resumes_user_id ON resumes(user_id);
CREATE INDEX ix_resume_analysis_score ON resume_analysis(score);
CREATE INDEX ix_resume_analysis_skills_gin ON resume_analysis USING GIN (skills);
CREATE INDEX ix_resume_analysis_missing_skills_gin ON resume_analysis USING GIN (missing_skills);
