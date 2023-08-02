#!/bin/bash
set -e

export ENDPOINT_URL=http://localhost:4566
export AWS_PROFILE=localstack
export QUEUE_NAME=foo-queue
region=ap-northeast-1

# 1) create queue
echo "------------"
echo "Create SQS queue"
aws --endpoint-url=${ENDPOINT_URL} sqs create-queue --queue-name "${QUEUE_NAME}" --region "${region}"

# 2) list queues
echo "------------"
echo "List SQS queues"
aws --endpoint-url=${ENDPOINT_URL} sqs list-queues --region "${region}"

# 3) get queue url
echo "------------"
echo "List SQS queues"
queue=$(aws --endpoint-url=${ENDPOINT_URL} sqs get-queue-url --queue-name "${QUEUE_NAME}" --region "${region}")
echo "${queue}"
queue_url=$(echo "${queue}" | jq -r '.QueueUrl')

# 4) set queue attributes
echo "------------"
echo "Set SQS queue attributes, VisibilityTimeout=10"
aws --endpoint-url=${ENDPOINT_URL} sqs set-queue-attributes --queue-url "${queue_url}" --attributes VisibilityTimeout=10 --region "${region}"

# 5) get queue attributes
echo "------------"
echo "Get SQS queue attributes"
aws --endpoint-url=${ENDPOINT_URL} sqs get-queue-attributes --queue-url "${queue_url}" --region "${region}" --attribute-names \
  Policy \
  VisibilityTimeout \
  MaximumMessageSize \
  MessageRetentionPeriod \
  ApproximateNumberOfMessages \
  ApproximateNumberOfMessagesNotVisible \
  CreatedTimestamp \
  LastModifiedTimestamp \
  QueueArn \
  ApproximateNumberOfMessagesDelayed \
  DelaySeconds \
  ReceiveMessageWaitTimeSeconds \
  RedrivePolicy

# 6) send messages
echo "------------"
echo "Sending messages to SQS"
aws --endpoint-url=${ENDPOINT_URL} sqs send-message --queue-url "${queue_url}" --message-body "{""name"": ""dummy""}" --region "${region}"
aws --endpoint-url=${ENDPOINT_URL} sqs send-message --queue-url "${queue_url}" --message-body "{""name"": ""dummy2""}" --region "${region}"

# 7) get message
echo "------------"
echo "Get message from SQS"
recieve1=$(aws --endpoint-url=${ENDPOINT_URL} sqs receive-message --queue-url "${queue_url}" --region "${region}")
receipt1=$(echo $recieve1 | jq -r '.Messages[].ReceiptHandle')
body1=$(echo $recieve1 | jq -r '.Messages[].Body')
echo "${recieve1}"
echo "1st received body '${body1}'"

recieve2=$(aws --endpoint-url=${ENDPOINT_URL} sqs receive-message --queue-url "${queue_url}" --region "${region}")
receipt2=$(echo "$recieve2" | jq -r '.Messages[].ReceiptHandle')
body2=$(echo "$recieve2" | jq -r '.Messages[].Body')
echo "${recieve2}"
echo "2nd received body '${body2}'"

# 8) change message visibility
echo "------------"
echo "Change message visibility to 60s"
aws --endpoint-url=${ENDPOINT_URL} sqs change-message-visibility --queue-url "${queue_url}" --receipt-handle "${receipt1}" --visibility-timeout 60 --region "${region}"

# 9) get message (must be empty)
echo "------------"
echo "Get message must be empty as of visible timeout"
aws --endpoint-url=${ENDPOINT_URL} sqs receive-message --queue-url "${queue_url}" --region "${region}"

# 10) delete message
echo "------------"
echo "Delete messages from SQS"
aws --endpoint-url=${ENDPOINT_URL} sqs delete-message --queue-url "${queue_url}" --receipt-handle "${receipt1}" --region "${region}"
aws --endpoint-url=${ENDPOINT_URL} sqs delete-message --queue-url "${queue_url}" --receipt-handle "${receipt2}" --region "${region}"

# 11) purge message
echo "------------"
echo "Purge messages from SQS"
aws --endpoint-url=${ENDPOINT_URL} sqs purge-queue --queue-url "${queue_url}" --region "${region}"
