#!/bin/bash
set -e

export ENDPOINT_URL=http://localhost:4566
export AWS_PROFILE=localstack
export BUCKET_NAME="accesslog-bucket"
region=ap-northeast-1

# 5) list objects
echo "------------"
echo "List messages from S3"
aws --endpoint-url=${ENDPOINT_URL} s3api list-objects-v2 --bucket "${BUCKET_NAME}"

# 6) get objects
echo "------------"
echo "Get message from S3"
objects=$(aws --endpoint-url=${ENDPOINT_URL} s3api list-objects-v2 --bucket "${BUCKET_NAME}" --query "Contents[].Key" --output text)
for object in ${objects}; do
    content=$(aws --endpoint-url=${ENDPOINT_URL} s3 cp "s3://${BUCKET_NAME}/${object}" -)
    echo -e "~~~~~~~~~~~~~~~\nkey => ${object}, object => ${content}"
done
