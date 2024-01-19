# Exolegend a Codingame contest

https://www.codingame.com/ide/challenge/exolegend-training

## Goal

The game is a pacman like.
On a given map with defined width and heigth, two players facing each others and need to get the highest score to win before 200 turns, or all pacs of a team died or all pellets are eated.
The player can have one or more pacs to controls (5 max).
A pellet is 1 point. There is also big pellets, which the positions are known, the value of them is 10 points.

## Strategy

First strategy to code : 

- get the map and update it with what you know.
    On the begining of a game, the entire map is given.
    So we can have a two dimensional array to store it
    ```
    ###############################
    ###   #   # # ### # #   #   ###
    ### ##### # # ### # # ##### ###
            # #     # #          
    ### ### ### ### ### ### ### ###
    # # #                     # # #
    # # # ### # # ### # # ### # # #
    # #       # #     # #       # #
    # # # ##### ### ### ##### # # #
    #                             #
    ##### # # ### # # ### # # #####
    ###############################
    ```
    We can store at the begining, a pellet on each spaces.
    We also have the position of the big pellets on the map.
    And we have the positions of our pacs.    
    ```
    ###############################
    ###**O#P**#*#*###*#*#*O*#***###
    ###*#####*#*#*###*#*#*#####*###
    ********#*#*****#*#********P***
    ###*###*###*###*###*###*###*###
    #*#*#*********O***********#*#*#
    #*#*#*###*#*#*###*#*# ###*#*#*#
    #*#*******#*#*****#*#*******#*#
    #*#*#*#####*###*###*#####*#*#*#
    #*P*****O*********************#
    #####*#*#*###*#*#*###*#*#O#####
    ###############################
    ```
    ```
    # is a wall
    * is a pellet
    O is a big pellet
    P is one of our pac
    E can be enemy pac when an enemy show off.
    ```

    For each turn, we should determinate the shortest path for our pacs to reach a big pellet.
    Then we will complete this strategy !

    Ok, let start working with A* algorithm.