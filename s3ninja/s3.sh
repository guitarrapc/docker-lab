#!/bin/bash
set -e

export ENDPOINT_URL=http://localhost:9000
export AWS_PROFILE=s3ninja
export BUCKET_NAME=foo-bucket

# 1) create bucket
echo "------------"
echo "Create S3 bucket"
aws --endpoint-url=${ENDPOINT_URL} s3 mb "s3://${BUCKET_NAME}"

# 2) list buckets
echo "------------"
echo "List S3 buckets"
aws --endpoint-url=${ENDPOINT_URL} s3 ls

# 3) put object
echo "------------"
echo "Put object to S3"
aws --endpoint-url=${ENDPOINT_URL} s3 cp ./foo.txt s3://${BUCKET_NAME}/foo.txt

# 4) list objects
echo "------------"
echo "List S3 objects"
aws --endpoint-url=${ENDPOINT_URL} s3 ls s3://${BUCKET_NAME}

# 5) get object
echo "------------"
echo "Get object from S3"
aws --endpoint-url=${ENDPOINT_URL} s3 cp s3://${BUCKET_NAME}/foo.txt /tmp/download.txt
cat /tmp/download.txt

# 6) delete object
echo "------------"
echo "Delete object from S3"
aws --endpoint-url=${ENDPOINT_URL} s3 rm s3://${BUCKET_NAME}/foo.txt

# 7) list objects
echo "------------"
echo "List S3 objects"
aws --endpoint-url=${ENDPOINT_URL} s3 ls s3://${BUCKET_NAME}
