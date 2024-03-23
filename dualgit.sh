#!/usr/bin/env bash

# Boolean convention: 0 for true, 1 for false

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

no_workflow_in_progress="No dualgit workflow is in progress. Please call \"dualgit init\"."

if [ "$1" == "set" ]
then
    usage="Usage: \"dualgit set <key> <value>\""
    second_param_usage="\"dualgit set\" should be called with param \"feature\" or \"refactor\"."

    if [ $is_initialized -eq 1 ]
    then
        echo "$no_workflow_in_progress" >&2
        exit 1
    fi

    if [ $# -ne 3 ]
    then
        echo "$usage" >&2
        exit 1
    fi

    if [ ! -v "base" ]
    then
        echo "A dualgit workflow is running, but \"base\" is not set." >&2
        echo "This should not happen." >&2
        exit 1
    fi

    if [ "$2" == "feature" ]
    then 
        feature="$3"
        to_check="$3"
    elif [ "$2" == "refactor" ]
    then 
        refactor="$3"
        to_check="$3"
    else
        echo "$second_param_usage" >&2
        exit 1
    fi

    if git rev-parse --verify "$to_check" >/dev/null 2>&1
    then
        if ! git merge-base --is-ancestor "$base" "$to_check"
        then
            echo "$2 is not a descendant of base object $base." >&2
            exit 1
        fi
    else
        git branch "$to_check"
    fi

    write_config
fi
