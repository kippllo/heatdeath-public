#!/bin/bash
#
# ===SETUP===
# sudo chmod +x startServer.sh
#
# sudo nano /etc/rc.local
# Put this at the bottom:
# /home/pi/Desktop/Backend/startServer.sh &


# Run in a function so we can use "cd"!
function start(){
    cd "/home/pi/Desktop/linux-arm/publish"
    ./BackendServer
}


#This calls the function!
start