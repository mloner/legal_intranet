#!/bin/bash

TG_TOKEN="1314602435:AAHOnekbN4fPFqxh3kXrMQZQrSbXFq39hok"
TG_CHAT="-476659704"
TG_URL="https://api.telegram.org/bot${TG_TOKEN}/sendMessage"

MESSAGE="Deploy status: $1

Project: Intranet Api Documentation
Details: $CI_PROJECT_URL/pipelines/$CI_PIPELINE_ID
Branch: $CI_COMMIT_REF_SLUG
Last commit: $CI_COMMIT_TITLE"

curl -s \
  --max-time 10 \
  --data-urlencode "chat_id=${TG_CHAT}" \
  --data-urlencode "disable_web_page_preview=1" \
  --data-urlencode "text=${MESSAGE}" \
  "${TG_URL}"
