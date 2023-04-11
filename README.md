# LiteBrokerNet

## Prerequisets

## Setup

```
git clone https://github.com/soltys/LiteBrokerNet

cd LiteBrokerNet
git submodule update --init --recursive
mkdir _Result
mkdir _Result/NuGet

dotnet tool restore
dotnet cake
```