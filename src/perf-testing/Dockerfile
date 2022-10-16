FROM mongo:latest

COPY . . 

RUN apt-get update && apt-get install wget
RUN wget https://dl.k6.io/deb/dists/stable/main/binary-amd64/k6-v0.40.0-amd64.deb
RUN apt-get install -y ./k6-v0.40.0-amd64.deb

WORKDIR perf-testing
RUN chmod +x ./entrypoint.sh

ENTRYPOINT ./entrypoint.sh
