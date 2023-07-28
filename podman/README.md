# Podman

## Install on Windows

Install Podman.

```sh
$ scoop install podman-desktop
$ podman machine init
$ podman machine start
$ podman machine list
NAME                     VM TYPE     CREATED             LAST UP            CPUS        MEMORY      DISK SIZE
podman-machine-default*  wsl         About a minute ago  Currently running  2           566.5MB     717.2MB
```

Install podman-compose.

```sh
$ scoop install podman-compose
```

Run podman.

```sh
$ podman run ubi8-micro date
Resolved "ubi8-micro" as an alias (/etc/containers/registries.conf.d/000-shortnames.conf)
Trying to pull registry.access.redhat.com/ubi8-micro:latest...
Getting image source signatures
Checking if image destination supports signatures
Copying blob sha256:17aa49b9a870e7787370b7918f8c7c43d643b86c51e398e7eab686b147516514
Copying config sha256:62c33d0617244531862c17cf9607961b55c4965a1a0ea037c1d9c357d00ef6d6
Writing manifest to image destination
Storing signatures
Tue Jun 10 01:52:48 UTC 2023

$ podman run --rm -d -p 8080:80 --name httpd docker.io/library/httpd
Trying to pull docker.io/library/httpd:latest...
Getting image source signatures
Copying blob sha256:49d8a68fd903c4c5786f34b8510d9cc221f6229ba9921f526afa7fbeb9cf91c5
Copying blob sha256:f03b40093957615593f2ed142961afb6b540507e0b47e3f7626ba5e02efbbbf1
Copying blob sha256:abaf8619eb1c96b5bda436caa32f832076cd92d6277e0fa8a719fbfb6de8a50e
Copying blob sha256:e3fe37d0c2ad420bd2a30bb0ac9f84b92eada7c790a914db00a4d9c76774008f
Copying blob sha256:52a1e37affe549886bc5f97b08a3fe1b19d41da3d3e3284ebda4823b7058d194
Copying config sha256:d1676199e60591c70e38ddfde2c6c3fc51452fafeeeab485f6713d715dacee3a
Writing manifest to image destination
Storing signatures
cf21cb08cba451ed37caf8a60b2ed9519021e56c103b165536fa83391e3f2401

$ podman ps
CONTAINER ID  IMAGE                           COMMAND           CREATED         STATUS         PORTS                 NAMES
cf21cb08cba4  docker.io/library/httpd:latest  httpd-foreground  45 seconds ago  Up 46 seconds  0.0.0.0:8080->80/tcp  httpd

$ curl http://localhost:8080/ -UseBasicParsing
Content : <html><body><h1>It works!</h1></body></html>

$ podman stop httpd
```

Run podman compose

```sh
$ podman-compose up -d
$ podman-compose ps
podman-compose version: 1.0.6
['podman', '--version', '']
using podman version: 4.5.1
podman ps -a --filter label=io.podman.compose.project=podman
CONTAINER ID  IMAGE                           COMMAND           CREATED        STATUS        PORTS                 NAMES
84464232c0a8  docker.io/library/httpd:latest  httpd-foreground  8 seconds ago  Up 8 seconds  0.0.0.0:8080->80/tcp  podman_httpd_1
exit code: 0
$ curl http://localhost:8080/ -UseBasicParsing

<html><body><h1>It works!</h1></body></html>
$ podman-compose down
```
