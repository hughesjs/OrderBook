FROM mongo:latest

COPY . . 

RUN apt-get update && apt-get install wget
RUN wget https://dl.k6.io/deb/dists/stable/main/binary-amd64/k6-v0.40.0-amd64.deb
RUN apt-get install -y ./k6-v0.40.0-amd64.deb

WORKDIR perf-testing
RUN chmod +x ./entrypoint.sh

ARG MONGO_PASS
ENV MONGO_PASS=$MONGO_PASS

ARG MONGO_USER
ENV MONGO_USER=$MONGO_USER

ARG MONGO_HOST
ENV MONGO_HOST=$MONGO_HOST

ARG MONGO_PORT
ENV MONGO_PORT=$MONGO_PORT

ARG TEST_FILE
ENV TEST_FILE=$TEST_FILE

ENTRYPOINT ./entrypoint.sh
