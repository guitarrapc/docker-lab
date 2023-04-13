terraform {
  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 4.0"
    }
  }
}

provider "aws" {
  region                      = "ap-northeast-1"
  access_key                  = "test"
  secret_key                  = "test"
  skip_credentials_validation = true
  skip_requesting_account_id  = true
  skip_metadata_api_check     = true
  # Starting with localstack version 0.11.0, all APIs are exposed via a single edge service,
  # which is accessible on http://localhost:4566 by default
  endpoints {
    sqs = "http://localhost:4566"
  }
}

resource "aws_sqs_queue" "test" {
  name          = "test-queue"
  delay_seconds = 0
  # max_message_size           = 256 * 1024        # max 262144kb
  # message_retention_seconds  = 60 * 60 * 24 * 14 # max 14 days
  receive_wait_time_seconds  = 0
  visibility_timeout_seconds = 30
}
