-- PostgreSQL cleanup script for French Learning Platform
-- Run in pgAdmin Query Tool as DB admin.

BEGIN;

-- Option A: full data reset (keeps schema, clears all records, resets identities).
TRUNCATE TABLE
    favorites,
    test_attempts,
    profiles,
    words,
    tests,
    categories,
    users
RESTART IDENTITY CASCADE;

COMMIT;

-- Option B: remove only known demo users (run instead of Option A if needed).
-- BEGIN;
-- DELETE FROM users
-- WHERE lower(email) IN ('test_teacher@gmail.com', 'test_user@gmail.com');
-- COMMIT;
