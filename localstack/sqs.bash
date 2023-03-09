#!/bin/bash
set -e

export QUEUE_NAME=foo-queue
export AWS_PROFILE=localstack

# create queue
aws --endpoint-url=http://localhost:4566 sqs create-queue --queue-name "${QUEUE_NAME}"

# list queues
echo "------------"
echo "List SQS queues"
aws --endpoint-url=http://localhost:4566 sqs list-queues

# send messages
echo "------------"
echo "Sending messages to SQS"
aws --endpoint-url=http://localhost:4566 sqs send-message --queue-url "http://localhost:4566/000000000000/${QUEUE_NAME}" --message-body "{""name"": ""dummy""}"
aws --endpoint-url=http://localhost:4566 sqs send-message --queue-url "http://localhost:4566/000000000000/${QUEUE_NAME}" --message-body "{""name"": ""dummy2""}"

# get message
echo "------------"
echo "Get message from SQS"
recieve1=$(aws --endpoint-url=http://localhost:4566 sqs receive-message --queue-url "http://localhost:4566/000000000000/${QUEUE_NAME}")
receipt1=$(echo $recieve1 | jq -r '.Messages[].ReceiptHandle')
body1=$(echo $recieve1 | jq -r '.Messages[].Body')
echo "${recieve1}"
echo "1st received body '${body1}'"

recieve2=$(aws --endpoint-url=http://localhost:4566 sqs receive-message --queue-url "http://localhost:4566/000000000000/${QUEUE_NAME}")
receipt2=$(echo "$recieve2" | jq -r '.Messages[].ReceiptHandle')
body2=$(echo "$recieve2" | jq -r '.Messages[].Body')
echo "${recieve2}"
echo "2nd received body '${body2}'"

# get message (must be empty)
echo "------------"
echo "Get message must be empty as of visible timeout"
aws --endpoint-url=http://localhost:4566 sqs receive-message --queue-url "http://localhost:4566/000000000000/${QUEUE_NAME}"

# delete message
echo "------------"
echo "Delete messages from SQS"
aws --endpoint-url=http://localhost:4566 sqs delete-message --queue-url "http://localhost:4566/000000000000/${QUEUE_NAME}" --receipt-handle "${receipt1}"
aws --endpoint-url=http://localhost:4566 sqs delete-message --queue-url "http://localhost:4566/000000000000/${QUEUE_NAME}" --receipt-handle "${receipt2}"
