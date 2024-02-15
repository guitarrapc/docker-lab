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
streams=$(aws --endpoint-url=${ENDPOINT_URL} logs describe-log-streams --log-group-name "${loggroup_name}" --region "${region}" --output text)
for object in ${objects}; do
    content=$(aws --endpoint-url=${ENDPOINT_URL} s3 cp "s3://${BUCKET_NAME}/${object}" -)
    echo -e "~~~~~~~~~~~~~~~\nkey => ${object}, object => ${content}"
done
