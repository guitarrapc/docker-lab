#!/bin/bash
set -e

export ENDPOINT_URL=http://localhost:4566
export AWS_PROFILE=localstack
export DELIVERY_NAME=foo-delivery
export BUCKET_NAME="hello-bucket"
export IAM_ROLE_NAME="hello-delivery-role"
region=ap-northeast-1

# 1) create delivery stream
echo "------------"
echo "Create Firehose delivery stream"
firehose_json=$(cat << EOF
{
    "DeliveryStreamName": "${DELIVERY_NAME}",
    "S3DestinationConfiguration": {
        "BucketARN": "arn:aws:s3:::${BUCKET_NAME}",
        "BufferingHints": {
            "SizeInMBs": 5,
            "IntervalInSeconds": 300
        },
        "CompressionFormat": "GZIP",
        "Prefix": "${DELIVERY_NAME}",
        "RoleARN": "arn:aws:iam::000000000000:role/${IAM_ROLE_NAME}"
    }
}
EOF
)
aws --endpoint-url=${ENDPOINT_URL} firehose create-delivery-stream --cli-input-json "${firehose_json}" --region ${region}

# 2) list delivery streams
echo "------------"
echo "List Firehose delivery streams"
aws --endpoint-url=${ENDPOINT_URL} firehose list-delivery-streams --region "${region}"

# 3) get delivery stream detail
echo "------------"
echo "Describe Firehose delivery stream"
aws --endpoint-url=${ENDPOINT_URL} firehose describe-delivery-stream --delivery-stream-name "${DELIVERY_NAME}" --region "${region}"

# 4) put message to delivery stream
echo "------------"
echo "Put messages to Firehose"
aws --endpoint-url=${ENDPOINT_URL} firehose put-record --delivery-stream-name "${DELIVERY_NAME}" --record="{\"Data\":\"$(echo "some data 1" | base64 - -w0)\"}" --region "${region}"
aws --endpoint-url=${ENDPOINT_URL} firehose put-record --delivery-stream-name "${DELIVERY_NAME}" --record="{\"Data\":\"$(echo "japanese 日本語だぞ☆" | base64 - -w0)\"}" --region "${region}"

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
    echo "key => ${object}, object => ${content}"
done
