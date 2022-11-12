#!/bin/bash
ftest () {
    if [[ "$2" == "$3" ]]; then
        printf "#$1 OK\n"
    else
        printf "#$1 ERR: Expected $2, got $3\n"
    fi
}


ftest 1 "2" "$(./publish-output/amcli eval "1 + 1")"
ftest 2 "2 * x" "$(./publish-output/amcli diff "x" "1 + x2")"
ftest 3 "2 * x * sqrt(3)" "$(./publish-output/amcli diff "x" "1 + x2 * sqrt(3)")"
ftest 4 "[0; +oo)" "$(./publish-output/amcli solve "x" "x >= 0")"
ftest 5 "2" "$(echo "1 + 1" | ./publish-output/amcli eval)"
ftest 6 "cos(x)" "$(echo "sin(x)" | ./publish-output/amcli diff "x")"
