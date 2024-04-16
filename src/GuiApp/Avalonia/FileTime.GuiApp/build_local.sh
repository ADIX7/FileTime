#!/bin/sh

dotnet publish -c Release /p:DefineConstants=VERBOSE_LOGGING
