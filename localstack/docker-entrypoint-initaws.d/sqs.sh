#!/bin/bash
set -e
awslocal sqs create-queue --queue-name "auto-queue"
