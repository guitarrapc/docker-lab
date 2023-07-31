#!/bin/bash
set -eo pipefail

# NOTE: use awslocal to access localstack service.

region=ap-northeast-1
queues=("hello-queue" "hello-deadletter-queue")

# create queues
for i in "${!queues[@]}"; do
  awslocal sqs create-queue --queue-name "${queues[i]}" --region "${region}"
done

# configure Queue
for i in "${!queues[@]}"; do
  queue_url="http://localhost:4566/000000000000/${queues[i]}"
  delaySeconds=0
  maximumMessageSize=262144 # 256kb
  messageRetentionPeriod=1209600 # 14days
  receiveMessageWaitTimeSeconds=0
  visibilityTimeout=300
  maxReceiveCount=10

  # Normal Queue
  (( i % 2 )) || awslocal sqs set-queue-attributes --queue-url "${queue_url}" --attributes VisibilityTimeout=${visibilityTimeout} --attributes ReceiveMessageWaitTimeSeconds=${receiveMessageWaitTimeSeconds} --attributes MessageRetentionPeriod=${messageRetentionPeriod} --attributes DelaySeconds=${delaySeconds} --attributes MaximumMessageSize=${maximumMessageSize} --attributes "RedrivePolicy='{\"deadLetterTargetArn\":\"arn:aws:sqs:${region}:000000000000:${queues[i+1]}\",\"maxReceiveCount\":\"${maxReceiveCount}\"}'" --region "${region}"

  # DeadLetter Queue
  (( i % 2 )) && awslocal sqs set-queue-attributes --queue-url "${queue_url}" --attributes VisibilityTimeout=${visibilityTimeout} --attributes ReceiveMessageWaitTimeSeconds=${receiveMessageWaitTimeSeconds} --attributes MessageRetentionPeriod=${messageRetentionPeriod} --attributes DelaySeconds=${delaySeconds} --attributes MaximumMessageSize=${maximumMessageSize} --attributes "RedriveAllowPolicy='{\"redrivePermission\":\"byQueue\",\"sourceQueueArns\":[\"arn:aws:sqs:${region}:000000000000:${queues[i-1]}\"]}'" --region "${region}"
done

# show queue detail
for i in "${!queues[@]}"; do
  queue_url="http://localhost:4566/000000000000/${queues[i]}"
  awslocal sqs get-queue-attributes --queue-url "${queue_url}" --attribute-names All --region "${region}"
done

# show queue list
awslocal sqs list-queues --region "${region}"
