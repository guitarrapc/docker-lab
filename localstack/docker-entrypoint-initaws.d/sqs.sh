#!/bin/bash
set -e

# use awslocal to access localstack service.
awslocal sqs create-queue --queue-name "auto-queue"
