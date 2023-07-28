# Getting started

```
$ cd localstack

# launch localstack
$ docker compose up -d

# create queue via terraform
$ terraform init
$ terraform apply

# operate sqs via aws cli
$ ./sqs.sh
```

# AWS Credentials

.aws/config

```
[profile localstack]
region = ap-northeast-1
```

.aws/credentials

```
[localstack]
aws_access_key_id = test
aws_secret_access_key = test
```
