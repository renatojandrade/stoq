-- =====================================================================
-- STEP 1 — Run against: stoq-identity
--
-- Paste the user UUID below, then run the script.
-- Before committing, note the HouseholdId values printed by the first
-- SELECT — you will need them for steps 2 and 3.
-- =====================================================================

CREATE TEMP TABLE _v AS SELECT 'paste-uuid-here'::uuid AS user_id;

-- Shows who will be deleted and which households they belong to
SELECT u."Id", u."Email", u."DisplayName"
  FROM identity."AspNetUsers" u
 WHERE u."Id" = (SELECT user_id FROM _v);

SELECT hm."HouseholdId", h."Name"
  FROM identity."HouseholdMembers" hm
  JOIN identity."Households" h ON h."Id" = hm."HouseholdId"
 WHERE hm."UserId" = (SELECT user_id FROM _v);

-- 1. Household memberships
DELETE FROM identity."HouseholdMembers"
 WHERE "UserId" = (SELECT user_id FROM _v);

-- 2. Households where this user was the sole member
DELETE FROM identity."Households"
 WHERE "Id" NOT IN (SELECT DISTINCT "HouseholdId" FROM identity."HouseholdMembers")
   AND "CreatedBy" = (SELECT user_id FROM _v);

-- 3. Refresh tokens
DELETE FROM identity."RefreshTokens"
 WHERE "UserId" = (SELECT user_id FROM _v);

-- 4. ASP.NET Identity records
DELETE FROM identity."AspNetUserClaims" WHERE "UserId" = (SELECT user_id FROM _v);
DELETE FROM identity."AspNetUserLogins" WHERE "UserId" = (SELECT user_id FROM _v);
DELETE FROM identity."AspNetUserTokens" WHERE "UserId" = (SELECT user_id FROM _v);

-- 5. The user
DELETE FROM identity."AspNetUsers"
 WHERE "Id" = (SELECT user_id FROM _v);
