<Query Kind="Program">
  <NuGetReference>AWSSDK.SQS</NuGetReference>
  <Namespace>Amazon</Namespace>
  <Namespace>Amazon.Runtime</Namespace>
  <Namespace>Amazon.SQS</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>Amazon.SQS.Model</Namespace>
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
    }).ConfigureAwait(false);

    var queueName = "my-queue";
    var dlqName = "my-deadletter-queue";
    var region = "ap-northeast-1";
    Func<int, string> messageFactory = i => $$"""
    {
        "Index": {{i}},
        "DateTime": {{DateTime.UtcNow}},
        "ID": {{Guid.NewGuid()}}
    }
    """;

    // for LocalStack
    var credentials = new Amazon.Runtime.BasicAWSCredentials("test", "test");
    var client = new Amazon.SQS.AmazonSQSClient(credentials, new AmazonSQSConfig
    {
        AuthenticationRegion = region,
        ServiceURL = "http://localhost:4566"
    });

    string? queueUrl = null;
    try
    {
        // create queue
        Console.WriteLine("Create queues");
        {
            var queues = await client.ListQueuesAsync(new ListQueuesRequest(), cts.Token);
            if (queues.QueueUrls.FirstOrDefault(x => string.Equals(x, queueName, StringComparison.OrdinalIgnoreCase)) is null)
            {
                await client.CreateQueueAsync(queueName, cts.Token);
            }
            if (queues.QueueUrls.FirstOrDefault(x => string.Equals(x, dlqName, StringComparison.OrdinalIgnoreCase)) is null)
            {
                await client.CreateQueueAsync(dlqName, cts.Token);
            }
        }

        // list queue name
        Console.WriteLine("List queues");
        {
            var queues = await client.ListQueuesAsync(new ListQueuesRequest(), cts.Token);
            queues.Dump();
            /*
            QueueUrls	List<String> (6 items)•••
                http://localhost:4566/000000000000/my-queue
                http://localhost:4566/000000000000/my-deadletter-queue
            */
        }

        // get queue url
        Console.WriteLine("Get queue");
        var queue = await client.GetQueueUrlAsync(queueName, cts.Token);
        queueUrl = queue.QueueUrl;
        queue.Dump();
        /*
            QueueUrl	http://localhost:4566/000000000000/my-queue
        */

        // set queue info
        Console.WriteLine("Set queue attributes");
        {
            await client.SetQueueAttributesAsync(queue.QueueUrl, new Dictionary<string, string>
            {
                { "DelaySeconds", "0"},
                { "MaximumMessageSize", "262144"}, // 256kb
                { "MessageRetentionPeriod", "1209600"}, // 14days
                { "ReceiveMessageWaitTimeSeconds", "0"},
                { "VisibilityTimeout", "300"},
            }, cts.Token);
        }

        // get queue info
        Console.WriteLine("Get queue attributes");
        {
            var attributes = await client.GetQueueAttributesAsync(queue.QueueUrl, new List<string> { "All" }, cts.Token);
            attributes.Dump();
        }

        // send message
        Console.WriteLine("Send messages");
        {
            Task.Run(async () =>
            {
                var i = 0;
                while (!cts.IsCancellationRequested)
                {
                    var items = Enumerable.Range(i, Random.Shared.Next(1, 10));
                    // single
                    await client.SendMessageAsync(queue.QueueUrl, messageFactory(Interlocked.Increment(ref i)), cts.Token);
                    
                    // batch (max 10 messages)
                    await client.SendMessageBatchAsync(queue.QueueUrl, items.Select(x => new SendMessageBatchRequestEntry
                    {
                        Id = Guid.NewGuid().ToString(),
                        MessageBody = messageFactory(Interlocked.Increment(ref i)),
                    }).ToList(), cts.Token);
                    
                    Console.WriteLine($"Sent {items.Count() + 1} messages");
                                        
                    await Task.Delay(TimeSpan.FromMicroseconds(Random.Shared.Next(1, 100)));
                }
            }, cts.Token).ConfigureAwait(false);
        }
        /*
        {
            "Index": 1,
            "DateTime": 2023/08/01 11:48:15,
            "ID": 54c7a3cc-4ca4-40f8-9ffd-0ed92e09b53c
        }
        */

        // execute message and delete it.
        Console.WriteLine("Read and handle messages");
        while (!cts.IsCancellationRequested)
        {
            // receive message
            var receive = await client.ReceiveMessageAsync(new ReceiveMessageRequest
            {
                QueueUrl = queue.QueueUrl,
                MaxNumberOfMessages = 10, // recieve 10 messages at once. max 10
            }, cts.Token);
            

            // no item check
            if (receive.Messages.Count == 0)
                continue;

            var begin = Stopwatch.GetTimestamp();
            foreach (var item in receive.Messages)
            {
                // do any execution...
                await Task.Delay(TimeSpan.FromMilliseconds(Random.Shared.Next(1, 5)));
            }

            // delete message
            Console.WriteLine($"Delete {receive.Messages.Count} messages. ({((double)(Stopwatch.GetTimestamp() - begin) / Stopwatch.Frequency * 1000).ToString("#.##")}ms)");
            await client.DeleteMessageBatchAsync(queue.QueueUrl, receive.Messages.Select(x => new Amazon.SQS.Model.DeleteMessageBatchRequestEntry
            {
                Id = x.MessageId,
                ReceiptHandle = x.ReceiptHandle,
            })
            .ToList(), cts.Token);
        }
    }
    catch (TaskCanceledException)
    {
        Console.WriteLine("Cancelled");
    }
    finally
    {
        // purge messages
        if (queueUrl is not null)
        {
            Console.WriteLine("Purge messages");
            await client.PurgeQueueAsync(queueUrl);
        }
    }
}
