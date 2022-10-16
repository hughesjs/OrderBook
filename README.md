# OrderBook
High-performance trading order-book

----
[![GitHub Workflow Status](https://img.shields.io/github/workflow/status/hughesjs/OrderBook/.NET%20Continuous%20Integration?label=BUILD%20CI&style=for-the-badge)](https://github.com/hughesjs/dotnet-6-ci-cd-template/actions)
![GitHub top language](https://img.shields.io/github/languages/top/hughesjs/OrderBook?style=for-the-badge)
[![GitHub](https://img.shields.io/github/license/hughesjs/OrderBook?style=for-the-badge)](LICENSE)
![FTB](https://raw.githubusercontent.com/hughesjs/custom-badges/master/made-in/made-in-scotland.svg)

# Introduction

# Design Assumptions

# Architectural Summary

# Testing Methodology

# Performance Testing Results

## Get Price Tests
100 orders in the table
50 of them required to make a 50k order
grpc_req_duration....: avg=17.54ms min=892.73µs med=1.62ms max=465.69ms p(90)=2.81ms p(95)=9.96ms

# Future Developments

# Notes

⚠️ This isn't finished yet, but the bulk of the functionality is now there. I'll be polishing it off tonight/tomorrow ⚠️

- If you are on Windows, make sure you have symlinks enabled in your git config or this **will not build**. No changes are needed on MacOs or Linux.
- The use of preview .NET 7 is deliberate, this is largely down to me wanting to use `required` properties and the huge [LINQ speed increases](https://devblogs.microsoft.com/dotnet/performance_improvements_in_net_7/).
