#!/bin/bash
set -eo pipefail

# NOTE: use awslocal to access localstack service.

region=ap-northeast-1
iamRoleName="firehose-accesslog-delivery-role"
bucketName="accesslog-bucket"
deliveryName="accesslog-delivery"

# create iamrole
awslocal iam create-role --role-name "${iamRoleName}" --assume-role-policy-document \
'{
    "Version": "2012-10-17",
    "Statement": [
        {
            "Effect": "Allow",
            "Principal": {
                "Service": "firehose.amazonaws.com"
            },
            "Action": "sts:AssumeRole"
        }
    ]
}'
policy=$(cat << EOF
{
    "Statement": [
        {
            "Action": [
                "s3:PutObject",
                "s3:ListBucketMultipartUploads",
                "s3:ListBucket",
                "s3:GetObject",
                "s3:GetBucketLocation",
                "s3:AbortMultipartUpload"
            ],
            "Effect": "Allow",
            "Resource": [
                "arn:aws:s3:::${bucketName}/*",
                "arn:aws:s3:::${bucketName}"
            ],
            "Sid": "S3Access"
      }
  ],
  "Version": "2012-10-17"
}
EOF
)
awslocal iam put-role-policy --role-name "${iamRoleName}" --policy-name "firehose-s3-delivery-policy" --policy-document "$policy"

# create s3 bucket
awslocal s3 mb s3://${bucketName} --region "${region}"

# create firehose delivery stream
firehose_json=$(cat << EOF
{
    "DeliveryStreamName": "$deliveryName",
    "S3DestinationConfiguration": {
        "BucketARN": "arn:aws:s3:::${bucketName}",
        "BufferingHints": {
            "SizeInMBs": 5,
            "IntervalInSeconds": 300
        },
        "CompressionFormat": "GZIP",
        "Prefix": "${deliveryName}",
        "RoleARN": "arn:aws:iam::000000000000:role/${iamRoleName}"
    }
}
EOF
)
awslocal firehose create-delivery-stream --cli-input-json "${firehose_json}" --region ${region}

# show firehose detail
awslocal firehose describe-delivery-stream --delivery-stream-name "${deliveryName}" --region "${region}"

# show queue list
awslocal firehose list-delivery-streams --region "${region}"
