#!/bin/bash
set -e

export AWS_PROFILE=localstack

# create queue
aws --endpoint-url=http://localhost:4566 sqs create-queue --queue-name foo-queue

# list queues
aws --endpoint-url=http://localhost:4566 sqs list-queues

# send messages
aws --endpoint-url=http://localhost:4566 sqs send-message --queue-url http://localhost:4566/000000000000/foo-queue --message-body "{""name"": ""dummy""}"
aws --endpoint-url=http://localhost:4566 sqs send-message --queue-url http://localhost:4566/000000000000/foo-queue --message-body "{""name"": ""dummy2""}"

# get message
recieve1=$(aws --endpoint-url=http://localhost:4566 sqs receive-message --queue-url http://localhost:4566/000000000000/foo-queue)
receipt1=$(echo $recieve1 | jq -r '.Messages[].ReceiptHandle')
body1=$(echo $recieve1 | jq -r '.Messages[].Body')
echo "${recieve1}"
echo "1st received body"
echo "${body1}"

recieve2=$(aws --endpoint-url=http://localhost:4566 sqs receive-message --queue-url http://localhost:4566/000000000000/foo-queue)
receipt2=$(echo $recieve2 | jq -r '.Messages[].ReceiptHandle')
body2=$(echo $recieve2 | jq -r '.Messages[].Body')
echo "${recieve2}"
echo "2nd received body"
echo "${body2}"

# no message as of visible timeout
aws --endpoint-url=http://localhost:4566 sqs receive-message --queue-url http://localhost:4566/000000000000/foo-queue

# delete message by receipt-handle
aws --endpoint-url=http://localhost:4566 sqs delete-message --queue-url http://localhost:4566/000000000000/foo-queue --receipt-handle "${receipt1}"
aws --endpoint-url=http://localhost:4566 sqs delete-message --queue-url http://localhost:4566/000000000000/foo-queue --receipt-handle "${receipt2}"
