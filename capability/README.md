## README

```sh
docker compose up -d
```

```sh
# Ubuntu 22.04
$ docker compose exec ubuntu2204 /bin/bash
$ getpcaps $(pgrep bash)
1: cap_chown,cap_dac_override,cap_fowner,cap_fsetid,cap_kill,cap_setgid,cap_setuid,cap_setpcap,cap_net_bind_service,cap_net_raw,cap_sys_chroot,cap_mknod,cap_audit_write,cap_setfcap=ep
9: cap_chown,cap_dac_override,cap_fowner,cap_fsetid,cap_kill,cap_setgid,cap_setuid,cap_setpcap,cap_net_bind_service,cap_net_raw,cap_sys_chroot,cap_mknod,cap_audit_write,cap_setfcap=ep
$ chmod 777 /var/log/apt/
```

```sh
# Ubuntu 22.04
$ docker compose exec ubuntu2204_cap /bin/bash
$ getpcaps $(pgrep bash)
1: cap_chown=ep
12: cap_chown=ep
$ chmod 777 /var/log/apt/
```

```sh
# Ubuntu 22.04
$ docker compose exec ubuntu2204_cap_user /bin/bash
$ getpcaps $(pgrep bash)
1: =
19: =
$ capsh --decode=$(grep CapBnd /proc/1/status|cut -f2)
0x00000000000000c9=cap_chown,cap_fowner,cap_setgid,cap_setuid
$ capsh --decode=$(grep CapBnd /proc/19/status|cut -f2)
0x00000000000000c9=cap_chown,cap_fowner,cap_setgid,cap_setuid
$ whoami
foo
$ sudo chmod 777 /var/log/apt/
sudo: unable to change to root gid: Operation not permitted  # need capability "cap_setgid,cap_setuid"
sudo: error initializing audit plugin sudoers_audit          # need capability "cap_audit_write"
```
