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

check_is_set() {
    if [[ ! -v "$1" ]]
    then
        echo "$2" >&2
        exit 1
    fi
}

check_feature_set() {
    check_is_set "feature" "Feature branch is not set.\nPlease call \"dualgit set feature <feature branch>\"."
}

check_refactor_set() {
    check_is_set "refactor" "Refactor branch is not set.\nPlease call \"dualgit set refactor <refactor branch>\"."
}

check_branches_set() {
    check_feature_set
    check_refactor_set
}


is_initialized=1

if [ -f .dualgit ]
then is_initialized=0
fi

if [ $is_initialized -eq 0 ]
then source .dualgit
fi


check_is_initialized() {
    if [ $is_initialized -ne 0 ]
    then
        echo "No dualgit workflow is in progress. Please call \"dualgit init\"." >&2
        exit 1
    fi
}


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

if [ "$1" == "set" ]
then
    usage="Usage: \"dualgit set <key> <value>\""
    second_param_usage="\"dualgit set\" should be called with param \"feature\" or \"refactor\"."

    check_is_initialized

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

if [ "$1" == "commit" ]
then
    check_is_initialized

    if [ $# -lt 2 ]
    then
        echo "Usage: dualgit commit [feature|refactor] <commit args>"
        exit 0
    fi

    current_branch="$(git branch --show-current)"

    if [ "$2" == "feature" ]
    then
        check_feature_set
        
        target="$feature"
    elif [ "$2" == "refactor" ]
    then
        check_refactor_set
        
        target="$refactor"
    fi

    if [ "$target" != "$current_branch" ]
    then smart_checkout "$target"
    fi

    git commit "${@:3}"
fi
