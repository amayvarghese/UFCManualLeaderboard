-- Supabase RLS Policy for DELETE operations
-- Run this in your Supabase SQL Editor to enable delete functionality

-- Create a policy that allows everyone to delete entries
CREATE POLICY "Allow public delete access" ON "Manual Leaderboard"
  FOR DELETE
  USING (true);

-- Alternative: If the above doesn't work, try this more permissive policy
-- DROP POLICY IF EXISTS "Allow public delete access" ON "Manual Leaderboard";
-- CREATE POLICY "Allow public delete access" ON "Manual Leaderboard"
--   FOR ALL
--   USING (true)
--   WITH CHECK (true);

-- Check if the policy was created successfully
SELECT * FROM pg_policies WHERE tablename = 'Manual Leaderboard';
