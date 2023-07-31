<Query Kind="Program">
  <NuGetReference>AWSSDK.IdentityManagement</NuGetReference>
  <NuGetReference>AWSSDK.KinesisFirehose</NuGetReference>
  <NuGetReference>AWSSDK.S3</NuGetReference>
  <Namespace>Amazon.KinesisFirehose</Namespace>
  <Namespace>Amazon.KinesisFirehose.Model</Namespace>
  <Namespace>Amazon.S3</Namespace>
  <Namespace>Amazon.S3.Model</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>Amazon.IdentityManagement</Namespace>
  <Namespace>Amazon.IdentityManagement.Model</Namespace>
</Query>

async Task Main()
{
    var cts = new CancellationTokenSource(TimeSpan.FromMinutes(5));
    Task.Run(() =>
    {
        Console.ReadLine();
        cts.Cancel();
        cts.Dispose();
    });

    var iamRoleName = "firehose-role";
    var bucketName = "my-bucket";
    var deliveryStreamName = "my-delivery";
    var region = "ap-northeast-1";

    // for LocalStack
    var credentials = new Amazon.Runtime.BasicAWSCredentials("test", "test");
    var iamClient = new Amazon.IdentityManagement.AmazonIdentityManagementServiceClient(credentials, new AmazonIdentityManagementServiceConfig
    {
        ServiceURL = "http://localhost:4566",
    });
    var s3Client = new Amazon.S3.AmazonS3Client(credentials, new AmazonS3Config
    {
        AuthenticationRegion = region,
        ServiceURL = "http://localhost:4566",
        ForcePathStyle = true,
    });
    var firehoseClient = new Amazon.KinesisFirehose.AmazonKinesisFirehoseClient(credentials, new AmazonKinesisFirehoseConfig
    {
        AuthenticationRegion = region,
        ServiceURL = "http://localhost:4566"
    });

    try
    {
        // create iam role
        Console.WriteLine("Create Firehose Role");
        var iamRole = await iamClient.CreateRoleAsync(new CreateRoleRequest
        {
            RoleName = iamRoleName,
            AssumeRolePolicyDocument = """
            {
                "Version": "2012-10-17",
                "Statement": [
                    {
                        "Sid": "",
                        "Effect": "Allow",
                        "Principal": {
                            "Service": "firehose.amazonaws.com"
                        },
                        "Action": "sts:AssumeRole"
                    }
                ]
            }
            """,
        }, cts.Token);
        var iamPolicy = await iamClient.CreatePolicyAsync(new CreatePolicyRequest
        {
            PolicyName = "firehose-execution-policy",
            PolicyDocument = $$"""
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
                            "arn:aws:s3:::{{bucketName}}/*",
                            "arn:aws:s3:::{{bucketName}}"
                        ],
                        "Sid": "S3Access"
                    }
                ],
                "Version": "2012-10-17"
            }
            """,
        }, cts.Token);
        await iamClient.AttachRolePolicyAsync(new AttachRolePolicyRequest
        {
            RoleName = iamRoleName,
            PolicyArn = iamPolicy.Policy.Arn,
        }, cts.Token);
        iamRole.Dump();
        // arn:aws:iam::000000000000:role/firehose-role
        
        // create bucket
        Console.WriteLine("Create Bucket");
        var s3 = await s3Client.PutBucketAsync(new PutBucketRequest
        {
            BucketName = bucketName,
            BucketRegion = region, // required for localstack. see: https://docs.aws.amazon.com/cli/latest/reference/s3api/create-bucket.html
        }, cts.Token);
        // arn:aws:s3:::my-bucket

        // create firehose delivery
        Console.WriteLine("Create Firehose Delivery");
        var firehose = await firehoseClient.CreateDeliveryStreamAsync(new CreateDeliveryStreamRequest
        {
            DeliveryStreamName = deliveryStreamName,
            DeliveryStreamType = DeliveryStreamType.DirectPut,
            ExtendedS3DestinationConfiguration = new ExtendedS3DestinationConfiguration
            {
                BucketARN = $"arn:aws:s3:::{bucketName}",
                BufferingHints = new BufferingHints
                {
                    IntervalInSeconds = 300, // 300s is recommended
                    SizeInMBs = 5, // 5MB is recommended.
                },
                CompressionFormat = Amazon.KinesisFirehose.CompressionFormat.GZIP,
                RoleARN = iamRole.Role.Arn,
            }
        }, cts.Token);
        firehose.Dump();
        // arn:aws:firehose:ap-northeast-1:000000000000:deliverystream/my-delivery

        // list bucket
        Console.WriteLine("List Buckets");
        var buckets = await s3Client.ListBucketsAsync(cts.Token);
        buckets.Dump();
        /*
        BucketName	CreationDate
        my-bucket	2023/07/31 20:14:22
        */

        // list firehose streams
        Console.WriteLine("List DeliveryStreams");
        var deliverystreams = await firehoseClient.ListDeliveryStreamsAsync(cts.Token);
        deliverystreams.Dump();
        /*
        DeliveryStreamNames	my-delivery
        */

        // send message
    }
    catch (TaskCanceledException)
    {
        Console.WriteLine("cancelled");
    }

}
