# Tic-Tac-Toe with AI & Multiplayer

![Platform](https://img.shields.io/badge/platform-Windows-blue.svg) ![Framework](https://img.shields.io/badge/framework-WPF_.NET-blue.svg)

## Table of Contents
- [Introduction](#introduction)
- [Features](#features)
- [Screenshots](#screenshots)
- [Prerequisites](#prerequisites)
- [Usage](#usage)
- [Acknowledgements](#acknowledgements)
- [Contact](#contact)

## Introduction

This WPF Tic-Tac-Toe game offers both single-player and multiplayer modes, allowing users to play against AI opponents with three difficulty levels or connect to a local network server to compete against others. This simple game provides a challenging AI experience and a social aspect with real-time multiplayer functionality.

## Features

Here are the main features of your WPF Tic-Tac-Toe app:
- AI Opponents using Mini-max Algo: Play against intelligent AI with three levels of difficulty-easy, medium, and hard-providing a challenging experience for all skill levels.
- Local Network Multiplayer: Join a local network server to compete against other players in real-time, adding a social and competitive element to the game.

## Screenshots

<img src="https://github.com/user-attachments/assets/86849756-1864-42ab-8282-c414159dbc10" alt="Framework" width="500"/>

<img src="https://github.com/user-attachments/assets/228e7acb-6ec6-42e4-bcc1-1f1515860227" alt="Framework" width="500"/>


## Prerequisites

- **.NET 7**
  - Required to run the WPF application.
- **Visual Studio 2022 or later**
- **NuGet Packages:**
  - Newtonsoft.json

## Usage

This WPF Tic-Tac-Toe game allows users to engage in both single-player mode and multiplayer, using TCP/IP Connection. 

To play singleplayer, a user can select the level of difficulty to play against an AI opponent. The hardest level uses the Mini-max algorithm, to backtrack and find the best move against the user.

To play multiplayer, users may run a server over their local network for others to join. There is a separate project in the solution to run a server on your pc over the local network. Given the local ip address, users can enter ip and username to join the server and challenge other users in real-time.

## Acknowledgements

Resources:
- [Minimax Algo](https://www.geeksforgeeks.org/finding-optimal-move-in-tic-tac-toe-using-minimax-algorithm-in-game-theory/)

## Contact

- Email: rj.lorenzo12@gmail.com
- GitHub: [richard-gits-it](https://github.com/richard-gits-it)
- LinkedIn: [Richard Lorenzo](https://www.linkedin.com/in/rj-lorenzo/)
