#!/usr/bin/env bash

config_vars=(
    "base"
    "feature"
    "refactor"
    "split_commits"
)

print_config() {
    for var in "${config_vars[@]}"
    do
        declare -n var_ref=$var
        if [[ -v var_ref ]]
        then echo "$var=${var_ref}"
        fi
    done
}

write_config () {
    print_config > .dualgit
}

# 0 -> true
# 1 -> false
is_initialized=1

if [ -f .dualgit ]
then is_initialized=0
fi

if [ $is_initialized -eq 0 ]
then source .dualgit
fi

if [ $# -lt 1 ]
then
    echo "Valid commands: status, init, set, commit, update, reset, switch"
    exit 0
fi

if [ "$1" == "status" ]
then
    if [ $# -gt 1 ]
    then
        echo "\"dualgit status\" takes no arguments." >&2
        exit 1
    fi
    
    if [ $is_initialized -eq 0 ]
    then
        cat .dualgit
    else
        echo "No dualgit workflow is in progress."
    fi

    exit 0
fi

if [ "$1" == "init" ]
then
    if [ $is_initialized ]
    then
        echo "Another dualgit workflow is already running." >&2
        exit 1
    fi

    if [ $# -gt 2 ]
    then
        echo "Usage: dualgit init [base_commit]" >&2
        exit 1
    fi

    if [ $# -eq 2 ]
    then current_commit="$2"
    else
        current_commit="$(git rev-parse HEAD)"
    fi
    
    echo "base=\"$current_commit\"" > .dualgit
    exit 0
fi
