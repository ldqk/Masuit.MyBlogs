还原数据库后使用以下脚本生成重置id序列的sql：
SELECT concat('SELECT setval(''"',c.relname,'"'', MAX("',SPLIT_PART(c.relname, '_', 2),'")) FROM "',SPLIT_PART(c.relname, '_', 1),'";') FROM pg_class c WHERE c.relkind = 'S';