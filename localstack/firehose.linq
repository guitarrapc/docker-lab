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
  <Namespace>System.Text.Json</Namespace>
</Query>

async Task Main()
{
    var timeout = TimeSpan.FromMinutes(5);
    var cts = new CancellationTokenSource(timeout);
    Console.WriteLine($"Begin: {DateTime.Now}, Timeup: {DateTime.Now.Add(timeout)}");
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
    var verbose = false; // output more dumps.
    string CreateMessage(int index) => new Message(index).ToJsonLine();

    // for LocalStack
    var credentials = new Amazon.Runtime.BasicAWSCredentials("test", "test");
    using var iamClient = new Amazon.IdentityManagement.AmazonIdentityManagementServiceClient(credentials, new AmazonIdentityManagementServiceConfig
    {
        ServiceURL = "http://localhost:4566",
    });
    using var s3Client = new Amazon.S3.AmazonS3Client(credentials, new AmazonS3Config
    {
        AuthenticationRegion = region,
        ServiceURL = "http://localhost:4566",
        ForcePathStyle = true,
    });
    using var firehoseClient = new Amazon.KinesisFirehose.AmazonKinesisFirehoseClient(credentials, new AmazonKinesisFirehoseConfig
    {
        AuthenticationRegion = region,
        ServiceURL = "http://localhost:4566"
    });

    try
    {
        // create iam role
        Console.WriteLine("Create IamRole");
        var roles = await iamClient.ListRolesAsync(cts.Token);
        var iamRoleArn = roles.Roles.FirstOrDefault(x => string.Equals(x.RoleName, iamRoleName, StringComparison.OrdinalIgnoreCase))?.Arn;
        if (iamRoleArn is null)
        {
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
            await iamClient.PutRolePolicyAsync(new PutRolePolicyRequest
            {
                RoleName = iamRoleName,
                PolicyName = "firehose-s3-delivery-policy",
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
        }

        // create bucket
        Console.WriteLine("Create Bucket");
        var s3buckets = await s3Client.ListBucketsAsync(cts.Token);
        var bucket = s3buckets.Buckets.FirstOrDefault(x => string.Equals(x.BucketName, bucketName, StringComparison.OrdinalIgnoreCase));
        if (bucket is null)
        {
            var s3 = await s3Client.PutBucketAsync(new PutBucketRequest
            {
                BucketName = bucketName,
                BucketRegion = region, // required for localstack. see: https://docs.aws.amazon.com/cli/latest/reference/s3api/create-bucket.html
            }, cts.Token);
        }

        // create firehose delivery
        Console.WriteLine("Create Firehose Delivery");
        var deliveryStreams = await firehoseClient.ListDeliveryStreamsAsync(cts.Token);
        var deliveryStream = deliveryStreams.DeliveryStreamNames.FirstOrDefault(x => string.Equals(x, deliveryStreamName, StringComparison.OrdinalIgnoreCase));
        if (deliveryStream is null)
        {
            var firehose = await firehoseClient.CreateDeliveryStreamAsync(new CreateDeliveryStreamRequest
            {
                DeliveryStreamName = deliveryStreamName,
                DeliveryStreamType = DeliveryStreamType.DirectPut,
                ExtendedS3DestinationConfiguration = new ExtendedS3DestinationConfiguration
                {
                    BucketARN = $"arn:aws:s3:::{bucketName}",
                    // Currently Bufferhinsts not respected. see: https://github.com/localstack/localstack/issues/7024
                    BufferingHints = new BufferingHints
                    {
                        IntervalInSeconds = 300, // default 300s
                        SizeInMBs = 5, // default 5MB
                    },
                    CompressionFormat = Amazon.KinesisFirehose.CompressionFormat.GZIP,
                    RoleARN = iamRoleArn,
                },
            }, cts.Token);
        }
        // arn:aws:firehose:ap-northeast-1:000000000000:deliverystream/my-delivery

        // list iam role
        Console.WriteLine("List IamRole");
        var iamRoles = await iamClient.ListRolesAsync(cts.Token);
        iamRoles.Dump();
        /*
        BucketName	CreationDate
        my-bucket	2023/07/31 20:14:22
        */

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

        // describe firehose stream
        Console.WriteLine("Describe DeliveryStreams");
        var deliverystream = await firehoseClient.DescribeDeliveryStreamAsync(new DescribeDeliveryStreamRequest
        {
            DeliveryStreamName = deliveryStreamName,
        }, cts.Token);
        deliverystream.Dump();
        /*
        {
            "DeliveryStreamDescription": {
                "DeliveryStreamName": "my-delivery",
                "DeliveryStreamARN": "arn:aws:firehose:ap-northeast-1:000000000000:deliverystream/my-delivery",
                "DeliveryStreamStatus": "ACTIVE",
                "DeliveryStreamType": "DirectPut",
                "VersionId": "1",
                "CreateTimestamp": 1691372439.783361,
                "Destinations": [
                    {
                        "DestinationId": "e913a48a",
                        "ExtendedS3DestinationDescription": {
                            "BucketARN": "arn:aws:s3:::my-bucket",
                            "BufferingHints": {
                                "SizeInMBs": 5,
                                "IntervalInSeconds": 300
                            },
                            "CompressionFormat": "GZIP"
                        }
                    }
                ],
                "HasMoreDestinations": false
            }
        }
        */

        // send message
        Console.WriteLine("Send messages");
        {
            Task.Run(async () =>
            {
                var i = 0;
                var records = new List<Record>();
                while (!cts.IsCancellationRequested)
                {
                    var items = Enumerable.Range(i, Random.Shared.Next(10, 200));

                    // single
                    var message = CreateMessage(Interlocked.Increment(ref i));
                    using (var data = new MemoryStream(Encoding.UTF8.GetBytes(message)))
                    {
                        await firehoseClient.PutRecordAsync(new PutRecordRequest
                        {
                            DeliveryStreamName = deliveryStreamName,
                            Record = new Record { Data = data },
                        }, cts.Token);
                    }
                    // batch
                    foreach (var item in items)
                    {
                        var m = CreateMessage(Interlocked.Increment(ref i));
                        using (var data = new MemoryStream(Encoding.UTF8.GetBytes(m)))
                        {
                            records.Add(new Record { Data = data });
                        }
                    }
                    await firehoseClient.PutRecordBatchAsync(new PutRecordBatchRequest
                    {
                        DeliveryStreamName = deliveryStreamName,
                        Records = records,
                    }, cts.Token);
                    records.Clear();

                    Console.WriteLine($"Sent {items.Count() + 1} messages");
                }
            }, cts.Token);
        }
        /*
        {
            "Index": 139,
            "DateTime": 2023/08/01 11:38:24,
            "ID": bef05fc8-9e4d-44cc-9ea7-e5d8ff8e325c
        }
        */

        Console.WriteLine("Read messages");
        var lastReadIndex = 0;
        var readLoopCount = 0;
        var first = true;
        while (!cts.IsCancellationRequested)
        {
            // read message
            // Firehose deliver to s3 on this datetime
            var utc = DateTime.UtcNow;
            var year = utc.Year;
            var month = utc.Month.ToString("D2");
            var day = utc.Day.ToString("D2");
            var hour = utc.Hour.ToString("D2");
            var keyPrefix = $"{year}/{month}/{day}/{hour}"; // default Firehose key is `{year}/{month}/{day}/{hour}/{deliveryName}-{deliveryStreamVersion}-{year}-{month}-{day}-{hour}-{minute}-{GUID}` see: https://docs.aws.amazon.com/firehose/latest/dev/basic-deliver.html#s3-object-name
            var s3Objects = await s3Client.ListObjectsV2Async(new ListObjectsV2Request
            {
                BucketName = bucketName,
                Prefix = keyPrefix,
            }, cts.Token);

            if (first)
            {
                first = false;
                lastReadIndex = s3Objects.KeyCount;
            }

            var insertedObjects = s3Objects.S3Objects.GetRange(lastReadIndex, s3Objects.KeyCount - lastReadIndex);

            // no item check
            if (insertedObjects.Count == 0)
                continue;

            lastReadIndex = s3Objects.KeyCount;
            readLoopCount++;

            long size = 0;
            foreach (var s3Object in insertedObjects)
            {
                // do any execution...
                size += s3Object.Size;
            }
            if (verbose)
            {
                // show contents
                await ReadS3ObjectAsync(s3Client, insertedObjects, cts.Token);
            }

            Console.WriteLine($"Delivered S3Objects:{insertedObjects.Count}, TotalBytes: {size}, LoopCount: {readLoopCount}");

            await Task.Delay(TimeSpan.FromMilliseconds(10 * 1000), cts.Token);
        }
    }
    catch (TaskCanceledException)
    {
        Console.WriteLine("cancelled");
    }
}

public async Task ReadS3ObjectAsync(AmazonS3Client client, IReadOnlyList<S3Object> s3Objects, CancellationToken ct)
{
    // too heavy to do serial.
    await Parallel.ForEachAsync(s3Objects, new ParallelOptions { MaxDegreeOfParallelism = 6, CancellationToken = ct }, async (source, cancellationToken) =>
    {
        var s3Object = await client.GetObjectAsync(new GetObjectRequest
        {
            BucketName = source.BucketName,
            Key = source.Key,
        }, ct);

        using (var reader = new StreamReader(s3Object.ResponseStream))
        {
            var content = await reader.ReadToEndAsync();
            content.Dump(source.Key);
        }
    });
}

public class Message
{
    public int Index { get; init; }
    public string DateTime { get; init; } = System.DateTime.UtcNow.ToString();
    public string Id { get; init; } = Guid.NewGuid().ToString();

    public Message(int index) => Index = index;

    public string ToJsonLine()
    {
        return JsonSerializer.Serialize(this) + "\n";
    }
}