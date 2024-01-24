#!/usr/bin/env bash

find . -type d | grep --invert '\.git' | while read dir
do
    (
        (git -C $dir submodule init 2>/dev/null) \
        && (git -C $dir submodule update 2>/dev/null)
    ) || echo $dir does not contain submodules
done
