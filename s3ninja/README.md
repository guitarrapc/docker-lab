# Getting started

```
$ cd s3ninja

# launch s3ninja
$ docker compose up -d

# operate s3 via aws cli
$ ./s3.sh
```

# AWS Credentials

.aws/config

```
[profile s3ninja]
region = ap-northeast-1
```

.aws/credentials

```
[s3ninja]
aws_access_key_id = AKIAIOSFODNN7EXAMPLE
aws_secret_access_key = wJalrXUtnFEMI/K7MDENG/bPxRfiCYEXAMPLEKEY
addressing_style = path
```
