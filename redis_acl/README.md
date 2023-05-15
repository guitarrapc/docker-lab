# Getting started

1. Launch Redis.
  ```sh
  docker compose up -d
  ```

2. Connect to redis

  ```csharp
  var redis = StackExchange.Redis.ConnectionMultiplexer.Connect("localhost:6379,allowAdmin=true,password=password123");
  redis.Dump();
  ```

# More detail

see: https://hub.docker.com/r/bitnami/redis/ for more detail.
