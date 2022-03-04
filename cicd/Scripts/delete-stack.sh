#!/bin/bash

set -eo pipefail

resourcesToRetain=$(echo $@ | xargs)
stackName=brighid-core-commands
command="aws cloudformation delete-stack --stack-name $stackName"

if [ "$resourcesToRetain" != "" ]; then
    command="$command --resources-to-retain $resourcesToRetain"
fi

$command
aws cloudformation wait stack-delete-complete --stack-name $stackName