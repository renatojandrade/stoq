-- =====================================================================
-- STEP 2 — Run against: stoq-catalog
--
-- Paste the HouseholdId(s) from step 1 below.
-- If the user belonged to multiple households, add them as extra rows.
-- =====================================================================

CREATE TEMP TABLE _households AS
SELECT 'paste-household-uuid-here'::uuid AS household_id;
-- INSERT INTO _households VALUES ('second-household-uuid-if-any'::uuid);

DELETE FROM catalog."Products"
 WHERE "HouseholdId" IN (SELECT household_id FROM _households);
