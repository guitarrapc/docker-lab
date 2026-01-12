FROM ubuntu:24.04 AS builder

ENV JMETER_VERSION="5.6.3"
ENV JMETER_HOME=/opt/apache-jmeter-${JMETER_VERSION}
ENV JMETER_PLUGINS_MANAGER_VERSION="1.10"
# SHA512 checksums for integrity verification
ENV JMETER_SHA512="5978a1a35edb5a7d428e270564ff49d2b1b257a65e17a759d259a9283fc17093e522fe46f474a043864aea6910683486340706d745fcdf3db1505fd71e689083"
ENV JMETER_PLUGINS_MANAGER_SHA512="38af806a7c78473c032ba93c7a2e522674871f01616985a0b0522483977d58afdc444d18bd5590b8036c344ccf11d2fe61be807501d5edb6d4bdebc9050c43ae"

RUN apt-get update \
  && apt-get install -y --no-install-recommends wget ca-certificates \
  # Download JMeter
  && wget https://archive.apache.org/dist/jmeter/binaries/apache-jmeter-${JMETER_VERSION}.tgz -O /tmp/jmeter.tgz \
  # Verify JMeter checksum
  && echo "${JMETER_SHA512}  /tmp/jmeter.tgz" | sha512sum -c - \
  && tar -xzf /tmp/jmeter.tgz -C /opt \
  # Download JMeter Plugins Manager
  && wget https://repo1.maven.org/maven2/kg/apc/jmeter-plugins-manager/${JMETER_PLUGINS_MANAGER_VERSION}/jmeter-plugins-manager-${JMETER_PLUGINS_MANAGER_VERSION}.jar -O ${JMETER_HOME}/lib/ext/jmeter-plugins-manager.jar \
  # Verify Plugins Manager checksum
  && echo "${JMETER_PLUGINS_MANAGER_SHA512}  ${JMETER_HOME}/lib/ext/jmeter-plugins-manager.jar" | sha512sum -c - \
  && rm -rf ${JMETER_HOME}/docs ${JMETER_HOME}/printable_docs /tmp/jmeter.tgz

FROM ubuntu:24.04

LABEL version="5.6.3"
LABEL description="An Ubuntu based docker image contains Apache JMeter GUI to configure scenario.\
  Enable connect container with VNC and RDP."
LABEL maintainer="3856350+guitarrapc@users.noreply.github.com"

ENV DEBIAN_FRONTEND=noninteractive
ENV JMETER_VERSION="5.6.3"
ENV JMETER_HOME=/opt/apache-jmeter-${JMETER_VERSION}
ENV JMETER_BIN=${JMETER_HOME}/bin
ENV PATH=${JMETER_BIN}:$PATH
ENV DISPLAY=":99" \
  RESOLUTION="1366x768x24" \
  PASS="root"

# Install minimal packages
RUN apt-get update \
  && apt-get install -y --no-install-recommends \
  wget ca-certificates \
  xvfb x11vnc \
  xrdp xorgxrdp \
  fluxbox xterm \
  openjdk-11-jre \
  && apt-get clean \
  && rm -rf /var/lib/apt/lists/*

COPY --from=builder /opt/apache-jmeter-${JMETER_VERSION} /opt/apache-jmeter-${JMETER_VERSION}

# Configure VNC
RUN x11vnc -storepasswd ${PASS} /etc/x11vnc.pass

# Set root password for RDP login
RUN echo "root:${PASS}" | chpasswd

# Configure RDP
COPY xrdp.ini /etc/xrdp/xrdp.ini

EXPOSE 5900 3389

WORKDIR /root

CMD ["bash", "-c", "rm -f /tmp/.X99-lock /var/run/xrdp.pid /var/run/xrdp-sesman.pid \
  && /usr/bin/Xvfb ${DISPLAY} -screen 0 ${RESOLUTION} -ac +extension GLX +render -noreset & \
  sleep 3 \
  && fluxbox & \
  sleep 2 \
  && jmeter -Jjmeter.laf=CrossPlatform & \
  sleep 3 \
  && x11vnc -xkb -noxrecord -noxfixes -noxdamage -display ${DISPLAY} -forever -rfbport 5900 -rfbauth /etc/x11vnc.pass -shared & \
  sleep 2 \
  && xrdp-sesman & \
  sleep 1 \
  && xrdp --nodaemon"]
