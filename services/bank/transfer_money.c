#include <stdlib.h>
#include <stdio.h>
#include <string.h>

#include "dict/dict.h"
#include "common.h"
#include "cgi.h"

int check_args(char* login, char* account, char* amount, char* login_to, char* account_to) {
    if (!login) {
        printf("%s\n", "Error: No login");
        return -1;
    }

    if (!account) {
        printf("%s\n", "Error: No account");
        return -1;
    }

    if (!amount) {
        printf("%s\n", "Error: No amount");
        return -1;
    }

    if (!login_to) {
        printf("%s\n", "Error: No dest login");
        return -1;
    }

    if (!account_to) {
        printf("%s\n", "Error: No dest account");
        return -1;
    }

    if (!login_good(login)) {
        printf("%s\n", "Error: Bad login");
        return -1;
    }

    if (!login_good(login_to)) {
        printf("%s\n", "Error: Bad dest login");
        return -1;
    }

    if (!account_good(account)) {
        printf("%s\n", "Error: Bad account");
        return 1;
    }
    if (!account_good(account_to)) {
        printf("%s\n", "Error: Bad dest account");
        return 1;
    }

}

int main() {
    printf("Content-type: text/html\n\n");
    printf("<head><title>add_money</title><head>\n");

    char* login = cgigetval("login");
    char* account = cgigetval("account");
    char* amount = cgigetval("amount");
    unsigned long amount_long;

    char* login_to = cgigetval("login_to");
    char* account_to = cgigetval("account_to");

    if (check_args(login, account, amount, login_to, account_to) == -1) {
        return 1;
    }

    amount_long = strtoul(amount, 0, 10);
    if (amount_long == 0) {
        printf("%s\n", "Error: Bad amount");
        return 1;        
    }

    struct dict t_from, to;

    if(new_dict(login, &t_from) == -1) {
        printf("Internal error\n");
        return 1;
    }

    unsigned long curr_balance = t_from.get(account);
    if(curr_balance < amount_long) {
        printf("%s\n", "Error: No money");
        return 1;
    }

    struct dict t_to;
    if(new_dict(login_to, &t_to) == -1) {
        printf("Internal error\n");
        return 1;
    }

    t_from.set(account, curr_balance - amount_long);
    t_to.set(account_to, t_to.get(account_to) + amount_long);

    printf("Successful<br>\n");
    printf("<a href='account.cgi?login=%s'>Go Back</a>\n", login);

    return 0;
}