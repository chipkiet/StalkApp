DO $$
DECLARE
    f RECORD;
    v_conversation_id uuid;
    v_exists boolean;
BEGIN
    FOR f IN
        SELECT "RequesterId", "AddresseeId" FROM "Friendships" WHERE "Status" = 1
    LOOP
        SELECT EXISTS (
            SELECT 1
            FROM "Conversations" c
            JOIN "Participants" p1 ON c."Id" = p1."ConversationId"
            JOIN "Participants" p2 ON c."Id" = p2."ConversationId"
            WHERE c."Type" = 0
              AND p1."UserId" = f."RequesterId"
              AND p2."UserId" = f."AddresseeId"
        ) INTO v_exists;

        IF NOT v_exists THEN
            v_conversation_id := gen_random_uuid();
            
            INSERT INTO "Conversations" ("Id", "Title", "AvatarUrl", "Type", "CreatedAt")
            VALUES (v_conversation_id, NULL, NULL, 0, timezone('utc', now()));

            INSERT INTO "Participants" ("ConversationId", "UserId", "Role", "JoinedAt", "HasDeleted", "ClearedAt", "LastReadMessageId")
            VALUES (v_conversation_id, f."RequesterId", 1, timezone('utc', now()), false, NULL, NULL);

            INSERT INTO "Participants" ("ConversationId", "UserId", "Role", "JoinedAt", "HasDeleted", "ClearedAt", "LastReadMessageId")
            VALUES (v_conversation_id, f."AddresseeId", 0, timezone('utc', now()), false, NULL, NULL);
        END IF;
    END LOOP;
END;
$$;
