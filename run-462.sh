#! /usr/bin/env bash
set -uvx
set -e
cd "$(dirname "$0")"
cwd=`pwd`
ts=`date "+%Y.%m%d.%H%M.%S"`
cd EasyObjectClassic.Demo
dotnet run --project EasyObjectClassic.Demo.csproj --framework net462 "$@"
