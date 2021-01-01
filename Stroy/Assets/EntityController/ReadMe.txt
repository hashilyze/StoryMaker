# Entitys

# Summary
Control action of character, make it easy to interact with others

# Description
Every character has its own action.
So, although we can implement the character-component for some characters,
what implement character-component for all characters is very difficult.

For this reason, this package serve 'EntityController'.
EntityController have base action and interaction with rigidbody apllied all character.
For example, If Player having ChracterController collide to wall,
player and dosen't through wall and stop in front of it.
Base action consist of simple actions such as move, teleport, but these are very powerful.
If you want other actions, can implement by using these.


# Component
EntityController: Base action and interaction
EntityManager: 
