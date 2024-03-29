
![BoxCode - A wild take on the pig pen cipher.](mesg1.png)
<p align="center">
<img 
    style="display: block; 
           margin-left: auto;
           margin-right: auto;
           width: 33%;"    
    src="center_block.png" 
    alt="Our logo">
</img>
</p>

![BoxCode - A wild take on the pig pen cipher.](title.png)

# Box Code

## Table of Contents

- [About](#about)
- [Getting Started](#getting_started)
- [Usage](#usage)


## About <a name = "about"></a>

A much wilder take on the pigpen code.

## Getting Started <a name = "getting_started"></a>

These instructions will get you a copy of the project up and running on your local machine for development and testing purposes. See [deployment](#deployment) for notes on how to deploy the project on a live system.

### Prerequisites

What things you need to install the software and how to install them.

```
* .NET 8.0 SDK (Required, Framework Highly Recommended)

* VSCode with .Net Related extensions Or familiarity with dotnet commandline tools.
* Alternatively you can use Visual Studio 2022.

```

### Installing

Create folder where you would like to install this app and then run

These instructions are assuming you have your command line prompt current working directory in the root project folder.
Windows

Alternatively, if you have dotnet sdk 8.0 installed (You should check prereqs).

```
dotnet publish -r YOURPLATFORM 
mkdir ~/dist
cp -r bin/Release/net8.0/win-x64/publish ~/dist
mkdir ~/.local/bin
ln -s ~/dist/BoxCode ~/bin/BoxCode
```
YOURPLATFORM Could be: win-x64, linux-x64, osx,x64, osx-arm64 ...

End with an example of getting some data out of the system or using it for a little demo.

## Usage <a name = "usage"></a>

```
BoxCode OPERATION input output
OPERATION
-e Encode a text into a image. Squared.
-e1 Encode a text into a image. Single Line.
-d Decode a text from a image.
```
