# SonicStreamer
Listen to your own music with SonicStreamer on Windows 10.   
[Subsonic](http://www.subsonic.org/) is a media streamer where you can get access to music which is stored on a server. Instead using the web browser you can use SonicStreamer as a Client App. Just connect the App to a Subsonic server and you will be able to stream all the music from this server.

Please keep in mind that you probably need a Premium License to use the online functionalities. For more information about Subsonic and how to set up a server please visit the [Subsonic homepage](http://www.subsonic.org/pages/premium.jsp/).

## Architecture
The whole app uses the [Model–view–view-model (MVVM) pattern](https://en.wikipedia.org/wiki/Model%E2%80%93view%E2%80%93viewmodel) as usual for Universal Windows Platform (UWP) Apps. Beside the three levels Model, ViewModel, View there is a fourth level called "back end". This level takes over the whole communication to external server like Subsonic and its derivates or other services like last.fm or MusicBrainz.