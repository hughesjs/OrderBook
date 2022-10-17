# OrderBook
High-performance Order Book Service for Trading

----
[![GitHub Workflow Status](https://img.shields.io/github/workflow/status/hughesjs/OrderBook/.NET%20Continuous%20Integration?label=BUILD%20CI&style=for-the-badge)](https://github.com/hughesjs/dotnet-6-ci-cd-template/actions)
![GitHub top language](https://img.shields.io/github/languages/top/hughesjs/OrderBook?style=for-the-badge)
[![GitHub](https://img.shields.io/github/license/hughesjs/OrderBook?style=for-the-badge)](LICENSE)
![FTB](https://raw.githubusercontent.com/hughesjs/custom-badges/master/made-in/made-in-scotland.svg)

# Introduction

The purpose of this service is to keep track of pending orders in a trading system and calculate the expected price of any potential new trade by working out the volume-weighted average price.

This service has been written in C#11 with .NET 7 and gRPC for communications. It uses MongoDB as a persistent datastore and Redis for caching.

If you don't have these installed, there are docker-compose files you can use to spin the service and dependencies up locally.

They all follow the naming convention `docker-compose-X.yaml`.

| X | Function |
| ---- | ---- |
| dev | All dependencies are created with exposed ports on localhost. This is used for the API tests. | 
| perf | Runs the performance tests, you must provide it with certain `env` vars. Of particular importance is `TEST_FILE` which determines which test it runs.
| prod | This emulates a production deployment. Dependencies are deployed on a hidden internal network and only the RPC service is exposed on localhost

I did create a postman collection for this, however, you can't export anything with gRPC endpoints in it for some reason.

Reflection is enabled on the service as long as `$ENVIRONMENT=Development`, so it should be pretty easy to get going with if you want to try it.

# Design Assumptions

I've designed this solution under the following (non-exhaustive) list of assumptions:

1. Reads are far more common that writes.
1. The service is internal, not exposed directly to the internet.
1. The most important hot-path is fetching the price of orders.


# Architectural Summary

The architecture of the service is simple and scaleable.

![img.png](docs/architecture.svg)

The service can be scaled horizontally through replication, and the two data-stores by sharding.

# Testing Methodology

Testing is thorough and exists at 5 different levels:

- Unit tests
- Integration tests
- API Tests
- Benchmarks
- Load/Performance Tests

Each level serves a different purpose and focusses on a different aspect of the application.

## Unit Tests

Compared to some solutions, there aren't that many of these. That's simple because the bulk of the logic for this application is contained within two areas:

- Mapping
- Calculating Prices

The majority of the rest of the application logic is basically just calling framework methods to move data around. If we were to unit test that too, it would make the solution far more inextensible.

The test-data-generator for the price calculation tests is fairly interesting. Admittedly it's probably a little overboard but I believe it's mathematically rigourous and captures a wide number of cases.

It essentially boils down to calculating random coefficients for both order and a base sale amount such that the order will always be satisfiable with a given number of orders and at a given price without essentially reimplementing the calculation used in the implementation.


## Integration Tests

The primary downstream interaction of this application is with the Mongo repository. Whilst I could have mocked this connection, I think it is more useful to verify that an actual MongoDB functions with our queries the way we would expect.

As such, I've used `EphemeralMongo6` to spin up a temporary Mongo DB which all of the tests are run against.

The primary purpose of this test level is to ensure that we can read, write and query the database as we would expect.

Admittedly, there could probably be some more tests focussed around the read caching, but I've not had time to implement them.

## API Tests

These tests verify the entire functionality of the application from a black-box perspective.

The tests can only interact with the exposed gRPC interface of the service and have no knowledge of the internal state.

These test:

- Data Validation
- Exception Tolerance
- Idempotency
- Price Calculations
- Add, Remove, and Update Orders

These tests must be run against real MongoDB and Redis instances. `docker-compose-dev.yml` stands up the relevant dependencies, all connected to localhost.

## Benchmarks

This test has one purpose, it uses `DotnetBenchmark` to evaluate the speed of the Price Calculations in the absence of any external factors.

If I were to spend some time optimising the algorithm for speed, this is the test I would use to guide me.

## Load/Performance Tests

The benchmark is great for looking at that calculation in isolation. However, it tells us nothing about the speed of the whole system.

This is where the performance tests come in.

These tests use `k6` to place a high parallel load on the service, monitoring the average response-time.

There is a test for each of the service endpoints.

### Test Method

#### GetPrice

For this test, the MongoDB was seeded with a set of 100 orders. 

50 of these orders would be required to complete a 50,000 unit order.

This test should see significant improvements from the caching as the document is invalidated after a minute, or on a write operation.

#### OtherEndpoints

Other endpoints are simply spammed with valid requests and the time monitored, since there's no real difference in the execution complexity of any given request.

Caching will also not play much of a role in these as they are all write requests.

# Performance Testing Results

It should be noted that I ran these tests on my local machine. 

As such, the `k6` workers will have been interfering with the test, as would everything else running on my machine.

Furthermore, the latency of communicating between the Mongo and Redis servers is much lower than it might be.

It would have been better to run these on separate machines entirely, but I've not got the hardware available.

As such, I'd take the results with a grain of salt.

## Get Price Tests

| Caching Enabled     | Avg (ms) | Min (ms) | Med (ms) | Max (ms) | p(90) (ms) | p(95) (ms) |
|-----|----------|----------|----------|----------|------------|------------| 
| No |          |          |          |          |            |            | 
| Yes | 17.18    | 0.8948   | 1.79     | 468.85   | 4.74       | 9.38       |


## Other Endpoints

Caching is enabled for all of these, however, this shouldn't have a significant impact as these are write operations.

There's a cycle test rather than a remove test because generating enough test data to handle sustained removes was impractical.

| Operation | Avg (us) | Min (us) | Med (us) | Max (us) | p(90) (us) | p(95) (us) |
|--| ------ | ----- | ----- | ---- | ---- | ---- | 
| Add Order | | | | | | | 
| Modify Order | | | | | | |
| Cycle Order | 

# Future Developments

# Notes

⚠️ This isn't finished yet, but the bulk of the functionality is now there. I'll be polishing it off tonight/tomorrow ⚠️

- If you are on Windows, make sure you have symlinks enabled in your git config or this **will not build**. No changes are needed on MacOs or Linux.
- The use of preview .NET 7 is deliberate, this is largely down to me wanting to use `required` properties and the huge [LINQ speed increases](https://devblogs.microsoft.com/dotnet/performance_improvements_in_net_7/).
