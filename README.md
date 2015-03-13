# InsireBot
## Describtion
InsireBot is a desktop application designed to connect to a given irc network and accept youtube 
links which then can be played locally on a specified audio-outputdevice. 
This works especially well in twitch.tv environment where the chat is a based on the irc protocol.

## Features

- select the audiodevice to play requests on
- select the irc/twitch channel to join in
- Blacklist(user, youtubevideo, keyword)
- custom bot-commands
- syncing your playlist to your youtube account
- take requests via irc/ twitch chat (works for single videos and whole playlists)
- paste your playlist to pastebin

## Commands
### User

- !request (youtube url of a song or playlist)
- !playlist
- !streamstats, !stats
- !checkuser (user)
- !checksong (song)
- !checkkeyword (keyword)
- !voteskip 

### Moderator only

- !removesong (keyword/url)
- !banuser (user)
- !bansong (song)
- !bankeyword (keyword)
- !unbanuser (user)
- !unbansong (song)
- !unbankeyword (keyword>
- !skip 

### Operator only

- !forceplay (url) or !force (url)
- !banuser (user, mod)
- !unbanuser (user, mod) 
