# <img src="https://i.imgur.com/0b8fFSE.png" width="36"> windynworkspaces
[![download](https://img.shields.io/badge/download-b3eade)](https://github.com/logonoff/windynworkspaces/releases)

implements that one GNOME feature where you can have exactly one empty virtual desktop at all times

## Command line arguments
- `-silent`: start in the background
- `-cooldown=<cooldown in ms>`: the maximum rate at which windynworkspaces will try to run in milliseconds (default 300)

## Technical details
upon opening / closing a window, find any empty workspaces (virtual desktops with no windows on em), delete all but one. create one if there are none.

## Contributing
please do, i have no idea what i'm doing

### credits:
- [@spitfirex86](https://github.com/spitfirex86) - ui
- [@michalpv](https://github.com/michalpv) - emotional support and api help

## License
MIT. do whatever you want with this, but please send patches over if you make it better
