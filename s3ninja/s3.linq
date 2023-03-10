<Query Kind="Program">
  <NuGetReference>AWSSDK.S3</NuGetReference>
  <Namespace>Amazon</Namespace>
  <Namespace>Amazon.Runtime</Namespace>
  <Namespace>Amazon.S3</Namespace>
  <Namespace>Amazon.S3.Model</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>System.Text.Json</Namespace>
  <Namespace>System.Buffers</Namespace>
</Query>

async Task Main()
{
    var bucketName = "my-bucket";

    // For S3ninja
    var credentials = new Amazon.Runtime.BasicAWSCredentials("AKIAIOSFODNN7EXAMPLE", "wJalrXUtnFEMI/K7MDENG/bPxRfiCYEXAMPLEKEY");
    var client = new Amazon.S3.AmazonS3Client(credentials, new AmazonS3Config
    {
        AuthenticationRegion = "ap-northeast-1",
        ServiceURL = "http://localhost:9000/",
        ForcePathStyle = true, // must
    });

    // create new bucket
    await client.PutBucketAsync(bucketName);

    // list buckets
    var buckets = await client.ListBucketsAsync();
    buckets.Dump();

    // put new objects to bucket
    await client.PutObjectAsync(new PutObjectRequest
    {
       BucketName = bucketName,
        Key = "foo",
        ContentBody = JsonSerializer.Serialize(new Dictionary<string, string>()
        {
            {"name", "foo"},
        }),
        ContentType = "application/json",
    });

    // list objects
    var objects = await client.ListObjectsAsync(new ListObjectsRequest
    {
       BucketName = bucketName,
    });
    objects.Dump();

    // list objects v2
    var objectsv2 = await client.ListObjectsV2Async(new ListObjectsV2Request
    {
       BucketName = bucketName,
    });
    objectsv2.Dump();

    // get objects
    var item = await client.GetObjectAsync(bucketName, "foo");
    await GetJsonObjectContentAsync<Dictionary<string, string>>(item).Dump();
}

private async Task<T?> GetJsonObjectContentAsync<T>(GetObjectResponse response)
{
    var itemSize = response.ContentLength > 81920 ? 81920 : (int)response.ContentLength;
    var rentBuffer = ArrayPool<byte>.Shared.Rent(itemSize); // default chunksize
    try
    {
        await response.ResponseStream.ReadAsync(rentBuffer, 0, itemSize);
    }
    finally
    {
        ArrayPool<byte>.Shared.Return(rentBuffer);
    }
    if (rentBuffer.Length > response.ContentLength)
    {
        return JsonSerializer.Deserialize<T>(rentBuffer.AsSpan().Slice(0, (int)response.ContentLength));
    }
    else
    {
        return JsonSerializer.Deserialize<T>(rentBuffer.AsSpan());
    }
}
