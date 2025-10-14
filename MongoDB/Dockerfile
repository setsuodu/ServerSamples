FROM ubuntu:22.04

MAINTAINER setsuodu@gmail.com

# Create an app directory
RUN mkdir -p /usr/src/LinuxServer
WORKDIR /usr/src/LinuxServer

COPY . /usr/src/LinuxServer

EXPOSE 3000

# Execute
ENTRYPOINT ["./Server.x86_64"]