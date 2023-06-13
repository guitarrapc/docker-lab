# Getting started

1. Launch Redis-Cluster
  ```sh
  docker compose up -d
  ```

2. Open new shell and SSubscribe.

  ```sh
  docker compose exec redis /bin/bash
  redis-cli -h redis-node-5 -a bitnami -c
  > ssubscribe sfoo
  ```

3. Open new shell and SPublish.

  ```sh
  docker compose exec redis /bin/bash
  redis-cli -h redis-node-5 -a bitnami -c
  > spublish sfoo 1
  ```

4. Repeat 2 and 3 for SSubscribe and SPublish.

  ```sh
  > ssubscribe sbar
  > spublish sbar 1
  ```

5. Stop Redis-Cluster
  ```sh
  docker compose down
  ```

# More detail

see: https://hub.docker.com/r/bitnami/redis-cluster/ for more detail.
