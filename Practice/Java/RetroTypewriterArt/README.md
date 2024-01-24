# Objectif
Back in the day, people had fun turning "recipes" into surprise images using typewriters.

Use the provided recipe to create a recognizable image.

Chunks in the recipe are separated by a space.
Each chunk will tell you either
nl meaning NewLine (aka Carriage Return)
~or~
how many of the character and what character

For example:
4z means zzzz
1{ means {
10= means ==========
5bS means \\\\\ (see Abbreviations list below)
27 means 77
123 means 333333333333
(If a chunk is composed only of numbers, the character is the last digit.)

So if part of the recipe is
2* 15sp 1x 4sQ nl
...that tells you to show
**               x''''
and then go to a new line.


Abbreviations used:
sp = space
bS = backSlash \
sQ = singleQuote '
and
nl = NewLine




Sources/references:
https://asciiart.cc
https://loriemerson.net/2013/01/18/d-i-y-typewriter-art/
https://www.youtube.com/watch?v=kyK5WvpFxqo

## Entrée
string recipe

## Sortie
string (multiple lines) to show the image the recipe creates

## Contraintes
5 ≤ Length of recipe ≤ 1000
There won't be any double quotes (") in the recipe
recipe will contain at least 1 nl
## Exemple
### Entrée
```
    1sp 1/ 1bS 1_ 1/ 1bS nl 1( 1sp 1o 1. 1o 1sp 1) nl 1sp 1> 1sp 1^ 1sp 1< nl 2sp 3|
```
### Sortie
```
     /\_/\
    ( o.o )
     > ^ <
      |||
```