#!/bin/sh
xbuild KopiLua.sln /p:Configuration=Release
cd tests/
nunit-console KopiLua.Tests.dll
