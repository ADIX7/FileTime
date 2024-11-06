#!/bin/sh

dotnet publish -c Release /p:DefineConstants=VERBOSE_LOGGING -r linux-musl-x64 -p:PublishSingleFile=true --self-contained true
