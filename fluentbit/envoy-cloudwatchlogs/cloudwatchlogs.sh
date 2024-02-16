#!/bin/bash
set -e

export ENDPOINT_URL=http://localhost:4566
export AWS_PROFILE=localstack
loggroup_name="fluent-bit-cloudwatch"
region=ap-northeast-1

# list groups
echo "------------"
echo "List groups from CloudWatch Logs"
aws --endpoint-url=${ENDPOINT_URL} logs describe-log-groups --region "${region}"

# get streams
echo "------------"
echo "Get streams from CloudWatch Logs"
aws --endpoint-url=${ENDPOINT_URL} logs describe-log-streams --log-group-name "${loggroup_name}" --region "${region}" --output text

# tail logs
echo "------------"
echo "Tail logs from CloudWatch Logs"
aws --endpoint-url=${ENDPOINT_URL} logs tail "${loggroup_name}" --region "${region}"
