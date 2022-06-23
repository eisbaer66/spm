#!/bin/bash
set -euo pipefail
IFS=$'\n\t'

LEVEL=${2:-0}
re='^[0-9]+$'
if ! [[ $LEVEL =~ $re ]] ; then
   echo "error: verbosity is not a number" >&2; 
   exit 1
fi
if [[ $LEVEL -gt 4 ]]; then
   echo "error: verbosity is to big (max allowed: 4)" >&2; 
   exit 2
fi
if [[ $LEVEL -lt 0 ]]; then
   echo "error: verbosity is to small (min allowed: 0)" >&2; 
   exit 3
fi

V=
for ((i = 0 ; i < $LEVEL ; i++)); do
  V="v$V"
done
if [[ $LEVEL -ne 0 ]]; then
  V="-$V"
fi

spm $V --working-directory ${3:-.} --github-token "${4:-}" $1 --github-action
