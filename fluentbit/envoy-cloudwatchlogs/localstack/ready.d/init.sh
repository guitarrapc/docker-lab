#!/bin/bash
set -eo pipefail

# NOTE: use awslocal to access localstack service.

region=ap-northeast-1
loggroup_names="fluent-bit-cloudwatch"

# create
while read -r cloudwatchlog_debuglog_name; do
  awslocal logs create-log-group --log-group-name "${cloudwatchlog_debuglog_name}" --region "${region}"
done <<< "${loggroup_names}"

# show
awslocal logs describe-log-groups --region "${region}" --page-size 50
