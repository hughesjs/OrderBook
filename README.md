# OrderBook
High-performance trading order-book

----
[![GitHub Workflow Status](https://img.shields.io/github/workflow/status/hughesjs/OrderBook/.NET%20Continuous%20Integration?label=BUILD%20CI&style=for-the-badge)](https://github.com/hughesjs/dotnet-6-ci-cd-template/actions)
![GitHub top language](https://img.shields.io/github/languages/top/hughesjs/OrderBook?style=for-the-badge)
[![GitHub](https://img.shields.io/github/license/hughesjs/OrderBook?style=for-the-badge)](LICENSE)
![FTB](https://raw.githubusercontent.com/hughesjs/custom-badges/master/made-in/made-in-scotland.svg)


# Notes

- A lot of the fixture code surrounding the gRPC testing infrastructure was taken from the gRPC author's example code. 
The license notices clearly indicate where this is true. This is the integration testing methodology [recommended by Microsoft for gRPC](https://learn.microsoft.com/en-us/aspnet/core/grpc/test-services?view=aspnetcore-6.0).
- If you are on Windows, make sure you have symlinks enabled in your git config or this **will not build**. No changes are needed on MacOs or Linux.


