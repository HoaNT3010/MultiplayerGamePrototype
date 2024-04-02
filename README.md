# MultiplayerGamePrototype

Simple prototype for a 3D multiplayer game using Unity's Netcode for gameobjects, Lobby Service and Relay Service.

This project is a small game that I made while trying to learn more about multiplayer in unity. The programmings in this project is quite basic for the new technologies/services as I trying to understand the fundalmentals.

Alongside new technologies/services, I also studied and implemented some design pattern for this project.

The multiplayer model applied for this project is Client-Service P2P.

## Features

* Authenticate player anonymously
* Browse and join available game lobbies
* Create new lobbies (Only the Host option in Hostmode dropdown is implemented)
* Connect to the playground when the game is started

### Technologies/Services Used

I had the oppoturnity to learn and understand these new Unity's technologies/services and using them to develop this project:
* Netcode for gameobjects: For communicating and transport data between server and client on network
* Lobby service: Manage browsing, creating and joining lobbies
* Relay service: Intermediator between players

### Patterns Implemented

This is some of the design pattern that I learned and utilized in this project:
* Finite state machine: For managing player's game character behaviours
* Observer pattern: Handle communication between gameobjects, UI,...
* Singleton: Managing essential managers


## Resources and Acknowledgments

Here are some of the guides and video tutorials I used to develop this project:
* Netcode for gameobjects:
  * https://docs-multiplayer.unity3d.com/netcode/current/about/
  * https://youtu.be/3yuBOB3VrCk?si=YWq0xIcVY5yjWRoc
* Lobby Service:
  * https://youtu.be/-KDlEBfCBiU?si=uawHchI5aeB7u5uG
* Relay Service:
  * https://youtu.be/msPNJ2cxWfw?si=5_76f9CpU_REwije
* Finite State Machine:
  * https://youtu.be/qsIiFsddGV4?si=KJNtnKTCZ4G2QZm1
  * https://youtu.be/N3jOOm--TTg?si=98pjxyO5WNFqMq_W

